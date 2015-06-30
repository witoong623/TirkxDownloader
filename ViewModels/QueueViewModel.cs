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
        private readonly IEventAggregator EventAggregator;
        private readonly DownloadEngine Engine;

        public BindableCollection<DownloadInfo> QueueDownloadList { get; private set; }

        public QueueViewModel(IEventAggregator eventAggregator, DownloadEngine engine)
        {
            EventAggregator = eventAggregator;
            Engine = engine;
            DisplayName = "Queue/Downloading";
            QueueDownloadList = new BindableCollection<DownloadInfo>();

            EventAggregator.Subscribe(this);
        }

        public bool IsEmpty
        {
            get { return QueueDownloadList.Count == 0; }
        }

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

        public void SelectItem(DownloadInfo info)
        {
            SelectedItem = info;
        }

        public void Download()
        {
            Engine.StartDownload(SelectedItem);
        }

        public void StartQueue()
        {
            Engine.StartQueueDownload(QueueDownloadList);
        }

        public void Delete()
        {
            QueueDownloadList.Remove(SelectedItem);
            NotifyOfPropertyChange(() => SelectedItem);
            NotifyOfPropertyChange(() => IsEmpty);
        }

        public void Stop()
        {
            Engine.StopDownload(selectedItem);
        }

        public void StopQueue()
        {
            Engine.StopQueueDownload();
        }

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
        }

        // Receive message from add windows to queue
        public void Handle(DownloadInfo message)
        {
            QueueDownloadList.Add(message);
            NotifyOfPropertyChange(() => IsEmpty);
        }
    }
}
