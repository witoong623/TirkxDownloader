using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.WindowsAPICodePack.Dialogs;
using MahApps.Metro.Controls;
using TirkxDownloader.Framework;

namespace TirkxDownloader.ViewModels
{
    public class NewDownloadViewModel : Screen
    {
        private readonly IWindowManager WindowManager;
        private readonly IEventAggregator EventAggregator;
        private MetroWindow View;

        public DownloadInfo CurrentItem { get; private set; }

        public NewDownloadViewModel(IWindowManager windowManager,
            IEventAggregator eventAggregator, TirkxFileInfo fileInfo)
        {
            WindowManager = windowManager;
            EventAggregator = eventAggregator;
            DisplayName = "New download file";
            View = (MetroWindow)GetView();

            CurrentItem = new DownloadInfo
            {
                FileName = fileInfo.FileName,
                DownloadLink = fileInfo.DownloadLink
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
            }
        }

        public void Download()
        {
            CurrentItem.Status = DownloadStatus.Downloading;
            CurrentItem.AddDate = DateTime.Now;
            EventAggregator.PublishOnUIThread(CurrentItem);

            var window = (MetroWindow)GetView();
            window.Close();
        }

        public void Queue()
        {
            CurrentItem.Status = DownloadStatus.Queue;
            CurrentItem.AddDate = DateTime.Now;
            EventAggregator.PublishOnUIThread(CurrentItem);

            var window = (MetroWindow)GetView();
            window.Close();
        }

        public void Cancel()
        {
            var window = (MetroWindow)GetView();
            window.Close();
        }
    }
}
