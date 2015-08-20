using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Caliburn.Micro;
using TirkxDownloader.Models;
using TirkxDownloader.Framework;

namespace TirkxDownloader.ViewModels
{
    public class QueueViewModel : Screen, IContentList, IHandle<DownloadInfo>, IHandle<string>
    {
        private DownloadInfo selectedItem;
        private readonly IEventAggregator eventAggregator;
        private readonly DownloadEngine engine;

        public BindableCollection<DownloadInfo> QueueDownloadList { get; private set; }

        public QueueViewModel(IEventAggregator eventAggregator, DownloadEngine engine)
        {
            this.eventAggregator = eventAggregator;
            this.engine = engine;
            DisplayName = "Queue/Downloading";
            QueueDownloadList = new BindableCollection<DownloadInfo>();

            this.eventAggregator.Subscribe(this);
        }

        #region notify methods
        public DownloadInfo SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                NotifyOfPropertyChange(() => SelectedItem);
                NotifyOfPropertyChange(() => CanDownload);
                NotifyOfPropertyChange(() => CanStop);
                NotifyOfPropertyChange(() => CanDelete);
            }
        }

        public bool CanDownload
        {
            get
            {
                return SelectedItem != null && (SelectedItem.Status == DownloadStatus.Queue ||
                    SelectedItem.Status == DownloadStatus.Error);
            }
        }

        public bool CanStop
        {
            get { return SelectedItem != null && (SelectedItem.Status == DownloadStatus.Downloading || 
                SelectedItem.Status == DownloadStatus.Preparing); }
        }

        public bool CanDelete
        {
            get { return SelectedItem != null; }
        }

        public bool CanStartQueue
        {
            get { return !engine.IsWorking; }
        }

        public bool CanStopQueue
        {
            get { return engine.IsWorking; }
        }

        public void SelectItem(DownloadInfo info)
        {
            SelectedItem = info;
        }

        public bool IsEmpty
        {
            get { return QueueDownloadList.Count == 0; }
        }
        #endregion

        #region Hanndle message method
        // Use to invoke NotifyOfPropertyChange from LoadingDetail instance
        public void Handle(string propertyName)
        {
            if (propertyName.Equals("CanDownload"))
            {
                NotifyOfPropertyChange(propertyName);
            }
            else if (propertyName.Equals("CanStop"))
            {
                NotifyOfPropertyChange(propertyName);
            }
            else if (propertyName.Equals("CanStartQueue"))
            {
                NotifyOfPropertyChange(propertyName);
            }
            else if (propertyName.Equals("CanStopQueue"))
            {
                NotifyOfPropertyChange(propertyName);
            }
        }

        // Receive message from add windows to queue
        public void Handle(DownloadInfo message)
        {
            QueueDownloadList.Add(message);
            NotifyOfPropertyChange(() => IsEmpty);
        }
        #endregion

        public void Download()
        {
            engine.StartDownload(SelectedItem);
        }

        public void StartQueue()
        {
            engine.StartQueueDownload(QueueDownloadList);
        }

        public void Delete()
        {
            QueueDownloadList.Remove(SelectedItem);
            NotifyOfPropertyChange(() => SelectedItem);
            NotifyOfPropertyChange(() => IsEmpty);
        }

        public void Stop()
        {
            engine.StopDownload(selectedItem);
        }

        public void StopQueue()
        {
            engine.StopQueueDownload();
        }

        
    }
}
