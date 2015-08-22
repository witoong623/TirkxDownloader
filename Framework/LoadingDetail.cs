using System;
using System.Collections.Generic;
using Caliburn.Micro;

namespace TirkxDownloader.Framework
{
    public class LoadingDetail : PropertyChangedBase
    {
        private int _throughput;
        private double _fileSize;
        private double _recievedSize;
        private double _percentProgress;
        private string _errorMessage;
        private DownloadStatus _loadingStatus;

        public DownloadInfo Parent { get; set; }

        public LoadingDetail(DownloadInfo parent)
        {
            Parent = parent;
            LoadingStatus = DownloadStatus.Preparing;
        }

        public double FileSize
        {
            get { return _fileSize; }
            set
            {
                _fileSize = value / 1048576;
                NotifyOfPropertyChange(() => FileSize);
            }
        }

        public DownloadStatus LoadingStatus
        {
            get { return _loadingStatus; }
            set
            {
                _loadingStatus = value;
                Parent.NotifyOfPropertyChange(() => Parent.Status);
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                NotifyOfPropertyChange(() => ErrorMessage);
            }
        }

        public int Throughput
        {
            get { return _throughput; }
            set
            {
                _throughput = value / 1024;
                NotifyOfPropertyChange(() => Throughput);
            }
        }

        public double PercentProgress
        {
            get { return _percentProgress; }
            set
            {
                _percentProgress = value / 1048576 * 100 / _fileSize;
                NotifyOfPropertyChange(() => PercentProgress);
            }
        }

        public double RecievedSize
        {
            get { return _recievedSize; }
            set
            {
                _recievedSize = value / 1048576;
                NotifyOfPropertyChange(() => RecievedSize);
            }
        }
    }
}
