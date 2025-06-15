// Services/SimpleCookieService.cs
namespace Piwonka.CC.Services
{
    public interface ISimpleCookieService
    {
        bool ShouldShowBanner(HttpContext context);
        void AcceptCookies(HttpContext context);
        bool HasAcceptedCookies(HttpContext context);
    }

    public class SimpleCookieService : ISimpleCookieService
    {
        private const string COOKIE_CONSENT_NAME = "cookie_accepted";
        private const int COOKIE_EXPIRY_DAYS = 365;

        public bool ShouldShowBanner(HttpContext context)
        {
            return !HasAcceptedCookies(context);
        }

        public void AcceptCookies(HttpContext context)
        {
            context.Response.Cookies.Append(COOKIE_CONSENT_NAME, "true", new CookieOptions
            {
                Expires = DateTime.Now.AddDays(COOKIE_EXPIRY_DAYS),
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true
            });
        }

        public bool HasAcceptedCookies(HttpContext context)
        {
            return context.Request.Cookies.ContainsKey(COOKIE_CONSENT_NAME) &&
                   context.Request.Cookies[COOKIE_CONSENT_NAME] == "true";
        }
    }
}