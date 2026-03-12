using PDFDownloader.Data;
using PDFDownloader.Services;
using Serilog;
using System.Collections.Concurrent;

    
public class DownloadService : IDownloadService 
{
    private IUniversalDownloadetFiles _uni;
    private readonly HttpClient _httpClient;
    public ConcurrentBag<(string url, string error)> _failedDownloads;
    private readonly ConcurrentBag<string> _succeededDownloads;
    public DownloadService(IUniversalDownloadetFiles uni, HttpClient httpClient)
    {
        _uni = uni;
        _httpClient = httpClient;
        _failedDownloads = new ConcurrentBag<(string url, string error)> { };
        _succeededDownloads = new ConcurrentBag<string> { };
    }

    public async Task TryDownloadToDestination(List<URLObject> listOfURLObjects, string downloadFolder, bool wantTjeck)
    {
        var tasks = new List<Task>();

        foreach (var obj in listOfURLObjects)
        {
            tasks.Add(TryDownloadFromValidUrl(obj, downloadFolder));
        }

        await Task.WhenAll(tasks);

        var lines = _failedDownloads.Select(x => $"{x.url} - {x.error}");
        var filePath = Path.Combine(downloadFolder, "failed_downloads.txt");
        Directory.CreateDirectory(downloadFolder);
        File.WriteAllLines(filePath, lines);
        var count = Directory.EnumerateFiles(downloadFolder).Count();
    }

    public async Task DownloadAsync(string url, string downloadFolder)
    {
       
        using var response = await _httpClient.GetAsync(url);
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

    private async Task TryDownloadFromValidUrl(URLObject obj, string downloadFolder)
    {
        var url1 = obj.url1;
        var url2 = obj.url2;
        bool succes = false;
        var firstErrorType = "";
        var secondErrorType = "";

        bool firstUsedUrlFailed = false;
        var firstErrorMessage = string.Empty;

        if (!string.IsNullOrEmpty(url1))
        {
            try
            {
                await DownloadAsync(url1, downloadFolder);
                _succeededDownloads.Add(url1);
                Log.Information("Downloaded: {URL}", url1);
                succes = true;
                return;
            }
            catch (Exception ex)
            {
                firstErrorMessage = ex.Message;

                if (ex is IOException || ex.InnerException is IOException)
                {
                    firstErrorType = "IOException";
                }
                else if (ex is HttpRequestException || ex.InnerException is HttpRequestException)
                {
                    firstErrorType = "HttpRequestException";
                }
                else
                {
                    firstErrorType = ex.GetType().Name;
                }

                firstUsedUrlFailed = true;
            }

            if (succes == false && !string.IsNullOrEmpty(url2))
            {
                (succes, var _localSuccesList) = await TryUrl2(url2, downloadFolder, firstErrorType, url1, firstErrorMessage);
                if (succes)
                    foreach (var item in _localSuccesList)
                    {
                        _succeededDownloads.Add(item);
                    }
                if (!succes)
                    _failedDownloads.Add((url1, firstErrorMessage));
            }
            else if (!succes)
            {
                if (firstErrorType == "IOException")
                {
                    Log.Error("DownloadFailure {ExType}: {URL1}, Fil-fejl ved download: {URL1} - {Message}", firstErrorType, url1, firstErrorMessage);
                }
                else if (firstErrorType == "HttpRequestException")
                {
                    Log.Error("DownloadFailure {ExType}: {URL1}, Netværksfejl ved download: {URL1} - {Message}", firstErrorType, url1, firstErrorMessage);
                }
                else
                {
                    Log.Error("DownloadFailure {ExType}: {URL1}, Ukendt fejl ved download: {URL1} - {Message}", firstErrorType, url1, firstErrorMessage);
                }
                _failedDownloads.Add((url1, firstErrorMessage));
            }
        }
        else if (!string.IsNullOrEmpty(url2))
        {
            (succes, var _localSuccesList) = await TryUrl2(url2, downloadFolder, null, null, null);
            if (succes)
                foreach (var item in _localSuccesList)
                {
                    _succeededDownloads.Add(item);
                }
        }
    }

    public async Task<(bool, List<string>)> TryUrl2(string url2, string downloadFolder, string? firstErrorType, string? url1, string? firstErrorMessage)
    {
        var _localSuccesList = new List<string>();
        try
        {
            await DownloadAsync(url2, downloadFolder);
            _localSuccesList.Add(url2);
            Log.Information("Downloaded: {URL2}", url2);
            return (true, _localSuccesList);

        }

        catch (HttpRequestException ex)
        {
            Log.Error("DownloadFailure {ExType}: {URL1}, Netværksfejl ved download: {URL2} - {Message}", firstErrorType, url1, url2, ex.Message);
            return (false, _localSuccesList);

        }
        catch (IOException ex)
        {
            Log.Error("DownloadFailure {ExType}: {URL1}, Fil-fejl ved download: {URL2} - {Message}", url1, url2, ex.Message);
            return (false, _localSuccesList);

        }
        catch (Exception ex)
        {
            Log.Error("DownloadFailure {ExType}: {URL1},, Ukendt fejl ved download: {URL2} - {Message}", url1, url2, ex.Message);
            return (false, _localSuccesList);
        }
    }



    public record DownloadResult(bool Success, string? FileName, string? ErrorMessage);
    

}
