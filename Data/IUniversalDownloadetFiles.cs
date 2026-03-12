using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDownloader.Data
{
    public interface IUniversalDownloadedFiles
    {
        public List<string> UniDownloadedFiles { get; set; } 
    }
}
