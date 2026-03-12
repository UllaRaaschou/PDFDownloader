using ClosedXML.Excel;
using Serilog;
using PDFDownloader.Services;
using PDFDownloader.Data;
using System.Collections.Concurrent;
using Task = System.Threading.Tasks.Task;

public partial class DownloadPreparer
{
    //private readonly IDownloadService _downloadService;
    public ConcurrentBag<string> _succeededDownloads;
    private readonly DataAccessService _access;
   
    private readonly IUniversalDownloadetFiles _universalDownloadetFiles;

    public DownloadPreparer(/*IDownloadService downloadService, */IUniversalDownloadetFiles uni, DataAccessService access)
    {
        //_downloadService = downloadService;
        //_failedDownloads = new ConcurrentBag<(string url, string error)> { };
        _universalDownloadetFiles = uni;
        //_succeededDownloads = new ConcurrentBag<string> { };
        _access = access;
    }

   public async Task PrepareForDownload(IXLWorksheet workSheet, string downloadFolder, bool wantTjeck) 
    { 
        var listOfURLObjects = GetURLObjects(workSheet);
        var NotDownloadetBefore = PerformEarlierDownloadetStatus(listOfURLObjects, downloadFolder, wantTjeck);
    }

    public List<URLObject> PerformEarlierDownloadetStatus(List<URLObject> listOfURLObjects, string downloadFolder, bool wantTjeck)
    {
        var notDownloadetBefore = new List<URLObject>();

        foreach (var obj in listOfURLObjects)
        {
            if (wantTjeck &&
                (_universalDownloadetFiles.UniDownloadedFiles.Contains(obj.url1 ?? "") ||
                 _universalDownloadetFiles.UniDownloadedFiles.Contains(obj.url2 ?? "")))
            {
                continue;
            }

            notDownloadetBefore.Add(obj);        
        }
        return notDownloadetBefore;
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
