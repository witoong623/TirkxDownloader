using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Newtonsoft.Json;
using TirkxDownloader.Framework;
using TirkxDownloader.Framework.Interface;
using System.Collections.Generic;

namespace TirkxDownloader.Services
{
    public class MessageReciever : IMessageReciever<HttpDownloadLink>
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly IDownloader _downloader;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowManager _windowManager;
        private readonly CancellationTokenSource _cts;
        private readonly FileHostingUtil _detailProvider;

        public ICollection<string> Prefixes { get { return _listener.Prefixes; } }

        public bool IsRecieving { get { return _listener.IsListening; } }

        public MessageReciever(IEventAggregator eventAggregator, IDownloader downloader,
            FileHostingUtil detailProvide)
        {
            _eventAggregator = eventAggregator;
            _downloader = downloader;
            _detailProvider = detailProvide;
        }

        /// <summary>
        /// Start listener and let Reciever send messages as event
        /// </summary>
        public async void StartSendEvent(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    await _eventAggregator.PublishOnUIThreadAsync(await GetMessageAsync(ct));
                }
            }
            catch (OperationCanceledException) { }
            catch { }
        }

        /// <summary>
        /// Asynchronous get message <typeparamref name="T"/> from reciever
        /// </summary>
        /// <returns>Message of type <typeparamref name="T"/></returns>
        public Task<HttpDownloadLink> GetMessageAsync()
        {
            return GetMessageAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronous get message <typeparamref name="T"/> from reciever, specify <typeparamref name="CancellationToken"/>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Message of type <typeparamref name="T"/></returns>
        public async Task<HttpDownloadLink> GetMessageAsync(CancellationToken ct)
        {
            _listener.Start();
            var requestContext = await _listener.GetContextAsync(ct);
            var streamReader = new StreamReader(requestContext.Request.InputStream, requestContext.Request.ContentEncoding);
            string jsonString = streamReader.ReadToEnd();
            _listener.Stop();

            return JsonConvert.DeserializeObject<HttpDownloadLink>(jsonString);
        }

        /// <summary>
        /// Close listener
        /// </summary>
        public void Close()
        {
            _listener.Close();
        }
    }
}
