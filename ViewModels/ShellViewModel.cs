using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using TirkxDownloader.Views;
using TirkxDownloader.Framework;
using TirkxDownloader.Models;

namespace TirkxDownloader.ViewModels
{
    public class ShellViewModel : Conductor<IContentList>.Collection.OneActive, IHandle<string>
    {
        private string queueEngineMessage;
        private readonly SettingShellViewModel settingScreen;
        private readonly IWindowManager windowManager;
        private readonly IEventAggregator eventAggregator;
        private readonly MessageReciever reciever;

        public string QueueEngineMessage
        {
            get { return queueEngineMessage; }
            set
            {
                queueEngineMessage = value;
                NotifyOfPropertyChange(() => QueueEngineMessage);
            }
        }

        public DownloadEngine Downloader { get; private set; }
        
        public ShellViewModel(IEnumerable<IContentList> screen, IWindowManager windowManager, SettingShellViewModel setting,
            IEventAggregator eventAggregator, MessageReciever messageRevicer, DownloadEngine engine)
        {
            settingScreen = setting;
            this.windowManager = windowManager;
            this.eventAggregator = eventAggregator;
            reciever = messageRevicer;
            Downloader = engine;
            DisplayName = "Tirkx Downloader 0.1 beta";
            queueEngineMessage = "Engine isn't working";

            Items.AddRange(screen);
            ActivateItem(Items[0]);
            this.eventAggregator.Subscribe(this);
            messageRevicer.Start();
        }

        public override async void CanClose(Action<bool> callback)
        {
            var dialogResult = MessageDialogResult.Affirmative;

            if (Downloader.Downloading != 0)
            {
                var metroWindow = (MetroWindow)GetView();
                dialogResult = await metroWindow.ShowMessageAsync("Do you really want to close?", "There is item being download\nDo you want to stop and close it?",
                    MessageDialogStyle.AffirmativeAndNegative);
            }

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                callback(true);
            }
            else if (dialogResult == MessageDialogResult.Negative)
            {
                callback(false);
            }
        }

        public void OpenSetting()
        {
            windowManager.ShowWindow(settingScreen);
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                reciever.Stop();
                Downloader.StopQueueDownload();
            }
        }

        public void Handle(string message)
        {
            if (message.Equals("EngineWorking"))
            {
                QueueEngineMessage = "Engine is working";
            }
            else if (message.Equals("EngineNotWorking"))
            {
                QueueEngineMessage = "Engine isn't working";
            }
        }
    }
}
