﻿using System;
using System.Collections.Generic;
using System.IO;
using Caliburn.Micro;
using Microsoft.WindowsAPICodePack.Dialogs;
using MahApps.Metro.Controls;
using TirkxDownloader.Models;
using TirkxDownloader.Framework;

namespace TirkxDownloader.ViewModels
{
    public class NewDownloadViewModel : Screen
    {
        private readonly DownloadEngine Engine;
        private readonly IWindowManager WindowManager;
        private readonly IEventAggregator EventAggregator;
        private MetroWindow View;

        public DownloadInfo CurrentItem { get; private set; }

        public bool CanQueue
        {
            get { return Directory.Exists(CurrentItem.SaveLocation); }
        }

        public bool CanDownload
        {
            get { return CanQueue; }
        }

        public NewDownloadViewModel(IWindowManager windowManager,
            IEventAggregator eventAggregator, TirkxFileInfo fileInfo, DownloadEngine engine)
        {
            WindowManager = windowManager;
            EventAggregator = eventAggregator;
            Engine = engine;
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
                NotifyOfPropertyChange(() => CanDownload);
                NotifyOfPropertyChange(() => CanQueue);
            }
        }

        public void Download()
        {
            CurrentItem.AddDate = DateTime.Now;
            EventAggregator.PublishOnUIThread(CurrentItem);
            Engine.StartDownload(CurrentItem);

            var window = (MetroWindow)GetView();
            window.Close();
        }

        public void Queue()
        {
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

        protected override void OnActivate()
        {
            base.OnActivate();
            var window = (MetroWindow)GetView();
            window.Focus();
        }
    }
}