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
    class QueueViewModel : Screen, IContentList
    {
        private BindableCollection<DownloadInfo> downloadInfoList;
        private DownloadInfo selectedItem;

        public BindableCollection<DownloadInfo> DownloadInfoList
        {
            get { return downloadInfoList; }
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

        public QueueViewModel()
        {
            DisplayName = "Queue/Downloading";
            downloadInfoList = new BindableCollection<DownloadInfo>();
        }

        protected override void OnInitialize()
        {
            for (int i = 0; i < 10; i++)
            {
                downloadInfoList.Add(new DownloadInfo
                {   
                    FileName = "School days",
                    DownloadLink = "tirkx download thread",
                    SaveLocation = "D drive",
                    AddDate = DateTime.Now,
                    CompleteDate = null,
                    Status = DownloadStatus.Queue
                });
            }

            downloadInfoList.Add(new DownloadInfo
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
            DownloadInfoList.Remove(SelectedItem);
            NotifyOfPropertyChange(() => SelectedItem);
        }

        public void Stop()
        {
            SelectedItem.Status = DownloadStatus.Queue;
            NotifyOfPropertyChange(() => SelectedItem);
            NotifyOfPropertyChange(() => CanDownload);
            NotifyOfPropertyChange(() => CanStop);
        }

        public void UnSelect()
        {
            Debug.WriteLine("UnSelect was called");
        }
    }
}
