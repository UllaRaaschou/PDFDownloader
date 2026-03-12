using ClosedXML.Excel;
using Serilog;
using PDFDownloader.Data;
using System.Collections.Concurrent;
using PDFDownloader;

public partial class DownloadPreparer
{   
    public ConcurrentBag<string>? _succeededDownloads; 
    private readonly IUniversalDownloadetFiles _universalDownloadetFiles;
    private readonly DataAccessService _access;

    public DownloadPreparer(IUniversalDownloadetFiles uni, DataAccessService access)
    {
        _universalDownloadetFiles = uni;    
        _access = access;
    }

   public async Task<List<PDFLinkObject>> PrepareForDownload(IXLWorksheet workSheet, string downloadFolder, bool wantTjeck) 
    { 
        var listOfPDFLinkObjects = GetPDFLinkObjectsFromWorksheet(workSheet);
        var notDownloadetBefore = new List<PDFLinkObject>();
        if (wantTjeck == true)
        {
            notDownloadetBefore = ReturnListOfPDFLinksNotDownloadetBefore(listOfPDFLinkObjects);
        }
        notDownloadetBefore = listOfPDFLinkObjects;
        return notDownloadetBefore;
    }

    public List<PDFLinkObject> ReturnListOfPDFLinksNotDownloadetBefore(List<PDFLinkObject> listOfURLObjects)
    {
        var notDownloadetBefore = new List<PDFLinkObject>();
        foreach (var obj in listOfURLObjects)
        {
            if (
                _universalDownloadetFiles.UniDownloadedFiles.Contains(obj.url1 ?? "") ||
                _universalDownloadetFiles.UniDownloadedFiles.Contains(obj.url2 ?? ""))
            {
                continue;
            }
            notDownloadetBefore.Add(obj);        
        }
        return notDownloadetBefore;
    }

    /// <summary>
    /// Returns a list of PDFLinkObjects made from user-input excell.
    /// If data is not readable or if ID or all URL's are missing, warnings are logged but procedure continues.
    /// </summary>
    /// <param name="workSheet"></param>
    /// <returns></returns>
    public List<PDFLinkObject> GetPDFLinkObjectsFromWorksheet(IXLWorksheet workSheet)
    {
        if (workSheet == null)
        {
            Log.Error("Worksheet er null - kan ikke læse data");
            return new List<PDFLinkObject>();
        }
        
        var listOfPDFLinkObjects = new List<PDFLinkObject>();
        var usedRange = workSheet.RangeUsed();
        Log.Information("Starter import af PDFLink-objekter");

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

                listOfPDFLinkObjects.Add(new PDFLinkObject(id, url1, url2));
            }
        }
        return listOfPDFLinkObjects.OrderBy(x => x.id).ToList();
    }
}
