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
                _currentItem = value;
                NotifyOfPropertyChange(nameof(CurrentItem));
                NotifyOfPropertyChange(nameof(CanDelete));
                NotifyOfPropertyChange(nameof(CanEdit));
            }
        }

        #region summary properties member
        public bool CanDelete
        {
            get { return CurrentItem != null; }
        }

        public bool CanEdit
        {
            get { return CurrentItem != null; }
        }
        #endregion

        #region modify properties member
        private string _username;
        private string _password;
        private string _targetName;

        public string TargetName
        {
            get { return _targetName; }
            set
            {
                _targetName = value;
                NotifyOfPropertyChange(nameof(TargetName));
                NotifyOfPropertyChange(nameof(CanSubmit));
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                NotifyOfPropertyChange(nameof(Username));
                NotifyOfPropertyChange(nameof(CanSubmit));
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                NotifyOfPropertyChange(nameof(Password));
                NotifyOfPropertyChange(nameof(CanSubmit));
            }
        }

        public bool CanSubmit
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) &&
                  !string.IsNullOrWhiteSpace(TargetName);
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
        #endregion

        #region override member
        protected override void OnInitialize()
        {
            DisplayName = "Authorization";
            Credentials.AddRange(_authorization.GetAllCredential());
        }
        #endregion

        #region summary view method
        public void Add()
        {
            IsSet = true;
            SubmitMessage = SAVESUBMIT;
            CancelMessage = CANCEL;
            var settingParent = (SettingShellViewModel)Parent;
            settingParent.State = SettingState.Modify;
        }

        public void Edit()
        {
            IsSet = false;
            SubmitMessage = SAVECHANGE;
            CancelMessage = UNCHANGE;
            var settingParent = (SettingShellViewModel)Parent;
            settingParent.State = SettingState.Modify;
        }

        public void Delete()
        {
            if (_authorization.DeleteCredential(CurrentItem.TargetName))
            {
                CurrentItem = null;
            }
        }

        public void SelectItem(AuthorizationInfo authorizationInfo)
        {
            CurrentItem = authorizationInfo;
        }
        #endregion

        #region Modify view method
        public void Submit()
        {
            bool flag = _authorization.SaveCredential(TargetName, Username, Password);
            var settingParent = (SettingShellViewModel)Parent;
            settingParent.State = SettingState.Summary;

            if (flag)
            {
                Credentials.Add(_authorization.GetCredential(TargetName));
            }

            CleanTextbox();
        }

        public void Cancel()
        {
            var settingParent = (SettingShellViewModel)Parent;
            settingParent.State = SettingState.Summary;
            CleanTextbox();
        }

        private void CleanTextbox()
        {
            TargetName = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
        }
        #endregion
    }
}
