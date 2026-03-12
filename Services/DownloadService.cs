using PDFDownloader;
using PDFDownloader.Data;
using PDFDownloader.Services;
using Serilog;
using System.Collections.Concurrent;

    
public class DownloadService : IDownloadService 
{
    private IDownloadHistory _history;
    private readonly HttpClient _httpClient;
    private ConcurrentBag<(string url, string error)> _failedDownloads;
    private ConcurrentBag<string> _succeededDownloads;
    public DownloadService(IDownloadHistory history, HttpClient httpClient)
    {
        _history = history;
        _httpClient = httpClient;
        _failedDownloads = new ConcurrentBag<(string url, string error)> { };
        _succeededDownloads = new ConcurrentBag<string> { };
    }

    public async Task DownloadAllAsync(List<PDFLinkObject> listOfURLObjects, string downloadFolder)
    {
        var tasks = new List<Task>();

        foreach (var obj in listOfURLObjects)
        {
            tasks.Add(DownloadWithFallbackAsync(obj, downloadFolder));
        }

        await Task.WhenAll(tasks);

        var lines = _failedDownloads.Select(x => $"{x.url} - {x.error}");
        var filePath = Path.Combine(downloadFolder, "failed_downloads.txt");
        Directory.CreateDirectory(downloadFolder);
        File.WriteAllLines(filePath, lines);        
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

        var downloadedFiles =  new List<string> {url};
        _history.PreviousDownloadedFiles.AddRange(downloadedFiles);
    }

    private async Task DownloadWithFallbackAsync(PDFLinkObject obj, string downloadFolder)
    {
        var url1 = obj.url1;
        var url2 = obj.url2;
        bool succes = false;
        var firstErrorType = "";  
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
            }

            if (succes == false && !string.IsNullOrEmpty(url2))
            {
                (succes, var _localSuccesList, var _localFailureList) = await TryUrl2(url2, downloadFolder, firstErrorType, url1, firstErrorMessage);
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
            (succes, var _localSuccesList, var _localFailureList) = await TryUrl2(url2, downloadFolder, null, null, null);
            if (succes)
                foreach (var item in _localSuccesList)
                {
                    _succeededDownloads.Add(item);
                }
                foreach (var item in _localFailureList)
                {
                    _failedDownloads.Add(item);
                }
        }
    }

    public async Task<(bool, List<string>, List<(string, string)>)> TryUrl2(string url2, string downloadFolder, string? firstErrorType, string? url1, string? firstErrorMessage)
    {
        var _localSuccesList = new List<string>();
        var _localFailureList = new List<(string, string)>(); 
        try
        {
            await DownloadAsync(url2, downloadFolder);
            _localSuccesList.Add(url2);
            Log.Information("Downloaded: {URL2}", url2);
            return (true, _localSuccesList, _localFailureList);

        }

        catch (HttpRequestException ex)
        {
            Log.Error("DownloadFailure {ExType}: {URL1}, Netværksfejl ved download: {URL2} - {Message}", firstErrorType, url1, url2, ex.Message);
            _localFailureList.Add((url2, ex.Message));
            return (false, _localSuccesList, _localFailureList);

        }
        catch (IOException ex)
        {
            Log.Error("DownloadFailure {ExType}: {URL1}, Fil-fejl ved download: {URL2} - {Message}", url1, url2, ex.Message);
            _localFailureList.Add((url2, ex.Message));
            return (false, _localSuccesList, _localFailureList);

        }
        catch (Exception ex)
        {
            Log.Error("DownloadFailure {ExType}: {URL1},, Ukendt fejl ved download: {URL2} - {Message}", url1, url2, ex.Message);
            _localFailureList.Add((url2, ex.Message));
            return (false, _localSuccesList, _localFailureList);
        }
    }

    public ExcelWriter CreateResultWriter(string downloadFolder)
    {
        return new ExcelWriter(downloadFolder, _failedDownloads, _succeededDownloads);
    }
}
