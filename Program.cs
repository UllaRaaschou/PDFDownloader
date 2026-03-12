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

if (workbook == null || accessedWorksheet == null)
{
    Log.Error("Failed to initialize workbook or worksheet");
    Console.WriteLine("Fejl: Kunne ikke åbne Excel-fil eller arbejdsark");
    Console.ReadLine();
    return;
}


var uni = new UniversalDownloadedFiles();
var httpClient = new HttpClient();



var preparer = new DownloadPreparer(uni, access);

var downloadFolder = access.DownloadFolder;
var wantCheck = access.WantCheckForFormerDownloads();
Console.WriteLine("Download starter, tag en kop kaffe");
var notDownloadedBefore = preparer.PrepareForDownload(accessedWorksheet, downloadFolder, wantCheck);

var httpDownloadService = new DownloadService(uni, httpClient);
await httpDownloadService.DownloadAllAsync(notDownloadedBefore, downloadFolder);

Console.WriteLine("Starting WriteToExcel...");
var writer = new ExcelWriter(downloadFolder, httpDownloadService._failedDownloads, httpDownloadService._succeededDownloads);
await writer.WriteToExcel(downloadFolder);
Console.WriteLine("WriteToExcel completed!");
//Console.WriteLine(Path.GetTempPath());
Console.ReadLine();
Console.WriteLine("Done!!!");
Console.ReadLine();

