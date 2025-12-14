using System.Drawing;

namespace Domain.Interfaces.Interfaces.Infrastructure
{
    public interface IQrCodeService
    {
        Bitmap GenerateQrWithLogo(string data, string logoPath);
    }
}
