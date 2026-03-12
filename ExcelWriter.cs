using ClosedXML.Excel;
using System.Collections.Concurrent;

namespace PDFDownloader
{
    public class ExcelWriter
    {
        private readonly string _downloadFolder;
        private ConcurrentBag<(string url, string error)> _failedDownloads;
        private ConcurrentBag<string> _succeededDownloads;


        public ExcelWriter(string downloadFolder, ConcurrentBag<(string url, string error)> failedDownloads,
            ConcurrentBag<string> succeededDownloads)
        {
            _downloadFolder = downloadFolder;
            _failedDownloads = failedDownloads;
            _succeededDownloads = succeededDownloads;
        }
        public async Task WriteToExcel(string downloadFolder)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Data");

            var row = 1;
            sheet.Cell(row++, 1).Value = "Ikke-downloadet: ";

            foreach (var item in _failedDownloads)
            {
                sheet.Cell(row, 1).Value = item.Item1;
                sheet.Cell(row, 2).Value = item.Item2;
                row++;
            }

            row++;

            sheet.Cell(row++, 1).Value = "Downloadet:";


            foreach (var item in _succeededDownloads)
            {
                sheet.Cell(row++, 1).Value = item;
            }

            var oversigtsFolder = Path.Combine(downloadFolder, "DownloadOversigt");
            Directory.CreateDirectory(oversigtsFolder);
            var path = Path.Combine(oversigtsFolder, "Oversigt.xlsx");
            workbook.SaveAs(path);

            await Task.CompletedTask;
        }
    }
}
