using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using TirkxDownloader.Models;
using TirkxDownloader.Framework.Interface;

namespace TirkxDownloader.Services
{
    public class Downloader : PropertyChangedBase, IDownloader
    {
        private bool _isQueueDownloading;
        private bool _isDownloading;
        private int _maxDownloadingItem;
        private int _downloadingItems;
        private long _maximumBytesPerSecond;
        private string _downloaderErrorMessage;
        private readonly object _mutex = new object();
        private Queue<DownloadInfo> _queueingItems;
        private Dictionary<DownloadInfo, Tuple<Task, CancellationTokenSource>> _downloadingItemsDic;
        private DetailProvider _detailProvider;
        private IEventAggregator _eventAggregator;

        #region constructors
        public Downloader(IEventAggregator eventAggregator, DetailProvider detailProvider)
        {
            _queueingItems = new Queue<DownloadInfo>();
            _downloadingItemsDic = new Dictionary<DownloadInfo, Tuple<Task, CancellationTokenSource>>();

            _detailProvider = detailProvider;
            _eventAggregator = eventAggregator;
            MaxDownloadingItems = 1;
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
                }
            }
        }

        public long MaximumBytesPerSecond { get; set; }

        public int MaxDownloadingItems
        {
            get { return _maxDownloadingItem; }
            set
            {
                _maxDownloadingItem = value;
                NotifyOfPropertyChange(nameof(MaxDownloadingItems));
            }
        }
        #endregion

        #region methods
        public void DownloadItem(DownloadInfo item)
        {
            DownloadItemImp(item);
        }

        public void DownloadItems(IEnumerable<DownloadInfo> items)
        {
            foreach (var item in items)
            {
                if (item.Status == DownloadStatus.Queue)
                {
                    _queueingItems.Enqueue(item);
                }
            }

            _isQueueDownloading = true;
            DownloadItemsImp();
        }

        private void DownloadItemImp(DownloadInfo item)
        {
            if (item != null)
            {
                // In case completed item from event
                if (item.Status != DownloadStatus.Queue)
                {
                    DownloadingItems--;
                    _downloadingItemsDic.Remove(item);
                }
            }

            try
            {
                if (_isQueueDownloading && _downloadingItems < MaxDownloadingItems)
                {
                    DownloadInfo nextItem = GetNextDownloadInfo(item);
                    var cts = new CancellationTokenSource();
                    var ct = cts.Token;
                    nextItem.DownloadComplete += DownloadItemImp;
                    _downloadingItems++;
                    Task downloadTask = StartDownloadProcess(nextItem, ct);
                    _downloadingItemsDic.Add(nextItem, new Tuple<Task, CancellationTokenSource>(downloadTask, cts));
                }
            }
            catch (InvalidOperationException)
            {
                // Queue is empty
                _isDownloading = false;
                
                if (item == null)
                {
                    throw;
                }
                else
                {
                    return;
                }
            }
        }

        /// <summary>
        /// This method should be called when <see cref="DownloadItems(IEnumerable{DownloadInfo})"/> is called 
        /// only 1 time for start queue download later queueing download shuold start from <see cref="DownloadItems(IEnumerable{DownloadInfo})"/>
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

        private Task StartDownloadProcess(DownloadInfo item, CancellationToken ct)
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

            return processTask;
        }

        public void StopDownloadItem(DownloadInfo item)
        {
            try
            {
                _downloadingItemsDic[item].Item2.Cancel();
            }
            catch { }
        }

        public void StopDownloadItems(IEnumerable<DownloadInfo> items)
        {
            _isDownloading = false;

            if (items != null)
            {
                _downloadingItemsDic.
                    Where(x => x.Key.Status == DownloadStatus.Downloading || x.Key.Status == DownloadStatus.Preparing).
                    Apply(x => x.Value.Item2.Cancel());
            }
            else
            {
                _downloadingItemsDic.Apply(x => x.Value.Item2.Cancel());
            }
        }

        private DownloadInfo GetNextDownloadInfo(DownloadInfo item)
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
        #endregion
    }
}
