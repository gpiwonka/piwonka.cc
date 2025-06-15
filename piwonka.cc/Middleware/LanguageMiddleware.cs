using Piwonka.CC.Services;

namespace Piwonka.CC.Middleware
{
    public class LanguageMiddleware
    {
        private readonly RequestDelegate _next;

        public LanguageMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILanguageService languageService)
        {
            // Language Switcher über Query Parameter
            if (context.Request.Query.TryGetValue("lang", out var langParam))
            {
                var requestedLanguage = langParam.ToString().ToLower();

                // Nur gültige Sprachen akzeptieren
                if (requestedLanguage == "de" || requestedLanguage == "en")
                {
                    await languageService.SetCurrentLanguageAsync(requestedLanguage);
                }

                // Redirect ohne lang Parameter um saubere URLs zu haben
                var pathAndQuery = context.Request.Path.ToString();
                var queryString = context.Request.QueryString.ToString();

                if (!string.IsNullOrEmpty(queryString))
                {
                    // lang Parameter entfernen
                    var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(queryString);
                    queryParams.Remove("lang");

                    if (queryParams.Any())
                    {
                        pathAndQuery += "?" + string.Join("&", queryParams.Select(x => $"{x.Key}={x.Value}"));
                    }
                }

                context.Response.Redirect(pathAndQuery);
                return;
            }

            await _next(context);
        }
    }
}