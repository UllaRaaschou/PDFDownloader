using System.Threading.Tasks;
using Serilog;
using System;
using System.IO;
using PDFDownloader.Services;
using static AccessData;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var workbook = GetDataClass.CreateWorkbook();
var accessedWorksheet = GetDataClass.AccessWorkSheet(workbook);
var httpDownloadService = new httpDownloadService();
var accessData = new AccessData(httpDownloadService);
var listOfURLObjects = accessData.GetURLObjects(accessedWorksheet);
var downloadFolder = GetDataClass.DownloadFolder;

accessData.TryDownloadFromURLs(listOfURLObjects, downloadFolder).Wait();
Console.ReadLine();

public record URLObject(int id, string? url1, string? url2);
