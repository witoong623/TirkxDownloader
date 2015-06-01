using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using TirkxDownloader.Views;

namespace TirkxDownloader.ViewModels
{
    public class ShellViewModel : Conductor<IContentList>.Collection.OneActive
    {
        private readonly IWindowManager windowManager;
        // IContentList implementations will be injected by IoC 
        public ShellViewModel(IEnumerable<IContentList> view, IWindowManager windowManager)
        {
            DisplayName = "Tirkx Downloader";
            this.windowManager = windowManager;

            // Add ViewModel to screen collection
            Items.AddRange(view);

            // Initial view to Anime list
            ActivateItem(Items[0]);
        }
    }
}
