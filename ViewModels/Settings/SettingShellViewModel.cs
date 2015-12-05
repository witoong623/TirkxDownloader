using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using TirkxDownloader.Framework.Interface;

namespace TirkxDownloader.ViewModels.Settings
{
    public class SettingShellViewModel : Conductor<ISetting>.Collection.OneActive
    {
        private SettingState _state;
        public SettingShellViewModel(IEnumerable<ISetting> screen)
        {
            DisplayName = "Setting";
            Items.AddRange(screen);
            State = SettingState.Summary;
        }

        protected override void OnActivate()
        {
            ActivateItem(Items[0]);
        }

        public SettingState State
        {
            get { return _state; }
            set
            {
                _state = value;
                NotifyOfPropertyChange(nameof(State));
            }
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

    public enum SettingState
    {
        Summary,
        Modify
    };
}
