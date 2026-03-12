using PDFDownloader.Data;
using Serilog;

namespace PDFDownloader
{
    public class ApplicationOrchestrator
    {
        private readonly DataAccessService _dataAccess;
        private readonly DownloadPreparer _preparer;
        private readonly DownloadService _downloadService;
        private readonly IUniversalDownloadedFiles _universalDownloadedFiles;

        public ApplicationOrchestrator(
            DataAccessService dataAccess,
            DownloadPreparer preparer,
            DownloadService downloadService,
            IUniversalDownloadedFiles universalDownloadedFiles)
        {
            _dataAccess = dataAccess;
            _preparer = preparer;
            _downloadService = downloadService;
            _universalDownloadedFiles = universalDownloadedFiles;
        }

        public async Task<bool> RunAsync()
        {
            try
            {                
                var workbook = _dataAccess.CreateWorkbook(); 
                var worksheet = _dataAccess.AccessWorkSheet(workbook);  

              
                if (workbook == null || worksheet == null)
                {
                    Console.WriteLine("Fejl: Kunne ikke åbne Excel-fil eller arbejdsark");
                    return false;
                }
                               
                var downloadFolder = _dataAccess.DownloadFolder;
                var wantCheck = _dataAccess.WantCheckForFormerDownloads();
                             
                Console.WriteLine("Download starter, tag en kop kaffe");
                var notDownloadedBefore = _preparer.PrepareForDownload(worksheet, downloadFolder, wantCheck);
                          
                await _downloadService.DownloadAllAsync(notDownloadedBefore, downloadFolder);

                Console.WriteLine("Starting WriteToExcel...");
                var writer = new ExcelWriter(
                    downloadFolder,
                    _downloadService._failedDownloads,
                    _downloadService._succeededDownloads);
                await writer.WriteToExcel(downloadFolder);
                Console.WriteLine("WriteToExcel completed!");

                return true;
            }
            catch (IOException ex)
            {               
                Log.Error(ex, "IO fejl: {Message}", ex.Message);
                Console.WriteLine($"Fejl: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {                
                Log.Error(ex, "Uventet fejl under download proces");
                Console.WriteLine($"Der opstod en uventet fejl: {ex.Message}");
                return false;
            }
        }
    }
}
