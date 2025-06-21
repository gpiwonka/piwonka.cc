using Microsoft.AspNetCore.Mvc;
using Piwonka.CC.Services;

namespace Piwonka.CC.Controllers
{
    [Route("[controller]")]
    public class IndexNowController : Controller
    {
        private readonly IndexNowService _indexNowService;

        public IndexNowController(IndexNowService indexNowService)
        {
            _indexNowService = indexNowService;
        }

        // Endpoint für die IndexNow Key-Datei
        [HttpGet("{key}.txt")]
        public IActionResult GetKey(string key)
        {
            var apiKey = _indexNowService.GetApiKey();

            if (key != apiKey)
            {
                return NotFound();
            }

            return Content(apiKey, "text/plain");
        }
    }
}