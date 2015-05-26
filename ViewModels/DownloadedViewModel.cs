using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace TirkxDownloader.ViewModels
{
    public class DownloadedViewModel : Screen, IContentList
    {
        public DownloadedViewModel()
        {
            DisplayName = "Downloaded List";
        }
    }
}
