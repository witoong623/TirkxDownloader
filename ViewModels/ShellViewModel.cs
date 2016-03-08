using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Caliburn.Micro;
using TirkxDownloader.Framework;
using TirkxDownloader.Framework.Interface;
using TirkxDownloader.Services;
using TirkxDownloader.ViewModels.Settings;

namespace TirkxDownloader.ViewModels
{
    public class ShellViewModel : Conductor<IContentList>.Collection.OneActive, IHandle<HttpDownloadLink>
    {
        private string _queueEngineMessage;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly FileHostingUtil _detailProvider;
        private readonly IMessageReciever<HttpDownloadLink> _reciever;
        private readonly CancellationTokenSource _cts;
        private readonly IDownloader _downloader;

        public string QueueEngineMessage
        {
            get { return _queueEngineMessage; }
            set
            {
                _queueEngineMessage = value;
                NotifyOfPropertyChange();
            }
        }

        public IDownloader Downloader { get { return _downloader; } }
        
        public ShellViewModel(IEnumerable<IContentList> screen, IWindowManager windowManager,
            IEventAggregator eventAggregator, IMessageReciever<HttpDownloadLink> messageRevicer,
            IDownloader downloader, FileHostingUtil detailProvider)
        {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _reciever = messageRevicer;
            _detailProvider = detailProvider;
            _downloader = downloader;
            _cts = new CancellationTokenSource();

            _eventAggregator.Subscribe(this);
            Items.AddRange(screen);
        }

        protected override void OnInitialize()
        {
            DisplayName = "Tirkx Downloader 0.1 beta";
            ActivateItem(Items[0]);
            _reciever.Prefixes.Add("http://localhost:6230/");
            _reciever.StartSendEvent(_cts.Token);
        }

        public override void CanClose(Action<bool> callback)
        {
            DialogResult result = DialogResult.Yes;

            if (_downloader.DownloadingItems != 0)
            {
                var confirmDialog = DialogViewModel.CreateDialog(DialogType.Confirmation, "Do you really want to close?",
                    "There is item being download\nDo you want to stop and close it?");
                _windowManager.ShowDialog(confirmDialog);

                result = confirmDialog.DialogResult;
            }

            if (result == DialogResult.Yes)
            {
                callback(true);
            }
            else
            {
                callback(false);
            }
        }

        public void OpenSetting()
        {
            _windowManager.ShowDialog(IoC.Get<SettingShellViewModel>());
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                _downloader.StopDownloadItems(null);
                _reciever.Close();
            }
        }

        public void Handle(HttpDownloadLink message)
        {
            _windowManager.ShowDialog(new NewDownloadViewModel(_eventAggregator, message, _downloader, _detailProvider));
        }
    }
}
