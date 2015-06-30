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
        private readonly IWindowManager WindowManager;
        private readonly IEventAggregator EventAggregator;
        private readonly MessageReciever Reciever;
        private readonly DownloadEngine Engine;

        public string QueueEngineMessage
        {
            get { return queueEngineMessage; }
            set
            {
                queueEngineMessage = value;
                NotifyOfPropertyChange(() => QueueEngineMessage);
            }
        }
        
        public ShellViewModel(QueueViewModel queueViewModel, DownloadedViewModel downloadViewModel,
            IWindowManager windowManager, IEventAggregator eventAggregator, MessageReciever messageRevicer, DownloadEngine engine)
        {
            WindowManager = windowManager;
            EventAggregator = eventAggregator;
            Reciever = messageRevicer;
            Engine = engine;
            DisplayName = "Tirkx Downloader";
            queueEngineMessage = "Engine isn't working";

            Items.Add(queueViewModel);
            Items.Add(downloadViewModel);
            ActivateItem(Items[0]);
            EventAggregator.Subscribe(this);
            messageRevicer.Start();
        }

        public override async void CanClose(Action<bool> callback)
        {
            var DialogResult = MessageDialogResult.Affirmative;

            if (Engine.CurrentDownload != 0)
            {
                var metroWindow = (MetroWindow)GetView();
                DialogResult = await metroWindow.ShowMessageAsync("Do you really want to close?", "There is item being download\nDo you want to stop and close it?",
                    MessageDialogStyle.AffirmativeAndNegative);
            }

            if (DialogResult == MessageDialogResult.Affirmative)
            {
                callback(true);
            }
            else if (DialogResult == MessageDialogResult.Negative)
            {
                callback(false);
            }
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                Reciever.Stop();
                Engine.StopQueueDownload();
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
