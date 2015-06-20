using System;
using System.Collections.Generic;
using System.IO;
using Caliburn.Micro;

namespace TirkxDownloader.Framework
{
    public enum DownloadStatus { Queue, Complete, Downloading, Error, Preparing, Stop }

    public class DownloadInfo : PropertyChangedBase
    {
        private string fileName;
        private DateTime? completeDate;
        private LoadingDetail downloadDetail;

        
        public string DownloadLink { get; set; }
        public string SaveLocation { get; set; }
        public DateTime AddDate { get; set; }
        public IEventAggregator EventAggretagor { get; set; }

        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                NotifyOfPropertyChange(() => FileName);
            }
        }

        public string FullName
        {
            get { return Path.Combine(SaveLocation, FileName); }
        }

        public DateTime? CompleteDate
        {
            get { return completeDate; }
            set
            {
                completeDate = value;
                NotifyOfPropertyChange(() => CompleteDate);
            }
        }

        public DownloadStatus Status
        {
            get
            {
                if (DownloadDetail == null)
                {
                    return DownloadStatus.Queue;
                }
                else
                {
                    return DownloadDetail.LoadingStatus;
                }
            }
        }

        public LoadingDetail DownloadDetail
        {
            get { return downloadDetail; }
            set
            {
                downloadDetail = value;
                NotifyOfPropertyChange(() => downloadDetail);
            }
        }
    }
}
