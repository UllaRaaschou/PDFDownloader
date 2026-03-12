using ClosedXML.Excel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDownloader.Services
{
    public class WorkBookService
    {
        string _functionalFilePath = string.Empty;

        public string DownloadFolder { get; set; } = string.Empty;

        public WorkBookService() { }
       


        public string SetDownloadFolder(string functionalFilePath)
        {
            var directory = Path.GetDirectoryName(functionalFilePath) ?? string.Empty;
            return Path.Combine(directory, "PDFs");
        }



        /// <summary>
        /// Creates XLworkbook from userinput
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        public XLWorkbook? CreateWorkbook(string functionalFilePath)
        {
            //Console.Write("Indtast filsti: ");
            //string? inputFilePath = Console.ReadLine();
            //var filePath = inputFilePath ?? throw new IOException("Unable to read inputFilePath");
            //var functionalFilePath = GetFunctionalInputPath(filePath);
            SetDownloadFolder(functionalFilePath);

            XLWorkbook? workbook = null;
            try
            {
                return workbook = new XLWorkbook(functionalFilePath);
            }
            catch (FileNotFoundException ex)
            {
                Log.Error("Excel-fil ikke fundet: {Path}", functionalFilePath);
                return null;
            }
        }

        /// <summary>
        /// Try access the worksheet numbered by user
        /// </summary>
        /// <param name="workbook"></param>
        /// <returns>relevant worksheet</returns>
        /// <exception cref="IOException"></exception>
        public IXLWorksheet? TryAccessWorkSheet(XLWorkbook workbook, int workSheetNumber)
        {
            //Console.Write("Hvilket nummer har det aktuelle worksheet?");
            //string? inputWorkSheetNumber = Console.ReadLine();

            //if (!int.TryParse(inputWorkSheetNumber, out int worksheetNumber))
            //    throw new IOException("WorkSheetNumber must be an integer");
            //int workSheetNumber = worksheetNumber;

            try
            {
                var worksheet = workbook.Worksheet(workSheetNumber);
                return worksheet;
            }
            catch (ArgumentException ex)
            {
                Log.Error("Worksheet {Number} findes ikke i arket", workSheetNumber);
                return null;
            }
        }
    }
}
