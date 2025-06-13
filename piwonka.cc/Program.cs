using Microsoft.EntityFrameworkCore;
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
            });
            builder.Services.AddHttpContextAccessor();
            // Add services to the container.
            builder.Services.AddScoped<IMenuService, MenuService>();
            builder.Services.AddScoped<FileUploadService>();

            builder.Services.AddScoped<ILanguageService, LanguageService>();
            builder.Services.AddScoped<ISearchService, SearchService>();
            
            
            builder.Services.AddRazorPages();


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

            app.UseMiddleware<LanguageMiddleware>();


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
