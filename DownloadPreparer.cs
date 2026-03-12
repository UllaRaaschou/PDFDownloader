using ClosedXML.Excel;
using Serilog;
using PDFDownloader.Data;
using System.Collections.Concurrent;
using PDFDownloader;

public partial class DownloadPreparer
{     
    private readonly IUniversalDownloadedFiles _universalDownloadedFiles;
    private readonly DataAccessService _access;

    public DownloadPreparer(IUniversalDownloadedFiles uni, DataAccessService access)
    {
        _universalDownloadedFiles = uni;    
        _access = access;
    }

   public List<PDFLinkObject> PrepareForDownload(IXLWorksheet workSheet, string downloadFolder, bool wantCheck) 
    { 
        var listOfPDFLinkObjects = GetPDFLinkObjectsFromWorksheet(workSheet);
        var notDownloadedBefore = new List<PDFLinkObject>();
        if (wantCheck == true)
            notDownloadedBefore = ReturnListOfPDFLinksNotDownloadedBefore(listOfPDFLinkObjects);        
        else
            notDownloadedBefore = listOfPDFLinkObjects;
        return notDownloadedBefore;
    }

    public List<PDFLinkObject> ReturnListOfPDFLinksNotDownloadedBefore(List<PDFLinkObject> listOfURLObjects)
    {
        var notDownloadedBefore = new List<PDFLinkObject>();
        foreach (var obj in listOfURLObjects)
        {
            if (
                _universalDownloadedFiles.UniDownloadedFiles.Contains(obj.url1 ?? "") ||
                _universalDownloadedFiles.UniDownloadedFiles.Contains(obj.url2 ?? ""))
            {
                continue;
            }
            notDownloadedBefore.Add(obj);        
        }
        return notDownloadedBefore;
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
