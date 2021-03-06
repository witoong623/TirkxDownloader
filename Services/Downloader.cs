﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using TirkxDownloader.Models;
using TirkxDownloader.Framework.Interface;
using TirkxDownloader.Models.Settings;

namespace TirkxDownloader.Services
{
    public class Downloader : PropertyChangedBase, IDownloader
    {
        private bool _isQueueDownloading;
        private bool _isDownloading;

        private int _downloadingItems;
        private string _downloaderErrorMessage;
        private readonly object _mutex = new object();
        private Queue<IDownloadItem> _queueingItems;
        private Dictionary<IDownloadItem, CancellationTokenSource> _downloadingItemsDic;
        private FileHostingUtil _detailProvider;
        private IEventAggregator _eventAggregator;

        #region constructors
        public Downloader(IEventAggregator eventAggregator, FileHostingUtil detailProvider)
        {
            _queueingItems = new Queue<IDownloadItem>();
            _downloadingItemsDic = new Dictionary<IDownloadItem, CancellationTokenSource>();

            _detailProvider = detailProvider;
            _eventAggregator = eventAggregator;

            DownloadingSetting.MaxConcurrentDownload.Subscribe(x => MaxDownloadingItems = x);
            DownloadingSetting.MaximumBytesPerSec.Subscribe(x => MaximumBytesPerSecond = x);
        }
        #endregion

        #region properties
        public string DownloaderErrorMessage
        {
            get { return _downloaderErrorMessage; }
            set
            {
                _downloaderErrorMessage = value;
                NotifyOfPropertyChange(nameof(DownloaderErrorMessage));
            }
        }

        public int DownloadingItems
        {
            get { return _downloadingItems; }
            set
            {
                _downloadingItems = value;
                NotifyOfPropertyChange(nameof(DownloadingItems));
            }
        }

        public bool IsDownloading
        {
            get
            {
                lock (_mutex)
                {
                    return _isDownloading;
                }
            }
            private set
            {
                lock (_mutex)
                {
                    _isDownloading = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public long MaximumBytesPerSecond { get; private set; } = DownloadingSetting.MaximumBytesPerSec.Value;

        public byte MaxDownloadingItems { get; private set; } = DownloadingSetting.MaxConcurrentDownload.Value;
        #endregion

        #region methods
        public void DownloadItem(IDownloadItem item)
        {
            DownloadItemImp(item);
        }

        public void DownloadItems(IEnumerable<IDownloadItem> items)
        {
            foreach (var item in items)
            {
                if (item.Status == DownloadStatus.Queue || item.Status == DownloadStatus.Stop ||
                        item.Status == DownloadStatus.Error)
                {
                    _queueingItems.Enqueue(item);
                }
            }

            _isQueueDownloading = true;
            NotifyCanQueueMethod();
            DownloadItemsImp();
        }

        private void DownloadItemImp(IDownloadItem item)
        {
            if (item != null)
            {
                // In case completed item from event
                if (item.Status != DownloadStatus.Queue)
                {
                    DownloadingItems--;
                    _downloadingItemsDic.Remove(item);

                    if (!_isQueueDownloading) return;
                }
            }

            try
            {
                if (_isQueueDownloading && _downloadingItems < MaxDownloadingItems)
                {
                    IDownloadItem nextItem = GetNextDownloadInfo(item);
                    var cts = new CancellationTokenSource();
                    var ct = cts.Token;
                    nextItem.DownloadComplete += DownloadItemImp;
                    DownloadingItems++;
                    Task downloadTask = StartDownloadProcess(nextItem, ct);
                    _downloadingItemsDic.Add(nextItem, cts);
                }
                else if (_downloadingItems < MaxDownloadingItems && item != null)
                {
                    var cts = new CancellationTokenSource();
                    var ct = cts.Token;
                    item.DownloadComplete += DownloadItemImp;
                    DownloadingItems++;
                    Task downloadTask = StartDownloadProcess(item, ct);
                    _downloadingItemsDic.Add(item, cts);
                }
            }
            catch (InvalidOperationException)
            {
                // Queue is empty
                _isDownloading = false;
                
                if (item == null)
                {
                    NotifyCanQueueMethod();
                    throw;
                }
                else
                {
                    NotifyCanQueueMethod();
                    return;
                }
            }
        }

        /// <summary>
        /// This method should be called when <see cref="DownloadItems(IEnumerable{GeneralDownloadItem})"/> is called 
        /// only 1 time for start queue download later queueing download shuold start from <see cref="DownloadItems(IEnumerable{GeneralDownloadItem})"/>
        /// </summary>
        private void DownloadItemsImp()
        {
            try
            {
                while (_downloadingItems < MaxDownloadingItems)
                {
                    DownloadItemImp(null);
                }
            }
            catch (InvalidOperationException) { }
        }

        private Task StartDownloadProcess(IDownloadItem item, CancellationToken ct)
        {
            var downloadProcess = new DownloadProcess();
            Task processTask = downloadProcess.StartDownloadProcess
                (
                    maximumBytesPerSecond: MaximumBytesPerSecond,
                    downloadInf: item,
                    eventAggregate: _eventAggregator,
                    ct: ct,
                    detailProvider: _detailProvider
                );

            _eventAggregator.Subscribe(downloadProcess);

            return processTask;
        }

        public void StopDownloadItem(IDownloadItem item)
        {
            try
            {
                _downloadingItemsDic[item].Cancel();
            }
            catch { }
        }

        public void StopDownloadItems(IEnumerable<IDownloadItem> items)
        {
            _isDownloading = false;

            if (items != null)
            {
                _downloadingItemsDic.
                    Where(x => x.Key.Status == DownloadStatus.Downloading || x.Key.Status == DownloadStatus.Preparing).
                    Apply(x => x.Value.Cancel());
            }
            else
            {
                _downloadingItemsDic.Apply(x => x.Value.Cancel());
            }
        }

        private IDownloadItem GetNextDownloadInfo(IDownloadItem item)
        {
            if (item == null)
            {
                return _queueingItems.Dequeue();
            }
            else
            {
                return item.Status == DownloadStatus.Queue ? item : _queueingItems.Dequeue();
            }
        }

        private void NotifyCanQueueMethod()
        {
            _eventAggregator.PublishOnUIThread("CanStartQueue");
            _eventAggregator.PublishOnUIThread("CanStopQueue");
        }
        #endregion
    }
}
