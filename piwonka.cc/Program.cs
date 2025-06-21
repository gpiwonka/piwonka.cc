using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Piwonka.CC.Data;
using Piwonka.CC.Middleware;
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

            builder.Services.AddHttpClient<IndexNowService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "piwonka.cc/1.0");
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
            builder.Services.AddHostedService<DailyAnalyticsBackgroundService>();
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
