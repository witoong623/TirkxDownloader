using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TirkxDownloader.Framework
{
    public enum DownloadStatus { Queue, Complete, Downloading, Error }

    public class DownloadInfo
    {
        public string FileName { get; set; }
        public string DownloadLink { get; set; }
        public string SaveLocation { get; set; }
        public DateTime AddDate { get; set; }
        public DateTime? CompleteDate { get; set; }
        public DownloadStatus Status { get; set; }
    }
}
