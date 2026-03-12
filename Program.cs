using Serilog;
using PDFDownloader.Data;
using PDFDownloader;
using PDFDownloader.Services;
internal class Program
{
    private static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
        .WriteTo.File("logs/download.log")
        .CreateLogger();
        var uni = new UniversalDownloadedFiles();
        var httpClient = new HttpClient();
        var inputService = new InputService();
        var outputService = new OutputExcelService();
        var workBookService = new WorkBookService();
        var preparer = new DownloadPreparer(uni);
        var downloadService = new DownloadService(uni, httpClient);
        var orchestrator = new ApplicationOrchestrator(
            inputService,
            outputService,
            workBookService,
            preparer,
            downloadService,
            uni);
        var success = await orchestrator.RunAsync();
        if (success)
        {
            Console.WriteLine("Done!!!");
        }
        Console.ReadLine();
    }
}