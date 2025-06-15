// Controllers/Api/CookieController.cs
using Microsoft.AspNetCore.Mvc;
using Piwonka.CC.Services;

namespace Piwonka.CC.Controllers.Api
{
    [ApiController]
    [Route("api")]
    public class CookieController : ControllerBase
    {
        private readonly ISimpleCookieService _cookieService;

        public CookieController(ISimpleCookieService cookieService)
        {
            _cookieService = cookieService;
        }

        [HttpPost("accept-cookies")]
        public IActionResult AcceptCookies()
        {
            _cookieService.AcceptCookies(HttpContext);
            return Ok(new { success = true });
        }

        [HttpGet("cookie-status")]
        public IActionResult GetCookieStatus()
        {
            var hasAccepted = _cookieService.HasAcceptedCookies(HttpContext);
            return Ok(new { accepted = hasAccepted });
        }
    }
}