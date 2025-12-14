using Domain.Interfaces.Interfaces.Application;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfSigningController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IDigitalSignatureService _signatureService;

        public PdfSigningController(IWebHostEnvironment env, IDigitalSignatureService signatureService)
        {
            _env = env;
            _signatureService = signatureService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (!file.FileName.EndsWith(".pdf"))
                return BadRequest("Only PDF files allowed.");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            var signedPdfUrl = await _signatureService.ProcessPdfAsync(
                stream,
                _env.WebRootPath,
                Request.Scheme,
                Request.Host.ToString()
            );

            return Ok(new { signedPdfUrl });
        }
    }
}