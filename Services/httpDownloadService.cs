using PDFDownloader.Services;

public partial class FileDownloader
{
    public class httpDownloadService : IDownloadService 
    {
        public async Task DownloadAsync(string url, string downloadFolder)
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync();

            var fileName = Path.GetFileName(url);
            var filePath = Path.Combine(downloadFolder, fileName);
            using var fileStream = File.Create(filePath);

            await stream.CopyToAsync(fileStream);
            //return new DownloadResult(true, fileName, null);
        }
    }

    public record DownloadResult(bool Success, string? FileName, string? ErrorMessage);
    

}
