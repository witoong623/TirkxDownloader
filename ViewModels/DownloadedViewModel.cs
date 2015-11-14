using Caliburn.Micro;
using TirkxDownloader.Models;

namespace TirkxDownloader.ViewModels
{
    public class DownloadedViewModel : Screen, IContentList
    {
        private DownloadInfo _selectedItem;

        public BindableCollection<DownloadInfo> DownloadCompleteList { get; private set; }

        public DownloadedViewModel()
        {
            DisplayName = "Downloaded list";
            DownloadCompleteList = new BindableCollection<DownloadInfo>();
        }

        public DownloadInfo SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                NotifyOfPropertyChange(() => SelectedItem);
                NotifyOfPropertyChange(() => CanDelete);
                NotifyOfPropertyChange(() => CanOpen);
            }
        }

        public bool IsEmpty
        {
            get { return DownloadCompleteList.Count == 0; }
        }

        public bool CanOpen
        {
            get { return SelectedItem != null; }
        }

        public bool CanDelete
        {
            get { return SelectedItem != null; }
        }

        public void SelectItem(DownloadInfo info)
        {
            SelectedItem = info;
        }

        public void Open()
        {

        }

        public void Delete()
        {
            DownloadCompleteList.Remove(SelectedItem);
            NotifyOfPropertyChange(() => CanOpen);
            NotifyOfPropertyChange(() => CanDelete);
            NotifyOfPropertyChange(() => IsEmpty);
        }
    }
}
