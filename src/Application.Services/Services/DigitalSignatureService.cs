using Domain.Interfaces.Interfaces.Application;
using Domain.Interfaces.Interfaces.Infrastructure;

namespace Application.Services.Services
{
    public class DigitalSignatureService : IDigitalSignatureService
    {
        private readonly IQrCodeService _qrCodeService;
        private readonly IPdfProcessingService _pdfProcessingService;

        public DigitalSignatureService(IQrCodeService qrCodeService, IPdfProcessingService pdfProcessingService)
        {
            _qrCodeService = qrCodeService;
            _pdfProcessingService = pdfProcessingService;
        }

        public async Task<string> ProcessPdfAsync(Stream fileStream, string webRootPath, string scheme, string host)
        {
            string baseDir = Path.Combine(webRootPath, "signed");
            string fontPath = Path.Combine(webRootPath, "fonts/NotoSans-Regular.ttf");
            string logoPath = Path.Combine(webRootPath, "images/finki_logo.png");

            Directory.CreateDirectory(baseDir);

            string uid = Guid.NewGuid().ToString();
            string signedPath = Path.Combine(baseDir, $"signed_{uid}.pdf");
            string publicUrl = $"{scheme}://{host}/signed/signed_{uid}.pdf";

            using var qrImage = _qrCodeService.GenerateQrWithLogo(publicUrl, logoPath);

            _pdfProcessingService.SignAndSavePdf(fileStream, signedPath, qrImage, publicUrl, fontPath);

            return publicUrl;
        }
    }
}
