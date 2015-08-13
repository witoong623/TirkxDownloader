using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using MahApps.Metro.Controls;
using TirkxDownloader.Framework;
using TirkxDownloader.Models;


namespace TirkxDownloader.ViewModels
{
    public class SettingShellViewModel : Conductor<IContentList>.Collection.OneActive
    {
        public SettingShellViewModel()
        {
            DisplayName = "Setting";
            Items.Add(new AuthorizationViewModel());
            ActivateItem(Items[0]);
        }
    }
}
