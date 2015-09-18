using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Caliburn.Micro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using TirkxDownloader.Models;
using TirkxDownloader.Framework;

namespace TirkxDownloader.ViewModels
{
    public class QueueViewModel : Screen, IContentList, IHandle<DownloadInfo>, IHandle<string>
    {
        private DownloadInfo _selectedItem;
        private readonly IEventAggregator _eventAggregator;
        private readonly DownloadEngine _engine;

        public BindableCollection<DownloadInfo> QueueDownloadList { get; private set; }

        public QueueViewModel(IEventAggregator eventAggregator, DownloadEngine engine)
        {
            this._eventAggregator = eventAggregator;
            this._engine = engine;
            
            QueueDownloadList = new BindableCollection<DownloadInfo>();

            this._eventAggregator.Subscribe(this);
        }

        #region notify methods
        public DownloadInfo SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
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
            get { return !_engine.IsWorking; }
        }

        public bool CanStopQueue
        {
            get { return _engine.IsWorking; }
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

        protected override void OnInitialize()
        {
            DisplayName = "Queue/Downloading";
        }

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
            _engine.StartDownload(SelectedItem);
        }

        public void Download(DownloadInfo info)
        {
            _engine.StartDownload(info);
        }

        public void StartQueue()
        {
            _engine.StartQueueDownload(QueueDownloadList);
        }

        public void Delete()
        {
            QueueDownloadList.Remove(SelectedItem);
            NotifyOfPropertyChange(() => SelectedItem);
            NotifyOfPropertyChange(() => IsEmpty);
        }

        public void Stop()
        {
            _engine.StopDownload(_selectedItem);
        }

        public void Stop(DownloadInfo info)
        {
            _engine.StopDownload(info);
        }

        public void StopQueue()
        {
            _engine.StopQueueDownload();
        }

        public async void BandwidthThrottling(DownloadInfo info)
        {
            if (info.InStream == null)
            {
                return;
            }

            MetroWindow metroWindow = (MetroWindow)((Screen)Parent).GetView();

            string bandwidthText = await metroWindow.ShowInputAsync(
                "Bandwidth Control", 
                "Enter bandwidth to use 0 for unlimited (in KB/s)",
                new MetroDialogSettings
                {
                    DefaultText = (info.InStream.MaximumBytesPerSecond / 1024).ToString(),
                    AffirmativeButtonText = "OK",
                    NegativeButtonText = "Cancel"
                });

            int bandwidth;
            bool flag = int.TryParse(bandwidthText, out bandwidth);

            if (!flag || bandwidth < 0)
            {
                return;
            }

            info.InStream.MaximumBytesPerSecond = bandwidth * 1024;
            _engine.MaximumBytesPerSecond = bandwidth * 1024;
        }
    }
}
