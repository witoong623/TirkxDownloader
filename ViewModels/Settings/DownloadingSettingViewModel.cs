using System;
using System.Collections.Generic;
using Caliburn.Micro;
using TirkxDownloader.Framework.Interface;
using TirkxDownloader.Framework.Message;
using TirkxDownloader.Models.Settings;

namespace TirkxDownloader.ViewModels.Settings
{
    public class DownloadingSettingViewModel : Screen, ISetting
    {
        private IEventAggregator _eventAggregator;
        #region constructor
        public DownloadingSettingViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            DisplayName = "Downloading setting";

            _maxDownloadingItems = DownloadingSetting.MaxConcurrentDownload.Value;
            _maximumBytesPerSecond = DownloadingSetting.MaximumBytesPerSec.Value;
        }
        #endregion
        #region properties
        public bool IsSet { get; set; }

        public byte MaxDownloadingItems
        {
            get { return _maxDownloadingItems; }
            set
            {
                if (_maxDownloadingItems == value) return;

                IsSet = true;
                _maxDownloadingItems = value;
                NotifyOfPropertyChange();
            }

        }

        private byte _maxDownloadingItems;

        public long MaximumBytesPerSecond
        {
            get { return _maximumBytesPerSecond; }
            set
            {
                if (_maximumBytesPerSecond == value) return;

                IsSet = true;
                _maximumBytesPerSecond = value;
                NotifyOfPropertyChange();
            }
        }

        private long _maximumBytesPerSecond;
        public ICollection<IDisposable> Disposeables { get; } = new List<IDisposable>();
        #endregion

        protected override void OnActivate()
        {
            base.OnActivate();

            var parent = (SettingShellViewModel)Parent;
            parent.State = SettingState.Summary;
        }
        protected override void OnDeactivate(bool close)
        {
            if (close && IsSet)
            {
                DownloadingSetting.MaxConcurrentDownload.Value = MaxDownloadingItems;
                
                if (DownloadingSetting.MaximumBytesPerSec.Value != MaximumBytesPerSecond * 1024)
                {
                    _eventAggregator.PublishOnUIThread(new MaxBpsUpdate(MaximumBytesPerSecond * 1024));
                    DownloadingSetting.MaximumBytesPerSec.Value = MaximumBytesPerSecond * 1024;
                }
            }
        }
    }
}
