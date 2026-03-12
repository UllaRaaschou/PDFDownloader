using static DownloadPreparer;

namespace PDFDownloader.Services
{
    public interface IDownloadService
    {
        Task DownloadAsync(string url, string downloadFolder);
    }
}
