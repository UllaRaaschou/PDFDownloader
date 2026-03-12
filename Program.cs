using System.Threading.Tasks;
using Serilog;
using System;
using System.IO;
using PDFDownloader.Services;
using static DownloadPreparer;
using PDFDownloader.Data;
using Serilog.Sinks.File;
using PDFDownloader; // Add this using directive for Serilog File sink

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/download.log")
    .CreateLogger();


var access = new DataAccessService();
var workbook = access.CreateWorkbook();
var accessedWorksheet = access.AccessWorkSheet(workbook);


var uni = new UniversalDownloadedFiles();
var httpClient = new HttpClient();



var preparer = new DownloadPreparer(uni, access);
var listOfURLObjects = preparer.GetURLObjects(accessedWorksheet);
var downloadFolder = access.DownloadFolder;
var wantTjeck = access.WantCheckForFormerDownloads();
Console.WriteLine("Download starter, tag en kop kaffe");
var notDownloadetBefore = preparer.PerformEarlierDownloadetStatus(listOfURLObjects, downloadFolder, wantTjeck);
var httpDownloadService = new DownloadService(uni, httpClient);
httpDownloadService.TryDownloadToDestination(notDownloadetBefore, downloadFolder, wantTjeck);

Console.WriteLine("Starting WriteToExcel...");
var writer = new ExcelWriter(downloadFolder, httpDownloadService._failedDownloads, preparer._succeededDownloads);
await writer.WriteToExcel(downloadFolder);
Console.WriteLine("WriteToExcel completed!");
//Console.WriteLine(Path.GetTempPath());
Console.ReadLine();
Console.WriteLine("Done!!!");
Console.ReadLine();

public record URLObject(int id, string? url1, string? url2);
