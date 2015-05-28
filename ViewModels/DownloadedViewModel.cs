using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using TirkxDownloader.Framework;

namespace TirkxDownloader.ViewModels
{
    public class DownloadedViewModel : Screen, IContentList
    {
        public BindableCollection<DownloadInfo> DownloadInfoList { get; set; }
        public DownloadInfo SelectedItem { get; set; }

        public DownloadedViewModel()
        {
            DisplayName = "Downloaded List";
        }

        public bool CanOpen
        {
            get { return SelectedItem != null; }
        }

        public bool CanDelete
        {
            get { return SelectedItem != null; }
        }

        public void Open()
        {

        }

        public void Delete()
        {
            DownloadInfoList.Remove(SelectedItem);
            NotifyOfPropertyChange(() => CanOpen);
            NotifyOfPropertyChange(() => CanDelete);
        }
    }
}
