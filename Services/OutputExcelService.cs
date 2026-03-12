using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDownloader.Services
{
    public class OutputExcelService
    {
        private string _functionalFilePath;
        public OutputExcelService(string functionalFilePath)
        {
            _functionalFilePath = functionalFilePath;
        }
        public string OutputExcelFolder { get; set; } = string.Empty;
        public void SetOutputExcelFolder(string functionalFilePath)
        {
            var excelDirectory = Path.GetDirectoryName(functionalFilePath) ?? string.Empty;
            OutputExcelFolder = Path.Combine(excelDirectory, "Oversigt");
        }
    }
}
