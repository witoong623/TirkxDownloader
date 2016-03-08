using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.WindowsAPICodePack.Dialogs;
using MetroRadiance.UI.Controls;
using TirkxDownloader.Models;
using TirkxDownloader.Framework;
using TirkxDownloader.Framework.Interface;
using TirkxDownloader.Services;
using Nito.AsyncEx;

namespace TirkxDownloader.ViewModels
{
    public class NewDownloadViewModel : Screen
    {
        private IDownloader _downloader;
        private IDownloadItem _currentItem;
        private IEventAggregator _eventAggregator;
        private FileHostingUtil _detailProvider;
        private HttpDownloadLink _httpDownloadLink;
        private CancellationTokenSource _cts;

        #region constructor
        public NewDownloadViewModel(IEventAggregator eventAggregator, HttpDownloadLink fileInfo,
            IDownloader downloader, FileHostingUtil detailProvide)
        {
            _eventAggregator = eventAggregator;
            _downloader = downloader;
            _detailProvider = detailProvide;
            _httpDownloadLink = fileInfo;
            _cts = new CancellationTokenSource();

            /*if (_detailProvider.CheckFileHosting(fileInfo.DownloadLink) == HostingProvider.GoogleDrive)
            {
                CurrentItem = new GoogleDriveDownloadItem
                {
                    FileName = fileInfo.FileName,
                    DownloadLink = fileInfo.DownloadLink
                };
            }
            else
            {
                CurrentItem = new GeneralDownloadItem
                {
                    FileName = fileInfo.FileName,
                    DownloadLink = fileInfo.DownloadLink
                };
            }*/
        }
        #endregion

        #region properties
        public IDownloadItem CurrentItem
        {
            get { return _currentItem; }
            set
            {
                _currentItem = value;
                NotifyOfPropertyChange();
            }
        }
        public INotifyTaskCompletion<IDownloadItem> CreateDownloadItemNotify { get; private set; }

        public bool CanQueue
        {
            get { return CurrentItem != null && Directory.Exists(CurrentItem.SaveLocation) && CreateDownloadItemNotify.IsSuccessfullyCompleted; }
        }

        public bool CanDownload
        {
            get { return CanQueue; }
        }
        #endregion

        protected override void OnInitialize()
        {
            DisplayName = "New download file";
            Task<IDownloadItem> createDownloadTask = _detailProvider.CreateDownloadFile(_httpDownloadLink, _cts.Token);
            CreateDownloadItemNotify = NotifyTaskCompletion.Create(createDownloadTask);

            CreateDownloadItemNotify.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName.Equals(nameof(CreateDownloadItemNotify.IsSuccessfullyCompleted)))
                {
                    CurrentItem = CreateDownloadItemNotify.Result;
                    NotifyOfPropertyChange(nameof(CanQueue));
                    NotifyOfPropertyChange(nameof(CanDownload));
                }
            };
        }

        public void BrowseFolder()
        {
            var folderBrowser = new CommonOpenFileDialog("Select folder");
            folderBrowser.IsFolderPicker = true;

            if (folderBrowser.ShowDialog() == CommonFileDialogResult.Ok)
            {
                CurrentItem.SaveLocation = folderBrowser.FileName;
                NotifyOfPropertyChange(nameof(CanDownload));
                NotifyOfPropertyChange(nameof(CanQueue));
            }
        }

        public void Download()
        {
            CurrentItem.AddOn = DateTime.Now;
            _eventAggregator.PublishOnUIThread(CurrentItem);
            _downloader.DownloadItem(CurrentItem);

            var window = (MetroWindow)GetView();
            window.Close();
        }

        public void Queue()
        {
            CurrentItem.AddOn = DateTime.Now;
            _eventAggregator.PublishOnUIThread(CurrentItem);

            var window = (MetroWindow)GetView();
            window.Close();
        }

        public void Cancel()
        {
            _cts.Cancel();
            var window = (MetroWindow)GetView();
            window.Close();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            var window = (MetroWindow)GetView();
            window.Focus();
        }
    }
}
