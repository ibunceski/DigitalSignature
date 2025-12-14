using System.Drawing;

namespace Domain.Interfaces.Interfaces.Infrastructure
{
    public interface IPdfProcessingService
    {
        void SignAndSavePdf(Stream sourceStream, string outputPath, Bitmap qrImage, string publicUrl, string fontPath);
    }
}
