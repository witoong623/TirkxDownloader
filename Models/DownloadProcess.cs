using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using TirkxDownloader.Framework;

namespace TirkxDownloader.Models
{
    public class DownloadProcess
    {
        private bool IsFileCreated;
        private HttpWebResponse WebResponse;
        private Stream InStream;
        private DownloadInfo CurrentFile;
        private CounterWarpper Counter;
        private CancellationToken ct;
        private long length;
        private IEventAggregator EventAggregate;

        public async Task StartProgress(DownloadInfo downloadInf, CounterWarpper counter, IEventAggregator eventAggregate, CancellationToken ct)
        {
            Counter = counter;
            CurrentFile = downloadInf;
            EventAggregate = eventAggregate;
            this.ct = ct;

            CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Preparing;
            Counter.Increase();
            EventAggregate.PublishOnUIThread("CanDownload");
            EventAggregate.PublishOnUIThread("CanStop");

            try
            {
                await GetFileInfo();
                await CreateFile();
                await Download();
            }
            catch (OperationCanceledException)
            {
                DeleteLocalFile();
            }
            catch
            {
                DeleteLocalFile();
            }
            finally
            {
                InStream.Close();
                Counter.Decrease();
            }
        }

        private async Task GetFileInfo()
        {
            try
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(CurrentFile.DownloadLink);
                FillCredential(request);

                WebResponse = await request.GetResponseAsync(ct);
                CurrentFile.DownloadDetail.FileSize = WebResponse.ContentLength;
                length = WebResponse.ContentLength;
                InStream = WebResponse.GetResponseStream();
            }
            catch (OperationCanceledException)
            {
                CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Stop;
                CurrentFile.DownloadDetail.ErrorMessage = "Download was canceled";

                throw;
            }
            catch (WebException ex)
            {
                CurrentFile.DownloadDetail.ErrorMessage = ex.Message;
                CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Error;

                throw;
            }
        }

        private void FillCredential(HttpWebRequest request)
        {
            request.UseDefaultCredentials = true;
        }

        private async Task CreateFile()
        {
            try
            {
                await Task.Run(() =>
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

                    }
                    catch (Exception ex)
                    {
                        CurrentFile.DownloadDetail.ErrorMessage = ex.Message;
                        CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Error;
                        DeleteLocalFile();
                    }
                }, ct);
            }
            catch (TaskCanceledException)
            {
                CurrentFile.DownloadDetail.ErrorMessage = "Download was canceled";
                CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Stop;

                throw;
            }
            catch (AggregateException ex)
            {
                CurrentFile.DownloadDetail.ErrorMessage = ex.InnerException.Message;
                CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Error;

                throw;
            }
        }

        private void DeleteLocalFile()
        {
            if (IsFileCreated)
            {
                File.Delete(CurrentFile.FullName);
            }
        }

        private async Task Download()
        {
            try
            {
                using (FileStream stream = new FileStream(CurrentFile.FullName, FileMode.Open, FileAccess.Write, FileShare.None, 65536))
                {
                    int maxReadSize = 102400;
                    int readByte = 0;
                    long downloadedSize = 0;
                    int roundCount = 0;
                    int byteCalRound = 0;
                    byte[] buffer = new byte[maxReadSize];
                    CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Downloading;
                    DateTime lastUpdate = DateTime.Now;

                    do
                    {
                        readByte = await InStream.ReadAsync(buffer, 0, maxReadSize, ct);
                        byteCalRound += readByte;
                        downloadedSize += readByte;
                        roundCount++;

                        if (roundCount == 5)
                        {
                            var now = DateTime.Now;
                            var interval = (now - lastUpdate).TotalSeconds;
                            var speed = (int)Math.Floor(byteCalRound / interval);
                            lastUpdate = now;
                            CurrentFile.DownloadDetail.RecievedSize = downloadedSize;
                            CurrentFile.DownloadDetail.Throughput = speed;
                            CurrentFile.DownloadDetail.PercentProgress = downloadedSize;

                            byteCalRound = 0;
                            roundCount = 0;
                        }

                        await stream.WriteAsync(buffer, 0, readByte);
                    } while (readByte != 0);
                }
                
                // Download completed
                CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Complete;
                EventAggregate.PublishOnUIThread("CanDownload");
                EventAggregate.PublishOnUIThread("CanStop");
            }
            catch (OperationCanceledException)
            {
                CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Stop;

                throw;
            }
            catch (Exception ex)
            {
                CurrentFile.DownloadDetail.ErrorMessage = ex.Message;
                CurrentFile.DownloadDetail.LoadingStatus = DownloadStatus.Error;

                throw;
            }
        }
    }
}
