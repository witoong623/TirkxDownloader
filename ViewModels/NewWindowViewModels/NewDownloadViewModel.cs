using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.WindowsAPICodePack.Dialogs;
using MahApps.Metro.Controls;
using TirkxDownloader.Models;
using TirkxDownloader.Framework;
using TirkxDownloader.Framework.Interface;
using TirkxDownloader.Services;
using Nito.AsyncEx;

namespace TirkxDownloader.ViewModels
{
    public class NewDownloadViewModel : Screen
    {
        // private INotifyTaskCompletion _detailDownloadTask;
        private readonly IDownloader _downloader;
        private readonly IEventAggregator _eventAggregator;
        private readonly DetailProvider _detailProvider;

        public DownloadInfo CurrentItem { get; }
        public INotifyTaskCompletion DetailDownloadTask { get; private set; }

        public bool CanQueue
        {
            get { return Directory.Exists(CurrentItem.SaveLocation) && DetailDownloadTask.IsSuccessfullyCompleted; }
        }

        public bool CanDownload
        {
            get { return CanQueue; }
        }

        public NewDownloadViewModel(IEventAggregator eventAggregator,
            HttpDownloadLink fileInfo, IDownloader downloader, DetailProvider detailProvide)
        {
            _eventAggregator = eventAggregator;
            _downloader = downloader;
            _detailProvider = detailProvide;

            CurrentItem = new DownloadInfo
            {
                FileName = fileInfo.FileName,
                DownloadLink = fileInfo.DownloadLink
            };
        }

        protected override void OnInitialize()
        {
            DisplayName = "New download file";
            Task getDetailTask = _detailProvider.TestFileAvailable(CurrentItem, CancellationToken.None);
            DetailDownloadTask = NotifyTaskCompletion.Create(getDetailTask);

            DetailDownloadTask.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName.Equals(nameof(DetailDownloadTask.IsSuccessfullyCompleted)))
                {
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
                NotifyOfPropertyChange(() => CurrentItem);
                NotifyOfPropertyChange(() => CanDownload);
                NotifyOfPropertyChange(() => CanQueue);
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
