using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using TirkxDownloader.Framework;

namespace TirkxDownloader.Models
{
    public class DownloadEngine : PropertyChangedBase
    {
        private int MaxCurrentlyDownload;
        private string engineErrorMessage;
        private CounterWarpper CurrentlyDownload;
        private Thread QueueWorker;
        private Dictionary<DownloadInfo, Thread> WorkerTheradList;
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
            WorkerTheradList = new Dictionary<DownloadInfo, Thread>();
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
            var workerThread = new Thread(downloadProgress.StartProgress);
            WorkerTheradList.Add(downloadInfo, workerThread);
            workerThread.Start(new ThreadParameter 
            { 
                Counter = CurrentlyDownload, 
                EventAggregate = EventAggregate, 
                DownloadInformation = downloadInfo 
            });
        }

        public void StopDownload(DownloadInfo downloadInfo)
        {
            try
            {
                var workerThread = WorkerTheradList[downloadInfo];

                if (workerThread.IsAlive)
                {
                    workerThread.Abort();
                }

                WorkerTheradList.Remove(downloadInfo);
            }
            catch (KeyNotFoundException)
            {
                return;
            }
        }

        public void StartQueueDownload(BindableCollection<DownloadInfo> downloadInfoList)
        {
            QueueWorker = new Thread(StartQueueDownloadImp);
            QueueWorker.Start(downloadInfoList);
            IsWorking = true;
            EventAggregate.PublishOnUIThread("EngineWorking");
        }

        private void StartQueueDownloadImp(object param)
        {
            try
            {
                var downloadInfoList = (BindableCollection<DownloadInfo>)param;
                int workRemaining = 0;

                do
                {
                    for (int i = 0; i < downloadInfoList.Count; i++)
                    {
                        if (CurrentlyDownload.Counter >= MaxCurrentlyDownload)
                        {
                            break;
                        }

                        if (downloadInfoList[i].Status == DownloadStatus.Queue || downloadInfoList[i].Status == DownloadStatus.Error)
                        {
                            StartDownload(downloadInfoList[i]);
                            Thread.Sleep(1000);
                        }
                    }

                    var downloading = from file in downloadInfoList
                                      where file.Status == DownloadStatus.Downloading || file.Status == DownloadStatus.Preparing
                                      select file;

                    foreach (var file in downloading)
                    {
                        var workerThread = WorkerTheradList[file];
                        workerThread.Join(5000);
                    }

                    workRemaining = downloadInfoList.Count(file => file.Status != DownloadStatus.Downloading || 
                        file.Status != DownloadStatus.Complete);

                } while (workRemaining != 0);

                EventAggregate.PublishOnUIThread("EngineNotWorking");
                IsWorking = false;
            }
            catch (ThreadAbortException)
            {
                EventAggregate.PublishOnUIThread("EngineNotWorking");
                IsWorking = false;

                return;
            }
            catch (Exception ex)
            {
                EventAggregate.PublishOnUIThread("EngineNotWorking");
                EngineErrorMessage = ex.Message;
                IsWorking = false;

                return;
            }
        }

        public void StopQueueDownload()
        {
            if (QueueWorker == null)
            {
                return;
            }
            else if (!QueueWorker.IsAlive)
            {
                return;
            }
            else
            {
                QueueWorker.Abort();
            }

            IsWorking = false;

            foreach (var file in WorkerTheradList)
            {
                var workerThread = file.Value;

                if (workerThread.IsAlive)
                {
                    workerThread.Abort();
                }
            }

            var completeThread = (from file in WorkerTheradList
                                  where file.Key.Status == DownloadStatus.Complete || file.Key.Status == DownloadStatus.Stop
                                  select file).ToArray();

            for (int i = 0; i < completeThread.Length; i++)
            {
                WorkerTheradList.Remove(completeThread[i].Key);
            }
        }
    }
}
