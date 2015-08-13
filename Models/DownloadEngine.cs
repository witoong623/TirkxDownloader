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
        private int MaxCurrentlyDownload;
        private string engineErrorMessage;
        private CounterWarpper CurrentlyDownload;
        private CancellationTokenSource CancelQueueDownload;
        private Queue<DownloadInfo> DownloadQueue;
        private Dictionary<DownloadInfo, Pair<Task, CancellationTokenSource>> TaskList;
        private IEventAggregator EventAggregate;

        public bool IsWorking { get; private set; }

        public int CurrentDownload
        {
            get { return CurrentlyDownload.Counter; }
        }

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
            CurrentlyDownload = new CounterWarpper();
            TaskList = new Dictionary<DownloadInfo, Pair<Task, CancellationTokenSource>>();
            DownloadQueue = new Queue<DownloadInfo>();
            EventAggregate = eventAggregator;
            MaxCurrentlyDownload = 1;
        }

        public void StartDownload(DownloadInfo downloadInfo)
        {
            if (CurrentlyDownload.Counter >= MaxCurrentlyDownload)
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
            var downloadTask = Task.Run(() => downloadProgress.StartProgress(downloadInfo, CurrentlyDownload, EventAggregate, cts.Token));
            TaskList.Add(downloadInfo, new Pair<Task, CancellationTokenSource>(downloadTask, cts));
        }

        public void StopDownload(DownloadInfo downloadInfo)
        {
            try
            {
                var taskCancel = TaskList[downloadInfo];
                taskCancel.Second.Cancel();
                TaskList.Remove(downloadInfo);
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

            foreach (var downloadItem in downloadInfoList)
            {
                if (downloadItem.Status != DownloadStatus.Complete && !DownloadQueue.Contains(downloadItem))
                {
                    DownloadQueue.Enqueue(downloadItem);
                }
            }

            Trace.WriteLine("Thread number is " + Thread.CurrentThread.ManagedThreadId);
            IsWorking = true;
            CancelQueueDownload = new CancellationTokenSource();
            Task.Run(() => StartQueueDownloadImp(CancelQueueDownload.Token));
            EventAggregate.PublishOnUIThread("EngineWorking");
            NotifyCanQueue();
        }

        private async Task StartQueueDownloadImp(CancellationToken ct)
        {
            Trace.WriteLine("Thread number is " + Thread.CurrentThread.ManagedThreadId);
            try
            {
                int queueRemaining = 0;
                int runningTaskRemaining = 0;
                int dequeueCount = 0;

                do
                {
                    foreach (var downloadItem in DownloadQueue)
                    {
                        if (CurrentlyDownload.Counter >= MaxCurrentlyDownload)
                        {
                            break;
                        }

                        StartDownload(downloadItem);
                        dequeueCount++;
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                    DownloadQueue.Dequeue(dequeueCount);
                    dequeueCount = 0;
                    var task = TaskList.Select(x => x.Value.First).ToArray();
                    var timeout = Task.Delay(TimeSpan.FromSeconds(10));
                    Task[] waitedTask = new Task[task.Length + 1];
                    task.CopyTo(waitedTask, 1);
                    waitedTask[0] = timeout;
                    var completedTask = await Task.WhenAny(waitedTask);

                    // If completedTask isn't timeout, it's download task
                    if (completedTask != timeout)
                    {
                        // Find completed task from TaskList
                        var completedDownload = TaskList.Where(t => t.Value.First.Status == TaskStatus.RanToCompletion ||
                            t.Value.First.Status == TaskStatus.Faulted || t.Value.First.Status == TaskStatus.Canceled).
                            Select(t => t.Key);

                        // Delete every task that completed
                        foreach (var downloadInfo in completedDownload)
                        {
                            TaskList.Remove(downloadInfo);
                        }
                    }

                    queueRemaining = DownloadQueue.Count(file => file.Status != DownloadStatus.Queue);
                    runningTaskRemaining = TaskList.Count(t => t.Value.First.Status == TaskStatus.WaitingForActivation);

                    // Check if cancelation is requested
                    ct.ThrowIfCancellationRequested();
                } while (queueRemaining != 0 || runningTaskRemaining != 0);

                IsWorking = false;
                EventAggregate.PublishOnUIThread("EngineNotWorking");
                NotifyCanQueue();
            }
            catch (OperationCanceledException)
            {
                IsWorking = false;
                EventAggregate.PublishOnUIThread("EngineNotWorking");
                NotifyCanQueue();
                // Cancel all of running task
                foreach (var cancelToken in TaskList.Values)
                {
                    cancelToken.Second.Cancel();
                }

                await Task.WhenAll(TaskList.Select(t => t.Value.First).ToArray());
            }
            catch (Exception ex)
            {
                IsWorking = false;
                EventAggregate.PublishOnUIThread("EngineNotWorking");
                NotifyCanQueue();
                EngineErrorMessage = ex.Message;

                // In case that exception is thrown before polling
                if (!CancelQueueDownload.IsCancellationRequested)
                {
                    CancelQueueDownload.Dispose();
                }
            }
            finally
            {
                // Clean up collection
                DownloadQueue.Clear();
                TaskList.Clear();
            }
        }

        private void NotifyCanQueue()
        {
            EventAggregate.PublishOnUIThread("CanStartQueue");
            EventAggregate.PublishOnUIThread("CanStopQueue");
        }

        public void StopQueueDownload()
        {
            try
            {
                if (CancelQueueDownload != null)
                {
                    CancelQueueDownload.Cancel();
                    CancelQueueDownload.Dispose();
                }
            }
            catch { }
        }
    }
}
