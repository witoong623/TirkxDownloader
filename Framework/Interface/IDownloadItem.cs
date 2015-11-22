using System;
using System.ComponentModel;
using System.IO;
using NodaTime;
using TirkxDownloader.Models;

namespace TirkxDownloader.Framework.Interface
{
    public interface IDownloadItem : INotifyPropertyChanged
    {
        #region event
        event DownloadCompleteHandler DownloadComplete;

        void OnDownloadComplete();
        #endregion

        #region properties
        /// <summary>
        /// File name including extension
        /// </summary>
        string FileName { get; set; }

        DownloadStatus Status { get; set; }

        string DownloadLink { get; set; }

        string SaveLocation { get; set; }

        /// <summary>
        /// Full path including filename and extension
        /// </summary>
        string FullName { get; }

        DateTime AddOn { get; set; }

        DateTime? CompleteOn { get; set; }

        /// <summary>
        /// File size in MB
        /// </summary>
        float FileSize { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        long FileSizeInBytes { get; set; }

        int Speed { get; set; }

        string ErrorMessage { get; set; }

        float PercentProgress { get; set; }

        /// <summary>
        /// In MB
        /// </summary>
        double RecievedSize { get; set; }

        Duration? ETA { get; set; }

        Stream InStream { get; set; }
        #endregion
    }
}
