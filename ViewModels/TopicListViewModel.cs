using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace TirkxDownloader.ViewModels
{
    public class TopicListViewModel : Screen, IContentList
    {
        public TopicListViewModel()
        {
            DisplayName = "Anime List";
        }
    }
}
