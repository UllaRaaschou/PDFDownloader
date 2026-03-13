using PDFDownloader.Data;
using PDFDownloader.Services;
using Serilog;

namespace PDFDownloader
{
    public class ApplicationOrchestrator
    {
        private readonly InputService _inputService;
        private readonly OutputExcelService _outputExcelService;
        private readonly WorkBookService _workBookService;
        private readonly DownloadPreparer _preparer;
        private readonly DownloadService _downloadService;
        private readonly IDownloadHistory _downloadHistory;

        public ApplicationOrchestrator(
            InputService inputService,
            OutputExcelService outputExcelService,
            WorkBookService workBookService,
            DownloadPreparer preparer,
            DownloadService downloadService,
            IDownloadHistory downloadHistory)
        {
            _inputService = inputService;
            _outputExcelService = outputExcelService;
            _workBookService = workBookService;
            _preparer = preparer;
            _downloadService = downloadService;
            _downloadHistory = downloadHistory;
        }

        public async Task<bool> RunAsync()
        {
            try
            {
                var filePath = _inputService.GetInputExcelPath();
                var downloadFolder = _workBookService.SetDownloadFolder(filePath);
                var workbook = _workBookService.CreateWorkbook(filePath); 
                var worksheet = _workBookService.TryAccessWorkSheet(workbook, _inputService.GetWorkSheetNumber());  
                var outputExcelFolder = _outputExcelService.SetOutputExcelFolder(filePath);
                var wantCheck = _inputService.WantCheckForFormerDownloads();

                if (workbook == null || worksheet == null)
                {
                    Console.WriteLine("Fejl: Kunne ikke åbne Excel-fil eller arbejdsark");
                    return false;
                }                             
                            
                Console.WriteLine("Download starter, tag en kop kaffe");
                var notDownloadedBefore = _preparer.PrepareForDownload(worksheet, downloadFolder, wantCheck);                          
                await _downloadService.DownloadAllAsync(notDownloadedBefore, downloadFolder);

                var writer = _downloadService.CreateResultWriter(outputExcelFolder);
                await writer.WriteToExcel();

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
