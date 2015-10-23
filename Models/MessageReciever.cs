using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Newtonsoft.Json;
using TirkxDownloader.ViewModels;
using TirkxDownloader.Framework;
using TirkxDownloader.Framework.Interface;
using System.Collections.Generic;

namespace TirkxDownloader.Models
{
    public class MessageReciever<T> : IMessageReciever<T>
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly IDownloader _downloader;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowManager _windowManager;
        private readonly CancellationTokenSource _cts;
        private readonly DetailProvider _detailProvider;

        public ICollection<string> Prefixes { get { return _listener.Prefixes; } }

        public bool IsRecieving { get { return _listener.IsListening; } }

        public MessageReciever(IEventAggregator eventAggregator, IDownloader downloader,
            DetailProvider detailProvide)
        {
            _eventAggregator = eventAggregator;
            _downloader = downloader;
            _detailProvider = detailProvide;
        }

        /// <summary>
        /// Let Reciever send messages as event
        /// </summary>
        public async void StartSendEvent(CancellationToken ct)
        {
            try
            {
                while (ct.IsCancellationRequested)
                {
                    await _eventAggregator.PublishOnUIThreadAsync(await GetMessageAsync());
                }
            }
            catch (OperationCanceledException) { }
            catch { }
        }

        public async Task<T> GetMessageAsync()
        {
            _listener.Start();
            var requestContext = await _listener.GetContextAsync();
            var streamReader = new StreamReader(requestContext.Request.InputStream, requestContext.Request.ContentEncoding);
            string jsonString = streamReader.ReadToEnd();
            _listener.Stop();

            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary>
        /// Stop reciever from listen to message
        /// </summary>
        public void Close()
        {
            _listener.Close();
        }

        /// <summary>
        /// Close Listener
        /// </summary>
        public void StopReciever()
        {
            _listener.Stop();
        }
    }
}
