using System.Collections.Generic;
using TirkxDownloader.Models;

namespace TirkxDownloader.Framework.Interface
{
    public delegate void DownloadCompleteHandler(GeneralDownloadItem downloadInfo);

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

        void DownloadItem(IDownloadItem item);

        void DownloadItems(IEnumerable<IDownloadItem> items);

        void StopDownloadItem(IDownloadItem item);

        void StopDownloadItems(IEnumerable<IDownloadItem> items);
    }
}
