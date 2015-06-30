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
        private readonly HttpListener Listener;
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
            Listener = new HttpListener();
            Listener.Prefixes.Add("http://localhost:6230/");
        }

        private void StartReciever()
        {
            while (true)
            {
                var fileInfo = GetFileInfo();

                if (fileInfo != null)
                {
                    Execute.OnUIThread(() => WindowManager.ShowDialog(
                        new NewDownloadViewModel(WindowManager, EventAggregator, fileInfo, Engine)));
                }
            }
        }

        private TirkxFileInfo GetFileInfo()
        {
            try
            {
                Listener.Start();
                var requestContext = Listener.GetContext();
                var streamReader = new StreamReader(requestContext.Request.InputStream, requestContext.Request.ContentEncoding);
                string jsonString = streamReader.ReadToEnd();
                Listener.Stop();

                return JsonConvert.DeserializeObject<TirkxFileInfo>(jsonString);
            }
            catch (ThreadAbortException)
            {
                return null;
            }
            catch (HttpListenerException)
            {
                return null;
            }
        }

        public void Start()
        {
            BackgroundThread.Start();
        }

        public void Stop()
        {
            Listener.Close();
            BackgroundThread.Abort();
        }
    }
}
