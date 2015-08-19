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
    public class SettingShellViewModel : Conductor<ISetting>.Collection.OneActive
    {
        public SettingShellViewModel(IEnumerable<ISetting> screen)
        {
            DisplayName = "Setting";
            Items.AddRange(screen);
        }

        protected override void OnActivate()
        {
            ActivateItem(Items[0]);
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                Items.OfType<IDeactivate>().Apply(x => x.Deactivate(true));
            }
            else
            {
                ScreenExtensions.TryDeactivate(ActiveItem, false);
            }
        }
    }
}
