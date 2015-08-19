﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using TirkxDownloader.ViewModels;
using TirkxDownloader.Models;
using TirkxDownloader.Framework;

namespace TirkxDownloader
{
    class AppBootstrapper : BootstrapperBase
    {
        private SimpleContainer container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            container = new SimpleContainer();
            // Framework
            container.Singleton<IWindowManager, WindowManager>();
            container.Singleton<IEventAggregator, EventAggregator>();

            // Screen
            container.Singleton<ShellViewModel>();
            container.Singleton<SettingShellViewModel>();
            container.Singleton<IContentList, QueueViewModel>();
            container.Singleton<IContentList, DownloadedViewModel>();
            container.Singleton<ISetting, AuthorizationViewModel>();

            // Serveice
            container.Singleton<MessageReciever>();
            container.Singleton<DownloadEngine>();
            container.Singleton<AuthorizationStore>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
