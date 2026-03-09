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
var access = new GetDataAccess();
var workbook = access.CreateWorkbook();
var accessedWorksheet = access.AccessWorkSheet(workbook);
var uni = new UniversalDownloadedFiles();
var httpDownloadService = new httpDownloadService(uni);

var fileDownloader = new FileDownloader(httpDownloadService, uni, access);
var listOfURLObjects = fileDownloader.GetURLObjects(accessedWorksheet);
var downloadFolder = access.DownloadFolder;
var wantTjeck = access.WantCheckForFormerDownloads();

fileDownloader.TryDownloadFromURLs(listOfURLObjects, downloadFolder, wantTjeck).Wait();
fileDownloader.WriteToExcel(downloadFolder);
Console.WriteLine(Path.GetTempPath());
Console.ReadLine();
Console.WriteLine("Done!!!");

public record URLObject(int id, string? url1, string? url2);
