﻿using System;
using System.IO;
using TirkxDownloader.Framework;
using TirkxDownloader.Framework.Interface;
using Caliburn.Micro;
using NodaTime;

namespace TirkxDownloader.Models
{
    public enum DownloadStatus { Queue, Complete, Downloading, Error, Preparing, Stop }

    public class GeneralDownloadItem : PropertyChangedBase, IDownloadItem
    {
        private int _speed;
        private float _fileSize;
        private double _recievedSize;
        private float _percentProgress;
        private string _errorMessage;
        private DownloadStatus _status;
        private string _fileName;
        private Duration? _eta;
        private DateTime? _completeDate;

        public event DownloadCompleteHandler DownloadComplete;

        public string DownloadLink { get; set; }
        public string SaveLocation { get; set; }
        public DateTime AddOn { get; set; }
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

        public DateTime? CompleteOn
        {
            get { return _completeDate; }
            set
            {
                _completeDate = value;
                NotifyOfPropertyChange(() => CompleteOn);
            }
        }

        public DownloadStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyOfPropertyChange(nameof(Status));
            }
        }

        #region download detail
        public float FileSize
        {
            get { return _fileSize; }
            set
            {
                _fileSize = value;
                NotifyOfPropertyChange(() => FileSize);
            }
        }

        public long FileSizeInBytes { get; set; }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                NotifyOfPropertyChange(() => ErrorMessage);
            }
        }

        public int Speed
        {
            get { return _speed; }
            set
            {
                _speed = value / 1024;
                NotifyOfPropertyChange(() => Speed);
            }
        }

        public float PercentProgress
        {
            get { return _percentProgress; }
            set
            {
                _percentProgress = value * 100 / FileSizeInBytes;
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

        public Duration? ETA
        {
            get { return _eta; }
            set
            {
                _eta = value;
                NotifyOfPropertyChange(nameof(ETA));
            }
        }

        public ThrottledStream InStream { get; set; }

        Stream IDownloadItem.InStream
        {
            get { return InStream; }
            set { InStream = (ThrottledStream)value; }
        }

        public void OnDownloadComplete()
        {
            if (DownloadComplete != null)
            {
                DownloadComplete(this);
            }
        }
        #endregion
    }
}