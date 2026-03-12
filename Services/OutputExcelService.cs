using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDownloader.Services
{
    public class OutputExcelService
    {      
        public OutputExcelService() { }
       
        //public string OutputExcelFolder { get; set; } = string.Empty;
        public string SetOutputExcelFolder(string functionalFilePath)
        {
            var excelDirectory = Path.GetDirectoryName(functionalFilePath) ?? string.Empty;
            return Path.Combine(excelDirectory, "Oversigt");
        }
    }
}
