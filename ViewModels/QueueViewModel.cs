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
            }
        }

        public QueueViewModel()
        {
            DisplayName = "Queue/Downloading";
            downloadInfoList = new BindableCollection<DownloadInfo>();
        }

        protected override void OnInitialize()
        {
            for (int i = 0; i < 30; i++)
            {
                downloadInfoList.Add(new DownloadInfo { FileName = "School days", DownloadLink = "tirkx download thread",
                    SaveDestination = "D drive", AddDate = DateTime.Now });
            }
        }

        public void SelectItem(System.Windows.Controls.ListView listView)
        {
            SelectedItem = (DownloadInfo)listView.SelectedItem;
        }
    }
}
