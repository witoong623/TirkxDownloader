using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Caliburn.Micro;
using TirkxDownloader.Framework;

namespace TirkxDownloader.Models
{
    public class DownloadProcess
    {
        private bool IsFileCreated;
        private Stream InStream;
        private DownloadInfo CurrentFile;
        private CounterWarpper Counter;
        private IEventAggregator EventAggregate;

        public void StartProgress(object param)
        {
            var parameter = (ThreadParameter)param;
            Counter = parameter.Counter;
            CurrentFile = parameter.DownloadInformation;
            EventAggregate = parameter.EventAggregate;

            CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Preparing;
            Counter.Increase();
            EventAggregate.PublishOnUIThread("CanDownload");
            EventAggregate.PublishOnUIThread("CanStop");

            if (!GetFileInfo())
            {
                return;
            }

            if (!CreateLocalFile())
            {
                return;
            }

            if (!Download())
            {
                return;
            }
        }

        private bool GetFileInfo()
        {
            try
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(CurrentFile.DownloadLink);
                FillCredential(request);

                var respone = (HttpWebResponse)request.GetResponse();
                CurrentFile.DownloadDetail.FileSize = respone.ContentLength;
                InStream = respone.GetResponseStream();

                return true;
            }
            catch (ThreadAbortException)
            {
                CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Stop;
                Counter.Decrease();

                return false;
            }
            catch (Exception ex)
            {
                CurrentFile.DownloadDetail.ErrorMessage = ex.Message;
                CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Error;
                Counter.Decrease();

                return false;
            }
        }

        private void FillCredential(HttpWebRequest request)
        {
            request.UseDefaultCredentials = true;
        }

        private bool CreateLocalFile()
        {
            try
            {
                if (File.Exists(CurrentFile.FullName))
                {
                    var localFile = new FileInfo(CurrentFile.FullName);
                    var fileName = Path.GetFileNameWithoutExtension(CurrentFile.FileName);
                    var ext = localFile.Extension;
                    int count = 1;
                    var newFileName = "";

                    do
                    {
                        newFileName = fileName + "(" + count + ")";
                        count++;
                    } while (File.Exists(Path.Combine(CurrentFile.SaveLocation, newFileName + ext)));

                    CurrentFile.FileName = newFileName + ext;
                }

                using (var file = File.Create(CurrentFile.FullName)) { }
                IsFileCreated = true;

                return true;
            }
            catch (ThreadAbortException)
            {
                CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Stop;
                DeleteLocalFile();
                Counter.Decrease();

                return false;
            }
            catch (Exception ex)
            {
                CurrentFile.DownloadDetail.ErrorMessage = ex.Message;
                CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Error;
                DeleteLocalFile();
                Counter.Decrease();

                return false;
            }
        }

        private void DeleteLocalFile()
        {
            if (IsFileCreated)
            {
                File.Delete(CurrentFile.FullName);
            }
        }

        private bool Download()
        {
            try
            {
                using (FileStream stream = new FileStream(CurrentFile.FullName, FileMode.Open, FileAccess.Write, FileShare.None, 65536))
                {
                    int maxReadSize = 102400;
                    int readByte = 0;
                    long downloadedSize = 0;
                    byte[] buffer = new byte[maxReadSize];
                    CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Downloading;
                    DateTime lastUpdate = DateTime.Now;

                    do
                    {
                        readByte = InStream.Read(buffer, 0, maxReadSize);
                        var now = DateTime.Now;
                        var interval = (now - lastUpdate).TotalSeconds;
                        var speed = (int)Math.Floor(readByte / interval);
                        lastUpdate = now;
                        downloadedSize += readByte;
                        CurrentFile.DownloadDetail.RecievedSize = downloadedSize;
                        CurrentFile.DownloadDetail.Throughput = speed;
                        CurrentFile.DownloadDetail.PercentProgress = downloadedSize;
                        stream.Write(buffer, 0, readByte);
                    } while (readByte != 0);
                }
                
                CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Complete;
                EventAggregate.PublishOnUIThread("CanDownload");
                EventAggregate.PublishOnUIThread("CanStop");
                Counter.Decrease();

                return true;
            }
            catch (ThreadAbortException)
            {
                CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Stop;
                DeleteLocalFile();
                Counter.Decrease();

                return false;
            }
            catch (Exception ex)
            {
                CurrentFile.DownloadDetail.ErrorMessage = ex.Message;
                CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Error;
                DeleteLocalFile();
                Counter.Decrease();

                return false;
            }
        }
    }
}
