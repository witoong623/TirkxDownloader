using System;
using System.Collections.Generic;
using System.IO;
using Caliburn.Micro;

namespace TirkxDownloader.Framework
{
    public enum DownloadStatus { Queue, Complete, Downloading, Error, Preparing, Stop }

    public class DownloadInfo : PropertyChangedBase
    {
        private int _throughput;
        private double _fileSize;
        private double _recievedSize;
        private double _percentProgress;
        private string _errorMessage;
        private DownloadStatus _status;
        private string _fileName;
        private DateTime? _completeDate;
        private LoadingDetail _downloadDetail;

        
        public string DownloadLink { get; set; }
        public string SaveLocation { get; set; }
        public DateTime AddDate { get; set; }
        public IEventAggregator EventAggretagor { get; set; }

        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                NotifyOfPropertyChange(() => FileName);
            }
        }

        public string FullName
        {
            get { return Path.Combine(SaveLocation, FileName); }
        }

        public DateTime? CompleteDate
        {
            get { return _completeDate; }
            set
            {
                _completeDate = value;
                NotifyOfPropertyChange(() => CompleteDate);
            }
        }

        public DownloadStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyOfPropertyChange("Status");
            }
        }

        #region download detail
        public double FileSize
        {
            get { return _fileSize; }
            set
            {
                _fileSize = value / 1048576;
                NotifyOfPropertyChange(() => FileSize);
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
        #endregion
    }
}
