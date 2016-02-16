using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using TirkxDownloader.ViewModels;
using TirkxDownloader.Services;
using TirkxDownloader.Framework;
using TirkxDownloader.Framework.Interface;
using TirkxDownloader.ViewModels.Settings;

namespace TirkxDownloader
{
    class AppBootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            _container = new SimpleContainer();
            // Framework
            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();

            // Screen
            _container.Singleton<ShellViewModel>();
            _container.Singleton<IContentList, QueueViewModel>();
            //_container.Singleton<IContentList, DownloadedViewModel>();

            // Setting screen
            _container.PerRequest<SettingShellViewModel>(nameof(SettingShellViewModel));
            _container.PerRequest<ISetting, DownloadingSettingViewModel>();
            _container.PerRequest<ISetting, AuthorizationViewModel>();

            // Serveice
            _container.Singleton<IMessageReciever<HttpDownloadLink>, MessageReciever>();
            _container.Singleton<IDownloader, Downloader>();
            _container.Singleton<AuthorizationManager>();
            _container.Singleton<GoogleFileHosting>();
            _container.Singleton<FileHostingUtil>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
