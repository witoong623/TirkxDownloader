using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using TirkxDownloader.Framework;

namespace TirkxDownloader.Models
{
    public class DownloadEngine : PropertyChangedBase
    {
        private int _maxCurrentlyDownload;
        private string _engineErrorMessage;
        private DetailProvider _detailProvider;
        private CancellationTokenSource _cancelQueueDownload;
        private Queue<DownloadInfo> _downloadQueue;
        private Dictionary<DownloadInfo, Pair<Task, CancellationTokenSource>> _taskList;
        private IEventAggregator _eventAggregate;

        #region Properties
        public bool IsWorking { get; private set; }

        public CounterWarpper DownloadCounter { get; private set; }

        public string EngineErrorMessage
        {
            get { return _engineErrorMessage; }
            set
            {
                _engineErrorMessage = value;
                NotifyOfPropertyChange(() => EngineErrorMessage);
            }
        }
        #endregion

        public DownloadEngine(IEventAggregator eventAggregator, DetailProvider detailProvider)
        {
            DownloadCounter = new CounterWarpper();
            _taskList = new Dictionary<DownloadInfo, Pair<Task, CancellationTokenSource>>();
            _downloadQueue = new Queue<DownloadInfo>();
            _eventAggregate = eventAggregator;
            _detailProvider = detailProvider;
            _maxCurrentlyDownload = 1;
        }

        public void StartDownload(DownloadInfo info)
        {
            if (DownloadCounter.Downloading >= _maxCurrentlyDownload)
            {
                return;
            }

            if (info.Status == DownloadStatus.Complete || info.Status == DownloadStatus.Downloading || info.Status == DownloadStatus.Preparing)
            {
                return;
            }

            info.Status = DownloadStatus.Preparing;
            var downloadProgress = new DownloadProcess();
            var cts = new CancellationTokenSource();
            var downloadTask = Task.Run(() => downloadProgress.StartProgress(info, DownloadCounter, _eventAggregate, cts.Token, _detailProvider));
            _taskList.Add(info, new Pair<Task, CancellationTokenSource>(downloadTask, cts));
        }

        public void StopDownload(DownloadInfo downloadInfo)
        {
            try
            {
                var taskCancel = _taskList[downloadInfo];
                taskCancel.Second.Cancel();
                _taskList.Remove(downloadInfo);
            }
            catch (KeyNotFoundException)
            {
                return;
            }
        }

        public void StartQueueDownload(BindableCollection<DownloadInfo> downloadInfoList)
        {
            if (IsWorking || downloadInfoList.Count == 0)
            {
                return;
            }

            // check if items in collection isn't complete and don't already have in queue
            downloadInfoList.Where(x => x.Status != DownloadStatus.Complete && !_downloadQueue.Contains(x)).
                Apply(x => _downloadQueue.Enqueue((x)));
            IsWorking = true;
            _cancelQueueDownload = new CancellationTokenSource();
            Task.Run(() => StartQueueDownloadImp(_cancelQueueDownload.Token));
            NotifyCanQueue();
        }

        private async Task StartQueueDownloadImp(CancellationToken ct)
        {
            try
            {
                int queueRemaining = 0;
                int runningTaskRemaining = 0;
                int dequeueCount = 0;

                do
                {
                    foreach (var downloadItem in _downloadQueue)
                    {
                        if (DownloadCounter.Downloading >= _maxCurrentlyDownload)
                        {
                            break;
                        }

                        StartDownload(downloadItem);
                        dequeueCount++;
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                    _downloadQueue.Dequeue(dequeueCount);
                    dequeueCount = 0;
                    var task = _taskList.Select(x => x.Value.First).ToArray();
                    var timeout = Task.Delay(TimeSpan.FromSeconds(10));
                    Task[] waitedTask = new Task[task.Length + 1];
                    task.CopyTo(waitedTask, 1);
                    waitedTask[0] = timeout;
                    var completedTask = await Task.WhenAny(waitedTask);

                    // If completedTask isn't timeout, it's download task
                    if (completedTask != timeout)
                    {
                        // Find completed task from TaskList
                        var completedDownload = _taskList.Where(t => t.Value.First.Status == TaskStatus.RanToCompletion ||
                            t.Value.First.Status == TaskStatus.Faulted || t.Value.First.Status == TaskStatus.Canceled).
                            Select(t => t.Key).ToArray(); ;

                        // Delete every task that completed
                        foreach (var downloadInfo in completedDownload)
                        {
                            _taskList.Remove(downloadInfo);
                        }
                    }

                    queueRemaining = _downloadQueue.Count(file => file.Status == DownloadStatus.Queue);
                    runningTaskRemaining = _taskList.Count(t => t.Value.First.Status == TaskStatus.WaitingForActivation);
                    // Check if cancelation is requested
                    ct.ThrowIfCancellationRequested();
                } while (queueRemaining != 0 || runningTaskRemaining != 0);

                IsWorking = false;
                NotifyCanQueue();
            }
            catch (OperationCanceledException)
            {
                IsWorking = false;
                NotifyCanQueue();
                // Cancel all of running task
                _taskList.Values.Apply(x => x.Second.Cancel());

                await Task.WhenAll(_taskList.Select(t => t.Value.First).ToArray());
            }
            catch (Exception ex)
            {
                IsWorking = false;
                NotifyCanQueue();
                EngineErrorMessage = ex.Message;

                // In case that exception is thrown before polling
                if (!_cancelQueueDownload.IsCancellationRequested)
                {
                    _cancelQueueDownload.Dispose();
                }
            }
            finally
            {
                // Clean up collection
                _downloadQueue.Clear();
                _taskList.Clear();
            }
        }

        private void NotifyCanQueue()
        {
            _eventAggregate.PublishOnUIThread("CanStartQueue");
            _eventAggregate.PublishOnUIThread("CanStopQueue");
        }

        public void StopQueueDownload()
        {
            try
            {
                if (_cancelQueueDownload != null)
                {
                    _cancelQueueDownload.Cancel();
                    _cancelQueueDownload.Dispose();
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}
