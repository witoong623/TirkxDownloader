using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TirkxDownloader.Framework.Interface
{
    public delegate void DownloadCompleteHandler(DownloadInfo downloadInfo);

    /// <summary>
    /// Implementation that implement this interface should implement PropertyChanged Event for data-binding
    /// </summary>
    public interface IDownloader
    {
        bool IsDownloading { get; }

        long MaximumBytesPerSecond { get; set; }

        int MaxDownloadingItems { get; set; }

        string DownloaderErrorMessage { get; set; }

        int DownloadingItems { get; set; }

        void DownloadItem(DownloadInfo item);

        void DownloadItems(IEnumerable<DownloadInfo> items);

        void StopDownloadItem(DownloadInfo item);

        void StopDownloadItems(IEnumerable<DownloadInfo> items);
    }
}
