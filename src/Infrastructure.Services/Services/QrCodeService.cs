using Domain.Interfaces.Interfaces.Infrastructure;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace Infrastructure.Services.Services
{
    public class QrCodeService : IQrCodeService
    {
        public Bitmap GenerateQrWithLogo(string data, string logoPath)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.H);
            var qrBytes = new PngByteQRCode(qrData).GetGraphic(20);

            using var tempQr = new Bitmap(new MemoryStream(qrBytes));
            var qr = new Bitmap(tempQr.Width, tempQr.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(qr)) g.DrawImage(tempQr, 0, 0);

            if (!File.Exists(logoPath))
                throw new Exception($"Logo not found at {logoPath}");

            using var logo = new Bitmap(Image.FromFile(logoPath));
            int size = (int)(qr.Width * 0.2);
            using var resizedLogo = new Bitmap(logo, new Size(size, size));
            using var qrG = Graphics.FromImage(qr);
            qrG.DrawImage(resizedLogo, (qr.Width - size) / 2, (qr.Height - size) / 2);

            return qr;
        }
    }
}