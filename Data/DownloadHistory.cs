using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDownloader.Data
{
    public class DownloadHistory : IDownloadHistory
    {
        public List<string> PreviousDownloadedFiles { get; set; } = new List<string>();   
    }
}
