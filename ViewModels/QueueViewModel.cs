using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace TirkxDownloader.ViewModels
{
    class QueueViewModel : Screen, IContentList
    {
        public QueueViewModel()
        {
            DisplayName = "Queue/Downloading";
        }
    }
}
