# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Run the app (from solution root) — defaults to piwonka.cc branding
dotnet run --project piwonka.cc

# Run a second-domain deployment (e.g. plainvanilla.tech) with an override config file
dotnet run --project piwonka.cc -- --site-config=appsettings.plainvanilla.json

# Build
dotnet build piwonka.cc

# Publish (release)
dotnet publish piwonka.cc -c Release

# EF Core migrations (from solution root)
dotnet ef migrations add <MigrationName> --project piwonka.cc
dotnet ef database update --project piwonka.cc
```

Database migrations are auto-applied on startup in Program.cs. No test projects exist.

Development URLs: http://localhost:5111 / https://localhost:7186

## Architecture

ASP.NET Core Razor Pages app (.NET 9.0) for a bilingual (DE/EN) personal website with blog, CMS, and admin panel.

**Key layers:**
- **Pages/** — Razor Pages UI. Admin pages under `Pages/Admin/` use `AdminAuthFilter` (in `Filters/`) for session-based auth. Blog pages under `Pages/Blog/`. Top-level pages in `Pages/` include `PlanAdvice`, `PlanResult`, `Contact`, `Search`, `Robots`, `Privacy`, `Seite` (slug-based page renderer). Public `Tickets/Create` page submits support tickets. SQL tooling pages (`QueryFormatter`, `IndexAdvisor`, `DeadlockAnalyzer`, `QueryConverter`) live in `Pages/Tools/`.
- **Services/** — Business logic. Scoped registrations: `MenuService`, `FileUploadService`, `IndexNowService` (also registered via `AddHttpClient<IndexNowService>` — both the typed-client and the `IIndexNowService` interface registrations are required), `LanguageService`, `LocalizationService`, `SearchService`, `AnalyticsService`, `SimpleCookieService`, `SitemapService`. `SqlFormatterService` is singleton; `DailyAnalyticsBackgroundService` is a hosted service. `PlanAdviceService` and `DbaToolService` are typed `HttpClient` clients (`AddHttpClient<T>`) registered without a separate interface. `BlueskyService`/`IBlueskyService` exist in `Services/` but are **not registered in DI** — adding them requires a `Program.cs` registration.
- **Interfaces/** — Service abstractions (`IAnalyticsService`, `ILanguageService`, `ISearchService`); other interfaces sit alongside their implementations in `Services/`.
- **Middleware/** — `LanguageMiddleware` → `AnalyticsMiddleware` → `SitemapUpdateMiddleware` (registered via `UseSitemapUpdateNotification()`), in this order, after `UseSession()`.
- **Controllers/** — `SitemapController` (XML sitemap), `IndexNowController` (key file endpoint), `LlmsTxtController` (`/llms.txt` markdown for AI crawlers, queries `Posts`/`Seiten` directly via injected `ApplicationDbContext`), `Api/CookieController` (cookie consent API), `Api/AdminImageController` (TinyMCE image upload at `POST /api/admin/upload-image` — does its own `Session["IsAuthenticated"]` check rather than going through `AdminAuthFilter`; returns `{ location }` JSON shape TinyMCE expects).
- **Data/ApplikationDbContext.cs** — EF Core context (file name uses German spelling "Applikation"; the class is `ApplicationDbContext`). Entities: `Post`, `Seite` (pages with parent-child hierarchy, unique `Slug` index), `Kategorie`, `Analytics`, `UserSession`, `PlanAdviceResult` (unique `Hash` index for cache lookups), `Ticket`, `IndexFeature`.
- **Models/** — Entity classes plus options POCOs (`SmtpSettings`, `AnthropicSettings`, `SiteSettings`) and view models (`ContactViewModel`). Note German naming: `Seite`=Page, `Kategorie`=Category, `Inhalt`=Content, `Titel`=Title, `ErstelltAm`=CreatedAt, `IstVeroeffentlicht`=IsPublished.
- **Jobs/** — `AnalyticsJob` (logic invoked by `DailyAnalyticsBackgroundService`).
- **Validation/** — Custom validation attributes (e.g. `OptionalFileAttribute`).
- **ViewComponents/** — Reusable UI components. **ViewModels/** — DTOs for presentation.
- **Helpers/** — `HtmlHelpers`, `LocalizationHelper`, `MarkdownHelper` (hand-rolled regex-based markdown→HTML; no Markdig dependency — extend the regex chain rather than swap libraries unless intentional). **Exrensions/** — Query extensions (note: folder name has a typo, keep it when adding files).

**Database access pattern:**
Services use `IDbContextFactory<ApplicationDbContext>` (pooled) and create short-lived contexts: `using var context = _contextFactory.CreateDbContext()`. Follow this pattern when adding new services. Controllers that inject `ApplicationDbContext` directly (e.g. `LlmsTxtController`) get a scoped instance from `AddDbContextFactory`'s registered context — fine for request-scoped reads.

**Startup quirks (Program.cs):**
- An optional per-deployment config file can be layered on top of `appsettings.json` via the `--site-config=<path>` command-line arg (or `SITE_CONFIG` env var). It's read early into the `configBuilder` before `builder.Configuration` is finalized, so it wins over the base config. See "Multi-site branding" below.
- Before `Database.Migrate()` runs, a raw SQL block backfills `__EFMigrationsHistory` with four legacy migration IDs. Don't rename or remove those historical migrations without also updating the seed block.
- 404s are intercepted via `UseStatusCodePages` and redirected to `/` (no 404 page is rendered). Anything that relies on 404 responses (e.g. probing) won't see them.
- Both `AddDbContextFactory<ApplicationDbContext>` and a separate singleton `PooledDbContextFactory<ApplicationDbContext>` are registered against the same connection string — keep both in sync if the connection string source changes.

**Patterns:**
- Language is an enum (`DE=0`, `EN=1`) stored per entity and managed via session (`CurrentLanguage` key)
- Search scoring: Title exact=100, partial=50, Meta=25, Content=10
- Slug-based URLs throughout (not ID-based)
- `DailyAnalyticsBackgroundService` aggregates daily stats at 2am and cleans sessions older than 7 days
- Admin auth uses hardcoded credentials from `appsettings.json` (not ASP.NET Identity), stored in session as `IsAuthenticated`
- `PlanAdviceService` integrates Anthropic Claude API for SQL execution plan analysis (bilingual prompts)
- IndexNow notifications are fire-and-forget via `Task.Run()` in `SitemapUpdateMiddleware`

**Multi-site branding (one codebase, multiple domains):**
The app serves multiple domains (e.g. piwonka.cc and plainvanilla.tech) from a **single codebase** — no fork. Sites are isolated by **separate databases + separate deployments** of the same binary, each with its own config file; there is no per-row tenant discriminator.
- All brand-specific values live in `Models/SiteSettings.cs` (bound from the `Site` config section). The POCO's defaults equal piwonka.cc, so the app runs unchanged if no `Site` section is present. It's registered both as `IOptions<SiteSettings>` and as a plain `SiteSettings` singleton, so views and controllers can `@inject SiteSettings Site` / take a `SiteSettings` ctor param directly.
- **Do not hardcode brand strings** (site name, URL, logo/OG image paths, author, address, contact email, coffee link, home-page SEO, llms.txt summary). Add a field to `SiteSettings` and read it. Branding already flows through `_Layout.cshtml` (title, logo, OG tags, both JSON-LD blocks, footer), `Index.cshtml` (hero image, wordmark, meta), `Contact`/`Privacy`/`datenschutz`/`Seite`/`Blog/Detail` (address, email, JSON-LD), the Tools pages + `PlanAdvice` (coffee link), plus `SitemapUpdateMiddleware`, `LlmsTxtController`, `IndexNowService`, and `Contact.cshtml.cs`.
- Known intentional leftovers (cosmetic, not brand-critical): the whimsical tagline array in `Index.cshtml` and an example-URL placeholder in `Pages/Admin/IndexNow/Index.cshtml`. The `?? "piwonka.cc"` fallbacks in the admin PageModels only fire on misconfiguration (`IndexNow:Host` is set per deployment).
- `SiteSettings` is a singleton bound at startup — branding changes need a restart (normal for separate deployments).

## Configuration

Settings in `appsettings.json`: SQL Server connection string, admin credentials, IndexNow config, Anthropic API key, SMTP settings, and a `Site` branding block. Sensitive values are in the committed config (this is intentional for this project's hosting setup).

Options pattern bindings: `SmtpSettings`, `AnthropicSettings`, `SiteSettings` (bound via `builder.Services.Configure<T>()`).

**Per-deployment override files:** A file like `appsettings.plainvanilla.json` is layered on top of the base `appsettings.json` via `--site-config=<file>` (see "Multi-site branding"). It only needs to contain the **deltas** for that domain — its own `ConnectionStrings:DefaultConnection` (separate database), `IndexNow` host/key, `SmtpSettings`, `AdminCredentials`, and `Site` block; everything else (e.g. the Anthropic key) is inherited from the base config. Its DB is built on first run by the auto-migration on startup.
