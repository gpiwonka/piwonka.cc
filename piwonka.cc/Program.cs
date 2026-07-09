using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Piwonka.CC.Data;
using Piwonka.CC.Middleware;
using Piwonka.CC.Models;
using Piwonka.CC.Services;

namespace Piwonka.CC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Console.WriteLine($"Environment from builder: {builder.Environment.EnvironmentName}");

            // Configuration Step-by-Step aufbauen
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Console.WriteLine("Base config loaded");

            try
            {
                configBuilder.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
                Console.WriteLine($"Environment config loaded: appsettings.{builder.Environment.EnvironmentName}.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading environment config: {ex.Message}");
            }

            // Optionale Site-spezifische Config via Startparameter (--site-config=<pfad>)
            // oder Umgebungsvariable SITE_CONFIG. Ermöglicht, mehrere Domains (z.B. plainvanilla.tech)
            // aus derselben Binärdatei mit je eigener appsettings-Datei zu betreiben.
            var startupConfig = new ConfigurationBuilder()
                .AddCommandLine(args)
                .AddEnvironmentVariables()
                .Build();
            var siteConfigFile = startupConfig["site-config"] ?? startupConfig["SITE_CONFIG"];
            if (!string.IsNullOrWhiteSpace(siteConfigFile))
            {
                configBuilder.AddJsonFile(siteConfigFile, optional: false, reloadOnChange: true);
                Console.WriteLine($"Site config loaded: {siteConfigFile}");
            }

            var config = configBuilder.Build();
            builder.Configuration.AddConfiguration(config);

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = ".AspNetCore.Session"; // Optional: eigener Name
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });
            builder.Services.AddHttpContextAccessor();

            var siteName = builder.Configuration["Site:Name"] ?? "piwonka.cc";
            builder.Services.AddHttpClient<IndexNowService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", $"{siteName}/1.0");
            });

            // Add services to the container.
            builder.Services.AddScoped<IMenuService, MenuService>();
            builder.Services.AddScoped<FileUploadService>();
            builder.Services.AddScoped<IIndexNowService, IndexNowService>();
            builder.Services.AddScoped<ILanguageService, LanguageService>();
            builder.Services.AddScoped<ILocalizationService, LocalizationService>();
            builder.Services.AddScoped<ISearchService, SearchService>();
            builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
            builder.Services.AddScoped<ISimpleCookieService, SimpleCookieService>();
            builder.Services.AddScoped<ISitemapService, SitemapService>();
            builder.Services.AddHostedService<DailyAnalyticsBackgroundService>();

            // SMTP-Einstellungen aus appsettings.json
            builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

            // Anthropic API Einstellungen
            builder.Services.Configure<AnthropicSettings>(builder.Configuration.GetSection("Anthropic"));

            // Pro-Deployment Branding (Name, Logo, Kontaktdaten) — siehe SiteSettings
            builder.Services.Configure<SiteSettings>(builder.Configuration.GetSection("Site"));
            // Direkt injizierbar in Views/Controllern via @inject Piwonka.CC.Models.SiteSettings Site
            builder.Services.AddSingleton(sp =>
                sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SiteSettings>>().Value);
            builder.Services.AddHttpClient<PlanAdviceService>();
            builder.Services.AddHttpClient<DbaToolService>();
            builder.Services.AddSingleton<SqlFormatterService>();

            builder.Services.AddRazorPages();

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
      options.UseSqlServer(connectionString));


            builder.Services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(provider =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(connectionString);
                return new PooledDbContextFactory<ApplicationDbContext>(optionsBuilder.Options);
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();

                    // Sicherstellen, dass alte Migrations in History eingetragen sind
                    context.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250614103503_init')
                        BEGIN
                            INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES
                            ('20250614103503_init', '9.0.5'),
                            ('20250614174143_feldlaengen', '9.0.5'),
                            ('20250615081856_Analytics', '9.0.5'),
                            ('20260322100354_bild raus', '9.0.5');
                        END
                    ");

                    // Automatisch Migrationen anwenden
                    context.Database.Migrate();


                }
                catch (Exception ex)
                {

                    // Optional: App beenden, wenn Migration fehlschlägt
                    // Environment.Exit(1);
                }
            }
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStatusCodePages(async context =>
            {
                var response = context.HttpContext.Response;
                var request = context.HttpContext.Request;

                if (response.StatusCode == 404)
                {
                    // Logging der 404 Anfrage
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("404 Error - Path: {Path}, UserAgent: {UserAgent}",
                        request.Path, request.Headers.UserAgent);

                    // Weiterleitung zur Homepage
                    response.Redirect("/");
                    return;
                }

                // Für andere Status Codes normale Behandlung
                await context.Next(context.HttpContext);
            });

            app.UseHttpsRedirection();

            
            app.UseStaticFiles();

            app.UseRouting();

             app.UseAuthentication();

            app.UseAuthorization();

            app.UseSession();

            app.UseMiddleware<LanguageMiddleware>();
            app.UseMiddleware<AnalyticsMiddleware>();
            app.UseSitemapUpdateNotification();
            app.MapControllers();

            // ✅ MapRazorPages NACH UseSession
            app.MapStaticAssets();
            app.MapRazorPages().WithStaticAssets();

            app.Run();
        }
    }
}
