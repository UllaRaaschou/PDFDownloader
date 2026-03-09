using ClosedXML.Excel;
using Serilog;
using PDFDownloader.Services;
using System.Runtime.CompilerServices;
using DocumentFormat.OpenXml.Bibliography;
using PDFDownloader.Data;

public partial class FileDownloader
{
    private readonly IDownloadService _downloadService;
    private List<string> _succeededDownloads;
    private List<(string url, string error)> _failedDownloads;
    private readonly IUniversalDownloadetFiles _universalDownloadetFiles;

    public FileDownloader(IDownloadService downloadService, IUniversalDownloadetFiles uni)
    {
        _downloadService = downloadService;
        _failedDownloads = new List<(string url, string error)> { };
        _universalDownloadetFiles = uni;
        _succeededDownloads = new List<string> { };
    }
    public async Task TryDownloadFromURLs(List<URLObject> listOfURLObjects, string downloadFolder, bool wantTjeck) 
    {
        foreach (var obj in listOfURLObjects)
        {

            if (wantTjeck &&
            (_universalDownloadetFiles.UniDownloadedFiles.Contains(obj.url1 ?? "") ||
            _universalDownloadetFiles.UniDownloadedFiles.Contains(obj.url2 ?? ""))) { continue; }

            await Methos(obj, downloadFolder);        
        }
        
        var lines = _failedDownloads.Select(x => $"{x.url} - {x.error}");
        var filePath = Path.Combine(downloadFolder, "failed_downloads.txt");
        Directory.CreateDirectory(downloadFolder);
        File.WriteAllLines(filePath, lines);
        var count = Directory.EnumerateFiles(downloadFolder).Count();
    }

    private async Task Methos(URLObject obj, string downloadFolder)
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
                await _downloadService.DownloadAsync(url1, downloadFolder);
                _succeededDownloads.Add(url1);
                Log.Information("Downloaded: {URL}", url1);
                succes = true;
                return;
            }
            catch (Exception ex)
            {
                firstErrorMessage = ex.Message;
                firstErrorType = ex.GetType().Name;
                firstUsedUrlFailed = true;
            }
        
            if(succes == false && !string.IsNullOrEmpty(url2)) 
            {
                (succes, var _localSuccesList) = await TryUrl2(url2, downloadFolder, firstErrorType, url1, firstErrorMessage);
                if (succes)
                    _succeededDownloads.AddRange(_localSuccesList);
                if(!succes)
                    _failedDownloads.Add((url1, firstErrorMessage));
            }  
        }
        await TryUrl2(url2, downloadFolder, null, null, null);



    }

    public async Task<(bool, List<string>)> TryUrl2(string url2, string downloadFolder, string? firstErrorType, string? url1, string? firstErrorMessage) 
    {
        var _localSuccesList = new List<string>();
        try
        {
            await _downloadService.DownloadAsync(url2, downloadFolder);
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


    public List<URLObject> GetURLObjects(IXLWorksheet workSheet)
    {
        if (workSheet == null)
        {
            Log.Error("Worksheet er null - kan ikke læse data");
            return new List<URLObject>();
        }
        
        var listOfURLObjects = new List<URLObject>();
        var usedRange = workSheet.RangeUsed();
        Log.Information("Starter import af URL-objekter");

        if (usedRange != null)
        {
            foreach (var row in usedRange.Rows())
            {
                if (row.RowNumber() == 1) continue;
                if (!row.Cell(1).Value.IsNumber)
                {
                    Log.Warning("Række {Row} mangler gyldigt ID", row.RowNumber());
                    continue;
                }
                var lastCol = row.LastCellUsed()?.Address.ColumnNumber ?? 0;
                var cell2AndCell3AreEmpty = lastCol<2 ? true : false;
                                   
                if (cell2AndCell3AreEmpty) 
                {
                    Log.Warning("Række {Row} mangler url", row.RowNumber());
                    continue;
                }                   

                var id = (int)row.Cell(1).Value.GetNumber();
                var url1 = row.Cell(2).Value.ToString();
                var url2 = row.Cell(3).Value.ToString();

                listOfURLObjects.Add(new URLObject(id, url1, url2));
            }
        }
        return listOfURLObjects.OrderBy(x => x.id).ToList();
    }

}
