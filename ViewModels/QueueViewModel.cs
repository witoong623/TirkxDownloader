using Caliburn.Micro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using TirkxDownloader.Models;
using TirkxDownloader.Framework;
using TirkxDownloader.Framework.Interface;

namespace TirkxDownloader.ViewModels
{
    public class QueueViewModel : Screen, IContentList, IHandle<DownloadInfo>, IHandle<string>
    {
        private DownloadInfo _selectedItem;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDownloader _downloader;

        public BindableCollection<DownloadInfo> QueueDownloadList { get; private set; }

        public QueueViewModel(IEventAggregator eventAggregator, IDownloader engine)
        {
            _eventAggregator = eventAggregator;
            _downloader = engine;
            
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
            get { return !_downloader.IsDownloading; }
        }

        public bool CanStopQueue
        {
            get { return _downloader.IsDownloading; }
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
            _downloader.DownloadItem(SelectedItem);
        }

        public void Download(DownloadInfo info)
        {
            _downloader.DownloadItem(info);
        }

        public void StartQueue()
        {
            _downloader.DownloadItems(QueueDownloadList);
        }

        public void Delete()
        {
            SelectedItem.Status = DownloadStatus.Stop;
            QueueDownloadList.Remove(SelectedItem);
            NotifyOfPropertyChange(() => SelectedItem);
            NotifyOfPropertyChange(() => IsEmpty);
        }

        public void Stop()
        {
            _downloader.StopDownloadItem(_selectedItem);
        }

        public void Stop(DownloadInfo info)
        {
            _downloader.StopDownloadItem(info);
        }

        public void StopQueue()
        {
            _downloader.StopDownloadItems(null);
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
            _downloader.MaximumBytesPerSecond = bandwidth * 1024;
        }
    }
}
