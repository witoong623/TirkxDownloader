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
        private int maxCurrentlyDownload;
        private string engineErrorMessage;
        private CounterWarpper counterWarpper;
        private CancellationTokenSource cancelQueueDownload;
        private Queue<DownloadInfo> downloadQueue;
        private Dictionary<DownloadInfo, Pair<Task, CancellationTokenSource>> taskList;
        private IEventAggregator eventAggregate;

        public bool IsWorking { get; private set; }

        public int Downloading { get { return counterWarpper.Downloading; } }

        public string EngineErrorMessage
        {
            get { return engineErrorMessage; }
            set
            {
                engineErrorMessage = value;
                NotifyOfPropertyChange(() => EngineErrorMessage);
            }
        }

        public DownloadEngine(IEventAggregator eventAggregator)
        {
            counterWarpper = new CounterWarpper();
            taskList = new Dictionary<DownloadInfo, Pair<Task, CancellationTokenSource>>();
            downloadQueue = new Queue<DownloadInfo>();
            eventAggregate = eventAggregator;
            maxCurrentlyDownload = 1;
        }

        public void StartDownload(DownloadInfo downloadInfo)
        {
            if (counterWarpper.Downloading >= maxCurrentlyDownload)
            {
                return;
            }

            if (downloadInfo.Status == DownloadStatus.Complete)
            {
                return;
            }

            downloadInfo.DownloadDetail = new LoadingDetail(downloadInfo);
            var downloadProgress = new DownloadProcess();
            var cts = new CancellationTokenSource();
            var downloadTask = Task.Run(() => downloadProgress.StartProgress(downloadInfo, counterWarpper, eventAggregate, cts.Token));
            taskList.Add(downloadInfo, new Pair<Task, CancellationTokenSource>(downloadTask, cts));
        }

        public void StopDownload(DownloadInfo downloadInfo)
        {
            try
            {
                var taskCancel = taskList[downloadInfo];
                taskCancel.Second.Cancel();
                taskList.Remove(downloadInfo);
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
            downloadInfoList.Where(x => x.Status != DownloadStatus.Complete && !downloadQueue.Contains(x)).
                Apply(x => downloadQueue.Enqueue((x)));
            IsWorking = true;
            cancelQueueDownload = new CancellationTokenSource();
            Task.Run(() => StartQueueDownloadImp(cancelQueueDownload.Token));
            eventAggregate.PublishOnUIThread("EngineWorking");
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
                    foreach (var downloadItem in downloadQueue)
                    {
                        if (counterWarpper.Downloading >= maxCurrentlyDownload)
                        {
                            break;
                        }

                        StartDownload(downloadItem);
                        dequeueCount++;
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                    downloadQueue.Dequeue(dequeueCount);
                    dequeueCount = 0;
                    var task = taskList.Select(x => x.Value.First).ToArray();
                    var timeout = Task.Delay(TimeSpan.FromSeconds(10));
                    Task[] waitedTask = new Task[task.Length + 1];
                    task.CopyTo(waitedTask, 1);
                    waitedTask[0] = timeout;
                    var completedTask = await Task.WhenAny(waitedTask);

                    // If completedTask isn't timeout, it's download task
                    if (completedTask != timeout)
                    {
                        // Find completed task from TaskList
                        var completedDownload = taskList.Where(t => t.Value.First.Status == TaskStatus.RanToCompletion ||
                            t.Value.First.Status == TaskStatus.Faulted || t.Value.First.Status == TaskStatus.Canceled).
                            Select(t => t.Key).ToArray(); ;

                        // Delete every task that completed
                        foreach (var downloadInfo in completedDownload)
                        {
                            taskList.Remove(downloadInfo);
                        }
                    }

                    queueRemaining = downloadQueue.Count(file => file.Status == DownloadStatus.Queue);
                    runningTaskRemaining = taskList.Count(t => t.Value.First.Status == TaskStatus.WaitingForActivation);
                    // Check if cancelation is requested
                    ct.ThrowIfCancellationRequested();
                } while (queueRemaining != 0 || runningTaskRemaining != 0);

                IsWorking = false;
                eventAggregate.PublishOnUIThread("EngineNotWorking");
                NotifyCanQueue();
            }
            catch (OperationCanceledException)
            {
                IsWorking = false;
                eventAggregate.PublishOnUIThread("EngineNotWorking");
                NotifyCanQueue();
                // Cancel all of running task
                taskList.Values.Apply(x => x.Second.Cancel());

                await Task.WhenAll(taskList.Select(t => t.Value.First).ToArray());
            }
            catch (Exception ex)
            {
                IsWorking = false;
                eventAggregate.PublishOnUIThread("EngineNotWorking");
                NotifyCanQueue();
                EngineErrorMessage = ex.Message;

                // In case that exception is thrown before polling
                if (!cancelQueueDownload.IsCancellationRequested)
                {
                    cancelQueueDownload.Dispose();
                }
            }
            finally
            {
                // Clean up collection
                downloadQueue.Clear();
                taskList.Clear();
            }
        }

        private void NotifyCanQueue()
        {
            eventAggregate.PublishOnUIThread("CanStartQueue");
            eventAggregate.PublishOnUIThread("CanStopQueue");
        }

        public void StopQueueDownload()
        {
            try
            {
                if (cancelQueueDownload != null)
                {
                    cancelQueueDownload.Cancel();
                    cancelQueueDownload.Dispose();
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}
