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
        private readonly HttpListener listener;
        private readonly DownloadEngine engine;
        private readonly IEventAggregator eventAggregator;
        private readonly IWindowManager windowManager;
        private Thread backgroundThread;

        public MessageReciever(IWindowManager windowManager, IEventAggregator eventAggregator, DownloadEngine engine)
        {
            this.windowManager = windowManager;
            this.eventAggregator = eventAggregator;
            this.engine = engine;
            backgroundThread = new Thread(StartReciever);
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:6230/");
        }

        private void StartReciever()
        {
            while (true)
            {
                var fileInfo = GetFileInfo();

                if (fileInfo != null && !engine.IsWorking)
                {
                    Execute.OnUIThread(() => windowManager.ShowDialog(
                        new NewDownloadViewModel(windowManager, eventAggregator, fileInfo, engine)));
                }
            }
        }

        private TirkxFileInfo GetFileInfo()
        {
            try
            {
                listener.Start();
                var requestContext = listener.GetContext();
                var streamReader = new StreamReader(requestContext.Request.InputStream, requestContext.Request.ContentEncoding);
                string jsonString = streamReader.ReadToEnd();
                listener.Stop();

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
            backgroundThread.Start();
        }

        public void Stop()
        {
            listener.Close();
            backgroundThread.Abort();
        }
    }
}
