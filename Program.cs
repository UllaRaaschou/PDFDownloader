using Serilog;
using PDFDownloader.Data;
using PDFDownloader;
internal class Program
{
    private static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
        .WriteTo.File("logs/download.log")
        .CreateLogger();
        var uni = new UniversalDownloadedFiles();
        var httpClient = new HttpClient();
        var dataAccess = new DataAccessService();
        var preparer = new DownloadPreparer(uni, dataAccess);
        var downloadService = new DownloadService(uni, httpClient);
        var orchestrator = new ApplicationOrchestrator(
            dataAccess,
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