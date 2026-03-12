using ClosedXML.Excel;
using Serilog;

public class DataAccessService 
{  
    public string ExcelFolder { get; set; } = string.Empty;
    public string DownloadFolder { get; set; } = string.Empty;

    public void SetDownloadFolder(string functionalFilePath )     
    {        
        ExcelFolder = Path.GetDirectoryName(functionalFilePath) ?? string.Empty;
        DownloadFolder = Path.Combine(ExcelFolder, "PDFs");
    }
    
    /// <summary>
    /// Creates XLworkbook from userinput
    /// </summary>
    /// <returns></returns>
    /// <exception cref="IOException"></exception>
    public XLWorkbook? CreateWorkbook()
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

    /// <summary>
    /// If inputpath from user contains "", trim them to get a valid inputPath for workbook
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public string GetFunctionalInputPath(string input)
    {
        string? inputPath = input;

        var lastIndex = inputPath.Length - 1;
        if (inputPath[0] == '"' && inputPath[lastIndex] == '"')
        {
            inputPath = inputPath.Trim('"');
        }

        return inputPath;
    }

    /// <summary>
    /// Validates, that userinput for worksheet is int
    /// </summary>
    /// <param name="workbook"></param>
    /// <returns>relevant worksheet</returns>
    /// <exception cref="IOException"></exception>
    public IXLWorksheet? AccessWorkSheet(XLWorkbook workbook)
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

    public bool WantCheckForFormerDownloads() 
    {
        Console.Write("Skal jeg kun downloade filer, hvis de ikke allerede er downloadet tidligere? (j/n): ");
        var result = Console.ReadLine()?.ToLower();
        while (result != "j" && result != "n")
        {
            Console.Write("Ugyldigt svar. Skriv 'j' for ja eller 'n' for nej: ");
            result = Console.ReadLine()?.ToLower();
        }
        return result == "j";
    }

    
}    

