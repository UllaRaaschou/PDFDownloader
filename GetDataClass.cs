using ClosedXML.Excel;
using Serilog;

public class GetDataClass 
{  
    public static string ExcelFolder { get; set; }
    public static string DownloadFolder { get; set; }

    public static void SetDownloadFolder(string functionalFilePath ) 
    {
        ExcelFolder = Path.GetDirectoryName(functionalFilePath);
        DownloadFolder = Path.Combine(ExcelFolder, "PDFs");
    }
    public static XLWorkbook? CreateWorkbook()
    {
        Console.Write("Indtast filsti: ");
        string? inputFilePath = Console.ReadLine();
        var filePath = inputFilePath ?? throw new IOException("Unable to read inputFilePath");
        var functionalFilePath = GetFunctionalInputPath(filePath);
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

    public static string GetFunctionalInputPath(string input)
    {
        string? inputPath = input;

        var lastIndex = inputPath.Length - 1;
        if (inputPath[0] == '"' && inputPath[lastIndex] == '"')
        {
            inputPath = inputPath.Trim('"');
        }

        return inputPath;
    }

    public static IXLWorksheet? AccessWorkSheet(XLWorkbook workbook)
    {
        Console.Write("Hvilket nummer har det aktuelle worksheet?");
        string? inputWorkSheetNumber = Console.ReadLine();

        if (!int.TryParse(inputWorkSheetNumber, out int worksheetNumber))
            throw new IOException("WorkSheetNumber must be an integer");
        int workSheetNumber = worksheetNumber;

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

