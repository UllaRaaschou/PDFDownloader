using ClosedXML.Excel;



Console.Write("Indtast filsti: ");
var input = Console.ReadLine();
var filePath = FileHelper.GetInputPath(input);
Console.WriteLine(filePath);
var workbook = new XLWorkbook(filePath);




public static class FileHelper
{
    public static string GetInputPath()
    {
        string? inputPath = null;
        try
        {
            inputPath = Console.ReadLine();
        }
        catch 
        { 
            throw new IOException("It was not possible to read inputPath")
        }
        var lastIndex = inputPath.Length - 1;
        if (inputPath[0] == '"' && inputPath[lastIndex] == '"')
        {
            inputPath = inputPath.Trim('"');
        }

        return inputPath;
    }
}