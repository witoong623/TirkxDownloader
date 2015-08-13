using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace TirkxDownloader.ViewModels
{
    public class AuthorizationViewModel : Screen, IContentList
    {
        public AuthorizationViewModel()
        {
            DisplayName = "Authorization";
        }

        public bool IsEmpty { get { return false; } }
    }
}
