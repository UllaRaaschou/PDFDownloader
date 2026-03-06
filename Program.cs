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
var fileDownloader = new FileDownloader(httpDownloadService);
var listOfURLObjects = fileDownloader.GetURLObjects(accessedWorksheet);
var downloadFolder = GetDataAccess.DownloadFolder;

fileDownloader.TryDownloadFromURLs(listOfURLObjects, downloadFolder).Wait();
Console.ReadLine();

public record URLObject(int id, string? url1, string? url2);
