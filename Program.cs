using ClosedXML.Excel;
using System.Threading.Tasks;
using Serilog;
using System;
using System.IO;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var workbook = GetDataClass.CreateWorkbook();
var accessedWorksheet = GetDataClass.AccessWorkSheet(workbook);
var accessData = new AccessData();
var listOfURLObjects = accessData.GetURLObjects(accessedWorksheet);
var downloadFolder = GetDataClass.DownloadFolder;

accessData.TryDownloadFromURLs(listOfURLObjects, downloadFolder).Wait();
Console.ReadLine();

public class AccessData
{
    public async Task TryDownloadFromURLs(List<URLObject> listOfURLObjects, string downloadFolder) 
    {
        foreach (var obj in listOfURLObjects)
        {
            if (!string.IsNullOrEmpty(obj.url1))
            {
                try 
                {
                    await DownloadAsync(obj.url1, downloadFolder);
                    Log.Information("Downloaded: {URL}", obj.url1);
                }
                catch (HttpRequestException ex)
                {
                    Log.Error("Netværksfejl ved download: {URL} - {Message}", obj.url1, ex.Message);
                }
                catch (IOException ex)
                {
                    Log.Error("Fil-fejl ved download: {URL} - {Message}", obj.url1, ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Error("Ukendt fejl ved download: {URL} - {Message}", obj.url1, ex.Message);
                }
            }
        }
    }
    public async Task DownloadAsync(string url, string downloadFolder)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(url);
        using var stream = await response.Content.ReadAsStreamAsync();

        var fileName = Path.GetFileName(url);
        var filePath = Path.Combine(downloadFolder, fileName);
        using var fileStream = File.Create(filePath);
        
        await stream.CopyToAsync(fileStream);
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
        return listOfURLObjects;
    }
}

public record URLObject(int id, string? url1, string? url2);
