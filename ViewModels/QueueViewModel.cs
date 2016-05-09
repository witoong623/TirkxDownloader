using Caliburn.Micro;
using MetroRadiance.UI.Controls;
using TirkxDownloader.Framework.Interface;
using TirkxDownloader.Framework.Message;
using TirkxDownloader.Models;
using TirkxDownloader.Models.Settings;
using TirkxDownloader.ViewModels;

namespace TirkxDownloader.ViewModels
{
    public class QueueViewModel : Screen, IContentList, IHandle<GeneralDownloadItem>, IHandle<string>
    {
        private GeneralDownloadItem _selectedItem;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDownloader _downloader;
        private readonly IWindowManager _windowManager;

        public BindableCollection<GeneralDownloadItem> QueueDownloadList { get; private set; }

        public QueueViewModel(IEventAggregator eventAggregator, IDownloader engine, IWindowManager windowManager)
        {
            _eventAggregator = eventAggregator;
            _downloader = engine;
            _windowManager = windowManager;
            
            QueueDownloadList = new BindableCollection<GeneralDownloadItem>();

            this._eventAggregator.Subscribe(this);
        }

        #region notify methods
        public GeneralDownloadItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;

                NotifyOfPropertyChange(nameof(SelectedItem));
                NotifyOfPropertyChange(nameof(CanDownload));
                NotifyOfPropertyChange(nameof(CanStop));
                NotifyOfPropertyChange(nameof(CanDelete));
            }
        }

        public bool CanDownload
        {
            get
            {
                return SelectedItem != null && (SelectedItem.Status == DownloadStatus.Queue ||
                    SelectedItem.Status == DownloadStatus.Error || SelectedItem.Status == DownloadStatus.Stop);
            }
        }

        public bool CanStop
        {
            get
            {
                return SelectedItem != null && (SelectedItem.Status == DownloadStatus.Downloading ||
                    SelectedItem.Status == DownloadStatus.Preparing);
            }
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

        public void SelectItem(GeneralDownloadItem info)
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
        public void Handle(GeneralDownloadItem message)
        {
            QueueDownloadList.Add(message);
            NotifyOfPropertyChange(nameof(IsEmpty));
        }
        #endregion

        public void Download()
        {
            
            _downloader.DownloadItem(SelectedItem);
        }

        public void Download(IDownloadItem info)
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
            _downloader.StopDownloadItem(SelectedItem);

            NotifyOfPropertyChange(nameof(SelectedItem));
            NotifyOfPropertyChange(nameof(IsEmpty));
        }

        public void Stop()
        {
            _downloader.StopDownloadItem(_selectedItem);
        }

        public void Stop(IDownloadItem info)
        {
            _downloader.StopDownloadItem(info);
        }

        public void StopQueue()
        {
            _downloader.StopDownloadItems(null);
        }

        public void BandwidthThrottling(GeneralDownloadItem info)
        {
            if (info == null || info.InStream == null)
            {
                return;
            }

            MetroWindow metroWindow = (MetroWindow)((Screen)Parent).GetView();
            var dialog = DialogViewModel<long>.CreateDialog("Bandwidth Control", "Enter bandwidth to use 0 for unlimited (in KB/s)",
                DownloadingSetting.MaximumBytesPerSec.Value / 1024);
            _windowManager.ShowDialog(dialog);

            if (dialog.DialogResult == DialogResult.Cancel) return;

            long bandwidth = dialog.InputResult;

            if (DownloadingSetting.MaximumBytesPerSec.Value == bandwidth) return;

            _eventAggregator.PublishOnUIThread(new MaxBpsUpdate(bandwidth * 1024));
            DownloadingSetting.MaximumBytesPerSec.Value = bandwidth * 1024;
        }
    }
}
