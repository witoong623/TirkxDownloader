using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Caliburn.Micro;
using Newtonsoft.Json;
using TirkxDownloader.ViewModels;
using TirkxDownloader.Framework;

namespace TirkxDownloader.Models
{
    public class MessageReciever
    {
        private readonly DownloadEngine Engine;
        private readonly IEventAggregator EventAggregator;
        private readonly IWindowManager WindowManager;
        private Thread BackgroundThread;

        public MessageReciever(IWindowManager windowManager, IEventAggregator eventAggregator, DownloadEngine engine)
        {
            WindowManager = windowManager;
            EventAggregator = eventAggregator;
            Engine = engine;
            BackgroundThread = new Thread(StartReciever);
        }

        private void StartReciever()
        {
            while (true)
            {
                var fileInfo = GetFileInfo();
                Execute.OnUIThread(() => WindowManager.ShowDialog(
                    new NewDownloadViewModel(WindowManager, EventAggregator, fileInfo, Engine)));
            }
        }

        private TirkxFileInfo GetFileInfo()
        {
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add("http://localhost:6230/");
                listener.Start();
                var requestContext = listener.GetContext();
                var streamReader = new StreamReader(requestContext.Request.InputStream, requestContext.Request.ContentEncoding);
                string jsonString = streamReader.ReadToEnd();

                return JsonConvert.DeserializeObject<TirkxFileInfo>(jsonString);
            }
        }

        public void Start()
        {
            BackgroundThread.Start();
        }

        public void Stop()
        {
            BackgroundThread.Abort();
        }
    }
}
