namespace Domain.Interfaces.Interfaces.Application
{
    public interface IDigitalSignatureService
    {
        Task<string> ProcessPdfAsync(Stream fileStream, string webRootPath, string scheme, string host);
    }
}
