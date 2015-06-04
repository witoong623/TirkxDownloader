using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Caliburn.Micro;
using TirkxDownloader.Framework;

namespace TirkxDownloader.ViewModels
{
    public class QueueViewModel : Screen, IContentList, IHandle<DownloadInfo>
    {
        private DownloadInfo selectedItem;
        private readonly IEventAggregator EventAggregator;

        public BindableCollection<DownloadInfo> QueueDownloadList { get; private set; }

        public QueueViewModel(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            DisplayName = "Queue/Downloading";
            QueueDownloadList = new BindableCollection<DownloadInfo>();

            EventAggregator.Subscribe(this);
        }

        public bool IsEmpty
        {
            get { return QueueDownloadList.Count == 0; }
        }

        public DownloadInfo SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                NotifyOfPropertyChange(() => SelectedItem);
                NotifyOfPropertyChange(() => CanDownload);
                NotifyOfPropertyChange(() => CanStop);
                NotifyOfPropertyChange(() => CanDelete);
            }
        }

        public bool CanDownload
        {
            get { return SelectedItem != null && SelectedItem.Status == DownloadStatus.Queue; }
        }

        public bool CanStop
        {
            get { return SelectedItem != null && SelectedItem.Status == DownloadStatus.Downloading; }
        }

        public bool CanDelete
        {
            get { return SelectedItem != null; }
        }

        protected override void OnInitialize()
        {
            for (int i = 0; i < 5; i++)
            {
                QueueDownloadList.Add(new DownloadInfo
                {   
                    FileName = "School days",
                    DownloadLink = "tirkx download thread",
                    SaveLocation = "D drive",
                    AddDate = DateTime.Now,
                    CompleteDate = null,
                    Status = DownloadStatus.Complete
                });
            }

            QueueDownloadList.Add(new DownloadInfo
            {
                FileName = "Shin sekai yori",
                DownloadLink = "tirkx download thread",
                SaveLocation = "D drive",
                AddDate = DateTime.Now,
                CompleteDate = null,
                Status = DownloadStatus.Downloading
            });
        }

        public void SelectItem(DownloadInfo info)
        {
            SelectedItem = info;
        }

        public void Download()
        {
            SelectedItem.Status = DownloadStatus.Downloading;
            NotifyOfPropertyChange(() => SelectedItem);
            NotifyOfPropertyChange(() => CanDownload);
            NotifyOfPropertyChange(() => CanStop);
        }

        public void Delete()
        {
            QueueDownloadList.Remove(SelectedItem);
            NotifyOfPropertyChange(() => SelectedItem);
            NotifyOfPropertyChange(() => IsEmpty);
        }

        public void Stop()
        {
            SelectedItem.Status = DownloadStatus.Queue;
            NotifyOfPropertyChange(() => SelectedItem);
            NotifyOfPropertyChange(() => CanDownload);
            NotifyOfPropertyChange(() => CanStop);
        }

        public void Handle(DownloadInfo message)
        {
            QueueDownloadList.Add(message);
        }
    }
}
