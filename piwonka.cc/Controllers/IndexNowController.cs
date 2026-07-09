using Microsoft.AspNetCore.Mvc;
using Piwonka.CC.Services;

namespace Piwonka.CC.Controllers
{
    public class IndexNowController : Controller
    {
        private readonly IndexNowService _indexNowService;
        private readonly ILogger<IndexNowController> _logger;

        public IndexNowController(IndexNowService indexNowService, ILogger<IndexNowController> logger)
        {
            _indexNowService = indexNowService;
            _logger = logger;
        }

        // IndexNow erwartet die Key-Datei unter https://<host>/<key>.txt (Root!).
        // regex-Constraint stellt sicher, dass nur Hex-Strings (mit optionalen GUID-Bindestrichen) matchen —
        // robots.txt, llms.txt etc. bleiben unberührt.
        [HttpGet("/{key:regex(^[[a-fA-F0-9-]]+$)}.txt")]
        public IActionResult GetKey(string key)
        {
            var apiKey = _indexNowService.GetApiKey();

            if (!string.Equals(key, apiKey, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("IndexNow key file requested with wrong key: {Requested}", key);
                return NotFound();
            }

            return Content(apiKey, "text/plain");
        }
    }
}