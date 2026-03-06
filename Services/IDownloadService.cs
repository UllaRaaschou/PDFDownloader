using static FileDownloader;

namespace PDFDownloader.Services
{
    public interface IDownloadService
    {
        Task<DownloadResult> DownloadAsync(string url, string downloadFolder);
    }
}
