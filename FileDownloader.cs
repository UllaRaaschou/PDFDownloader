using ClosedXML.Excel;
using Serilog;
using PDFDownloader.Services;
using System.Runtime.CompilerServices;

public partial class FileDownloader
{
    private readonly IDownloadService _downloadService;
    private List<(string url, string error)> _failedDownloads;
    public FileDownloader(IDownloadService downloadService)
    {
        _downloadService = downloadService;
        _failedDownloads = new List<(string url, string error)> { };
    }
    public async Task TryDownloadFromURLs(List<URLObject> listOfURLObjects, string downloadFolder) 
    {
        foreach (var obj in listOfURLObjects)
        {
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

        var usedUrl = string.IsNullOrEmpty(url1)? url2 : url1;
        if (!string.IsNullOrEmpty(usedUrl))
        {
            try
            {
                await _downloadService.DownloadAsync(usedUrl, downloadFolder);
                Log.Information("Downloaded: {URL}", usedUrl);
                var count = Directory.EnumerateFiles(downloadFolder).Count();

            }
            catch (HttpRequestException ex)
            {
                Log.Error("Netværksfejl ved download: {URL} - {Message}", usedUrl, ex.Message);
                _failedDownloads.Add((usedUrl, ex.Message));
            }
            catch (IOException ex)
            {
                Log.Error("Fil-fejl ved download: {URL} - {Message}", usedUrl, ex.Message);
                _failedDownloads.Add((usedUrl, ex.Message));
            }
            catch (Exception ex)
            {
                Log.Error("Ukendt fejl ved download: {URL} - {Message}", usedUrl, ex.Message);
                _failedDownloads.Add((usedUrl, ex.Message));
            }
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
