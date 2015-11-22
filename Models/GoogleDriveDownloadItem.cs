using System;
using TirkxDownloader.Services;

namespace TirkxDownloader.Models
{
    public class GoogleDriveDownloadItem : GeneralDownloadItem, GoogleFileHosting.IGoogleDriveFile
    {
        private string _fileId;

        public string FileId
        {
            get { return _fileId; }
            set
            {
                _fileId = value;
                NotifyOfPropertyChange(nameof(FileId));
            }
        }
    }
}
