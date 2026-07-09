using Microsoft.AspNetCore.Mvc;
using Piwonka.CC.Services;

namespace Piwonka.CC.Controllers.Api
{
    [ApiController]
    [Route("api/admin")]
    public class AdminImageController : ControllerBase
    {
        private static readonly string[] AllowedExtensions =
            { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg", ".bmp" };

        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        private readonly FileUploadService _uploadService;

        public AdminImageController(FileUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        [HttpPost("upload-image")]
        [RequestSizeLimit(MaxFileSize)]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (HttpContext.Session.GetString("IsAuthenticated") != "true")
            {
                return Unauthorized(new { error = new { message = "Nicht authentifiziert." } });
            }

            // TinyMCE postet das File unter dem Feldnamen "file"; fallback auf erstes File.
            if (file == null && Request.Form.Files.Count > 0)
            {
                file = Request.Form.Files[0];
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = new { message = "Keine Datei übertragen." } });
            }

            if (file.Length > MaxFileSize)
            {
                return BadRequest(new { error = new { message = "Datei zu groß (max. 10 MB)." } });
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || Array.IndexOf(AllowedExtensions, ext) < 0)
            {
                return BadRequest(new { error = new { message = "Dateityp nicht erlaubt." } });
            }

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = new { message = "Datei ist kein Bild." } });
            }

            var path = await _uploadService.UploadImageAsync(file);
            if (string.IsNullOrEmpty(path))
            {
                return StatusCode(500, new { error = new { message = "Upload fehlgeschlagen." } });
            }

            // TinyMCE erwartet { location: "..." }
            return Ok(new { location = path });
        }
    }
}
