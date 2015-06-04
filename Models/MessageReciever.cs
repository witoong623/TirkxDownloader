using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        private readonly IEventAggregator EventAggregator;
        private readonly IWindowManager WindowManager;
        private Thread BackgroundThread;

        public MessageReciever(IWindowManager windowManager, IEventAggregator eventAggregator)
        {
            WindowManager = windowManager;
            EventAggregator = eventAggregator;
            BackgroundThread = new Thread(StartReciever);
        }

        private void StartReciever()
        {
            while (true)
            {
                var fileInfo = GetFileInfo();
                Execute.OnUIThread(() => WindowManager.ShowDialog(
                    new NewDownloadViewModel(WindowManager, EventAggregator, fileInfo)));
            }
        }

        private TirkxFileInfo GetFileInfo()
        {
            using (var stdin = Console.OpenStandardInput())
            {
                int msgLength = 0;

                while (true)
                {
                    byte[] sizeBuffer = new byte[4];
                    stdin.Read(sizeBuffer, 0, 4);
                    msgLength = BitConverter.ToInt32(sizeBuffer, 0);

                    if (msgLength > 0)
                    {
                        break;
                    }
                }

                byte[] msgBuffer = new byte[msgLength];
                stdin.Read(msgBuffer, 0, msgLength);
                var jsonText = Encoding.UTF8.GetString(msgBuffer);

                return JsonConvert.DeserializeObject<TirkxFileInfo>(jsonText);
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
