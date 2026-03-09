using PDFDownloader.Data;
using PDFDownloader.Services;
using Serilog;

public partial class FileDownloader
{   
    
    public class httpDownloadService : IDownloadService 
    {
        private IUniversalDownloadetFiles _uni;


        public httpDownloadService(IUniversalDownloadetFiles uni)
        {
            _uni = uni;
        }

        private bool TjekForPreviousDownload(string url)
        {
            return _uni.UniDownloadedFiles.Contains(url);
        }

        public async Task DownloadAsync(string url, string downloadFolder)
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync();

            var fileName = Path.GetFileName(url);
            Directory.CreateDirectory(downloadFolder);
            var filePath = Path.Combine(downloadFolder, fileName);
            using var fileStream = File.Create(filePath);

            await stream.CopyToAsync(fileStream);

            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                Log.Information("Downloaded: {FileName}, Size: {Size} bytes", fileInfo.Name, fileInfo.Length);
            }
            else
            {
                Log.Error("Filen blev ikke oprettet: {FilePath}", filePath);
            }

            var downloadetFiles =  new List<string> {url};
            _uni.UniDownloadedFiles.AddRange(downloadetFiles);
        }

    }

    public record DownloadResult(bool Success, string? FileName, string? ErrorMessage);
    

}
