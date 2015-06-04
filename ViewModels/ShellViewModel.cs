using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using TirkxDownloader.Views;
using TirkxDownloader.Framework;
using TirkxDownloader.Models;

namespace TirkxDownloader.ViewModels
{
    public class ShellViewModel : Conductor<IContentList>.Collection.OneActive
    {
        private readonly IWindowManager WindowManager;
        private readonly IEventAggregator EventAggregator;
        private readonly MessageReciever Reciever;
        
        public ShellViewModel(QueueViewModel queueViewModel, DownloadedViewModel downloadViewModel,
            IWindowManager windowManager, IEventAggregator eventAggregator, MessageReciever messageRevicer)
        {
            DisplayName = "Tirkx Downloader";
            WindowManager = windowManager;
            EventAggregator = eventAggregator;
            Reciever = messageRevicer;

            Items.Add(queueViewModel);
            Items.Add(downloadViewModel);
            ActivateItem(Items[0]);
            messageRevicer.Start();
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                Reciever.Stop();
            }
        }
    }
}
