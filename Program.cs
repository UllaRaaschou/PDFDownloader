using System.Threading.Tasks;
using Serilog;
using System;
using System.IO;
using PDFDownloader.Services;
using static FileDownloader;
using PDFDownloader.Data;
using Serilog.Sinks.File; // Add this using directive for Serilog File sink

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/download.log")
    .CreateLogger();

var workbook = GetDataAccess.CreateWorkbook();
var accessedWorksheet = GetDataAccess.AccessWorkSheet(workbook);
var uni = new UniversalDownloadedFiles();
var httpDownloadService = new httpDownloadService(uni);
var fileDownloader = new FileDownloader(httpDownloadService, uni);
var listOfURLObjects = fileDownloader.GetURLObjects(accessedWorksheet);
var downloadFolder = GetDataAccess.DownloadFolder;
var wantTjeck = GetDataAccess.WantCheckForFormerDownloads();

fileDownloader.TryDownloadFromURLs(listOfURLObjects, downloadFolder, wantTjeck).Wait();
Console.WriteLine(Path.GetTempPath());
Console.ReadLine();

public record URLObject(int id, string? url1, string? url2);
