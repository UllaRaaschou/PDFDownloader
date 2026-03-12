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
        var history = new DownloadHistory();
        var httpClient = new HttpClient();
        var inputService = new InputService();
        var outputService = new OutputExcelService();
        var workBookService = new WorkBookService();
        var preparer = new DownloadPreparer(history);
        var downloadService = new DownloadService(history, httpClient);
        var orchestrator = new ApplicationOrchestrator(
            inputService,
            outputService,
            workBookService,
            preparer,
            downloadService,
            history);
        var success = await orchestrator.RunAsync();
        if (success)
        {
            Console.WriteLine("Done!!!");
        }
        Console.ReadLine();
    }
}