using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using TirkxDownloader.Models;
using TirkxDownloader.Framework;

namespace TirkxDownloader.ViewModels
{
    public class AuthorizationViewModel : Screen, ISetting
    {
        private AuthorizationStore _authorization;
        public BindableCollection<AuthorizationInfo> Credentials { get; private set; }
        public AuthorizationViewModel(AuthorizationStore credentialsStorage)
        {
            DisplayName = "Authorization";
            _authorization = credentialsStorage;
            Credentials = new BindableCollection<AuthorizationInfo>();
            
        }

        protected override void OnInitialize()
        {
            Credentials.AddRange(_authorization.GetAllCredential());
            Credentials.Add(new AuthorizationInfo { TargetName = "Tirkx" });
            Credentials.Add(new AuthorizationInfo { TargetName = "Tirkx" });
            Credentials.Add(new AuthorizationInfo { TargetName = "Tirkx" });
            Credentials.Add(new AuthorizationInfo { TargetName = "Tirkx" });
            Credentials.Add(new AuthorizationInfo { TargetName = "Tirkx" });
            Credentials.Add(new AuthorizationInfo { TargetName = "Tirkx" });
        }

        public bool IsSet { get; private set; }

        public void Add()
        {

        }
    }
}
