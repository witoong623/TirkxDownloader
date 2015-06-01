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
        private readonly IWindowManager windowManager;

        public DownloadInfo CurrentItem { get; set; }

        public NewDownloadViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
            DisplayName = "New download file";
            // For testing
            CurrentItem = new DownloadInfo
            {
                FileName = "Saenai Kanojo no Sodatekata",
                DownloadLink = "www.tirkx.com/Saenai Kanojo no Sodatekata",
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

            var window = (MetroWindow)GetView();
            window.Focus();
        }

        public void Download()
        {

        }

        public void Queue()
        {

        }

        public void Cancel()
        {
            var window = (MetroWindow)GetView();
            window.Close();
        }
    }
}
