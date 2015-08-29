using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using TirkxDownloader.Models;
using TirkxDownloader.Framework;
using MahApps.Metro.Controls;

namespace TirkxDownloader.ViewModels.Settings
{
    public class AuthorizationViewModel : Screen, ISetting
    {
        private const string SAVESUBMIT = "Save";
        private const string CANCEL = "Cancel";
        private const string SAVECHANGE = "Save change";
        private const string UNCHANGE = "Back";
        private string _submitMessage;
        private string _cancelMessage;
        private AuthorizationInfo _currentItem;
        private AuthorizationManager _authorization;

        public BindableCollection<AuthorizationInfo> Credentials { get; private set; }
        public AuthorizationViewModel(AuthorizationManager credentialsStorage)
        {
            _authorization = credentialsStorage;
            Credentials = new BindableCollection<AuthorizationInfo>();
            
        }

        public bool IsSet { get; private set; }

        public AuthorizationInfo CurrentItem
        {
            get { return _currentItem; }
            set
            {
                NotifyOfPropertyChange(nameof(CurrentItem));
            }
        }

        public string SubmitMessage
        {
            get { return _submitMessage; }
            set
            {
                _submitMessage = value;
                NotifyOfPropertyChange(nameof(SubmitMessage));
            }
        }

        public string CancelMessage
        {
            get { return _cancelMessage; }
            set
            {
                _cancelMessage = value;
                NotifyOfPropertyChange(nameof(CancelMessage));
            }
        }

        protected override void OnInitialize()
        {
            DisplayName = "Authorization";
            Credentials.AddRange(_authorization.GetAllCredential());
        }

        

        public void Add()
        {
            SubmitMessage = SAVESUBMIT;
            CancelMessage = CANCEL;
            var settingParent = (SettingShellViewModel) Parent;
            settingParent.State = SettingState.Modify;
        }

        public void Cancel()
        {
            
        }
    }
}
