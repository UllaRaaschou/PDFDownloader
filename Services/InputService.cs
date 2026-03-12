namespace PDFDownloader.Services
{
    public class InputService
    {
        public string GetInputExcelPath()
        {
            Console.Write("Indtast filsti: ");
            string? inputFilePath = Console.ReadLine();
            var filePath = inputFilePath ?? throw new IOException("Unable to read inputFilePath");
            return GetFunctionalInputPath(filePath);
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

        public int GetWorkSheetNumber()
        {
            Console.Write("Hvilket nummer har det aktuelle worksheet?");
            string? inputWorkSheetNumber = Console.ReadLine();

            if (!int.TryParse(inputWorkSheetNumber, out int worksheetNumber))
                throw new IOException("WorkSheetNumber must be an integer");
            return worksheetNumber;
        }
    }
}
