using System;
using System.Collections.Generic;
using Caliburn.Micro;

namespace TirkxDownloader.Framework
{
    public class LoadingDetail : PropertyChangedBase
    {
        private int throughput;
        private double fileSize;
        private double recievedSize;
        private double percentProgress;
        private string errorMessage;
        private DownloadStatus loadingStatus;

        public DownloadInfo Parent { get; set; }

        public LoadingDetail(DownloadInfo parent)
        {
            Parent = parent;
            LoadingStatus = DownloadStatus.Preparing;
        }

        public double FileSize
        {
            get { return fileSize; }
            set
            {
                fileSize = value / 1048576;
                NotifyOfPropertyChange(() => FileSize);
            }
        }

        public DownloadStatus LoadingStatus
        {
            get { return loadingStatus; }
            set
            {
                loadingStatus = value;
                Parent.NotifyOfPropertyChange(() => Parent.Status);
            }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                errorMessage = value;
                NotifyOfPropertyChange(() => ErrorMessage);
            }
        }

        public int Throughput
        {
            get { return throughput; }
            set
            {
                throughput = value / 1024;
                NotifyOfPropertyChange(() => Throughput);
            }
        }

        public double PercentProgress
        {
            get { return percentProgress; }
            set
            {
                percentProgress = value / 1048576 * 100 / fileSize;
                NotifyOfPropertyChange(() => PercentProgress);
            }
        }

        public double RecievedSize
        {
            get { return recievedSize; }
            set
            {
                recievedSize = value / 1048576;
                NotifyOfPropertyChange(() => RecievedSize);
            }
        }
    }
}
