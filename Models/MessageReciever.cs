using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Newtonsoft.Json;
using TirkxDownloader.ViewModels;
using TirkxDownloader.Framework;

namespace TirkxDownloader.Models
{
    public class MessageReciever
    {
        private readonly HttpListener _listener;
        private readonly DownloadEngine _downloader;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowManager _windowManager;
        private readonly CancellationTokenSource _cts;
        private readonly DetailProvider _detailProvider;

        public MessageReciever(IWindowManager windowManager, IEventAggregator eventAggregator, DownloadEngine engine,
            DetailProvider detailProvide)
        {
            this._windowManager = windowManager;
            this._eventAggregator = eventAggregator;
            _downloader = engine;
            _detailProvider = detailProvide;
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:6230/");
        }

        private async void StartReciever()
        {
            try
            {
                while (true)
                {
                    var fileInfo = await GetFileInfo();

                    if (fileInfo != null && !_downloader.IsWorking)
                    {
                        await Execute.OnUIThreadAsync(() => _windowManager.ShowDialog(
                            new NewDownloadViewModel(_eventAggregator, fileInfo, _downloader, _detailProvider)));
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (HttpListenerException) { }
        }

        private async Task<HttpDownloadLink> GetFileInfo()
        {
            _listener.Start();
            var requestContext = await _listener.GetContextAsync();
            var streamReader = new StreamReader(requestContext.Request.InputStream, requestContext.Request.ContentEncoding);
            string jsonString = streamReader.ReadToEnd();
            _listener.Stop();

            return JsonConvert.DeserializeObject<HttpDownloadLink>(jsonString);
        }

        public void Start()
        {
            Task.Run(() => StartReciever());
        }

        public void Stop()
        {
            try
            {
                _listener.Close();
            }
            catch { }
        }
    }
}
