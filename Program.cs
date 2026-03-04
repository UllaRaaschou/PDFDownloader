using System.Threading.Tasks;
using Serilog;
using System;
using System.IO;
using PDFDownloader.Services;
using static FileDownloader;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var workbook = GetDataAccess.CreateWorkbook();
var accessedWorksheet = GetDataAccess.AccessWorkSheet(workbook);
var httpDownloadService = new httpDownloadService();
var accessData = new FileDownloader(httpDownloadService);
var listOfURLObjects = accessData.GetURLObjects(accessedWorksheet);
var downloadFolder = GetDataAccess.DownloadFolder;

accessData.TryDownloadFromURLs(listOfURLObjects, downloadFolder).Wait();
Console.ReadLine();

public record URLObject(int id, string? url1, string? url2);
