namespace Piwonka.CC.Models
{
    /// <summary>
    /// Pro-Deployment konfigurierbares Branding. Wird aus der Config-Section "Site" gebunden.
    /// Erlaubt, dieselbe Codebasis unter mehreren Domains (z.B. piwonka.cc / plainvanilla.tech)
    /// mit eigenem Logo, Namen und Kontaktdaten zu betreiben.
    /// Die Defaults hier entsprechen piwonka.cc, damit ohne "Site"-Section nichts kaputtgeht.
    /// </summary>
    public class SiteSettings
    {
        /// <summary>Anzeigename der Seite, z.B. "piwonka.cc".</summary>
        public string Name { get; set; } = "piwonka.cc";

        /// <summary>Basis-URL ohne abschließenden Slash, z.B. "https://piwonka.cc".</summary>
        public string Url { get; set; } = "https://piwonka.cc";

        /// <summary>Wortmarke im Hero der Startseite, z.B. "PIWONKA".</summary>
        public string Wordmark { get; set; } = "PIWONKA";

        // --- Bilder (Pfade relativ zu wwwroot) ---
        public string LogoPath { get; set; } = "/images/logo.png";
        public string LogoAlt { get; set; } = "Piwonka Logo";
        public string HeroImagePath { get; set; } = "/images/piwonka.png";
        public string OgImagePath { get; set; } = "/images/piwonka.png";

        // --- Person / Betreiber ---
        public string Author { get; set; } = "Georg Piwonka";
        public string AuthorEmail { get; set; } = "georg@piwonka.cc";
        public string ContactEmail { get; set; } = "georg@piwonka.cc";
        public string JobTitle { get; set; } = "Software Developer";

        // --- Adresse (Impressum / JSON-LD) ---
        public string Street { get; set; } = "Reichsstraße 150a";
        public string Zip { get; set; } = "6800";
        public string City { get; set; } = "Feldkirch";
        /// <summary>Ländercode für JSON-LD (ISO), z.B. "AT".</summary>
        public string Country { get; set; } = "AT";
        /// <summary>Ausgeschriebener Ländername für die Anzeige, z.B. "Österreich".</summary>
        public string CountryName { get; set; } = "Österreich";

        // --- Sonstiges ---
        public string FooterTagline { get; set; } = "Baustelle seit 2000";
        public string CoffeeUrl { get; set; } = "https://paypal.me/geopiw";
        public string[] KnowsAbout { get; set; } =
        {
            "SQL Server", "Microsoft .NET", "ASP.NET Core", "Datenbankoptimierung", "Query Performance"
        };

        // --- Startseiten-SEO ---
        public string HomeTitle { get; set; } = "Georg Piwonka – Blog & SQL Tools";
        public string HomeMetaDescription { get; set; } =
            "Persönliche Website von Georg Piwonka: Blog rund um Softwareentwicklung, .NET und SQL Server sowie kostenlose SQL-Tools (Plan Advice, Query Formatter, Index Advisor, Deadlock Analyzer).";
        public string HomeMetaKeywords { get; set; } =
            "Georg Piwonka, piwonka.cc, SQL Server, .NET Blog, Plan Advice, Query Formatter, Index Advisor, Deadlock Analyzer, Nova SQL";

        // --- llms.txt ---
        public string LlmsSummary { get; set; } =
            "Persönliche Website von Georg Piwonka — Software-Entwickler aus Feldkirch, Vorarlberg (Österreich). Schwerpunkt: Microsoft .NET und SQL Server. Bietet einen Blog, kostenlose SQL-Tools und kommerzielle Software (Nova SQL).";
    }
}
