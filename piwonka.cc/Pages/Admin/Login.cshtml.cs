// Pages/Admin/Login.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace Piwonka.CC.Pages.Admin
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // Wenn bereits eingeloggt, direkt zum Admin-Bereich weiterleiten
            if (HttpContext.Session.GetString("IsAuthenticated") == "true")
            {
                return RedirectToPage("/Admin/Index");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            var adminUsername = _configuration["AdminCredentials:Username"];
            var adminPassword = _configuration["AdminCredentials:Password"];

            if (Username == adminUsername && Password == adminPassword)
            {
                // Session-Variable setzen, um den Login-Status zu speichern
                HttpContext.Session.SetString("IsAuthenticated", "true");

                return RedirectToPage("/Admin/Index");
            }
            else
            {
                ErrorMessage = "Ungültiger Benutzername oder Passwort.";
                return Page();
            }
        }
    }
}