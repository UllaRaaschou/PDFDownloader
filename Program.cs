using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using Serilog;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();


Console.Write("Indtast filsti: ");
string? inputFilePath = Console.ReadLine();
var filePath = inputFilePath?? throw new IOException("Unable to read inputFilePath");
var functionalFilePath = FileHelper.GetFunctionalInputPath(filePath);
var workbook = new XLWorkbook(functionalFilePath);

Console.Write("Hvilket nummer har det aktuelle worksheet?");
string? inputWorkSheetNumber = Console.ReadLine(); 

if(!int.TryParse(inputWorkSheetNumber, out int worksheetNumber)) 
    throw new IOException("WorkSheetNumber must be an integer");
int workSheetNumber = worksheetNumber;

var worksheet = workbook.Worksheet(workSheetNumber);




public static class FileHelper
{
    
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

    public static List<URLObject> GetURLObjects(IXLWorksheet workSheet)
    {
        var listOfURLObjects = new List<URLObject>();
        var usedRange = workSheet.RangeUsed();
        Log.Information("Starter import af URL-objekter");

        if (usedRange != null)
        {

            foreach (var row in usedRange.Rows())
            {
                if (row.RowNumber() == 1) continue;
                if (!row.Cell(1).Value.IsNumber)
                {
                    Log.Warning("Række {Row} mangler gyldigt ID", row.RowNumber());
                    continue;
                }
                var lastCol = row.LastCellUsed()?.Address.ColumnNumber ?? 0;
                var cell2AndCell3AreEmpty = lastCol<2 ? true : false;
                                  
                if (cell2AndCell3AreEmpty) 
                {
                    Log.Warning("Række {Row} mangler url", row.RowNumber());
                    continue;
                }                   

                var id = (int)row.Cell(1).Value.GetNumber();
                var url1 = row.Cell(2).Value.ToString();
                var url2 = row.Cell(3).Value.ToString();

                listOfURLObjects.Add(new URLObject(id, url1, url2));
            }
        }
        return listOfURLObjects;
    }
}

public record URLObject(int id, string? url1, string? url2);

