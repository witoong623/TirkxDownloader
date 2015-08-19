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
        private bool isFileCreated;
        private HttpWebResponse webResponse;
        private Stream inStream;
        private DownloadInfo currentFile;
        private CounterWarpper counter;
        private CancellationToken ct;
        private long length;
        private IEventAggregator eventAggregate;

        public async Task StartProgress(DownloadInfo downloadInf, CounterWarpper counter, IEventAggregator eventAggregate, CancellationToken ct)
        {
            this.counter = counter;
            currentFile = downloadInf;
            this.eventAggregate = eventAggregate;
            this.ct = ct;

            currentFile.DownloadDetail.LoadingStatus = DownloadStatus.Preparing;
            this.counter.Increase();
            this.eventAggregate.PublishOnUIThread("CanDownload");
            this.eventAggregate.PublishOnUIThread("CanStop");

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
                inStream.Close();
                this.counter.Decrease();
            }

            return;
        }

        private async Task GetFileInfo()
        {
            try
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(currentFile.DownloadLink);
                FillCredential(request);

                webResponse = await request.GetResponseAsync(ct);
                currentFile.DownloadDetail.FileSize = webResponse.ContentLength;
                length = webResponse.ContentLength;
                inStream = webResponse.GetResponseStream();
            }
            catch (OperationCanceledException)
            {
                currentFile.DownloadDetail.LoadingStatus = DownloadStatus.Stop;
                currentFile.DownloadDetail.ErrorMessage = "Download was canceled";

                throw;
            }
            catch (WebException ex)
            {
                currentFile.DownloadDetail.ErrorMessage = ex.Message;
                currentFile.DownloadDetail.LoadingStatus = DownloadStatus.Error;

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
                        if (File.Exists(currentFile.FullName))
                        {
                            var localFile = new FileInfo(currentFile.FullName);
                            var fileName = Path.GetFileNameWithoutExtension(currentFile.FileName);
                            var ext = localFile.Extension;
                            int count = 1;
                            var newFileName = "";

                            do
                            {
                                newFileName = fileName + "(" + count + ")";
                                count++;
                            } while (File.Exists(Path.Combine(currentFile.SaveLocation, newFileName + ext)));

                            currentFile.FileName = newFileName + ext;
                        }

                        using (var file = File.Create(currentFile.FullName)) { }
                        isFileCreated = true;

                    }
                    catch (Exception ex)
                    {
                        currentFile.DownloadDetail.ErrorMessage = ex.Message;
                        currentFile.DownloadDetail.LoadingStatus = DownloadStatus.Error;
                        DeleteLocalFile();
                    }
                }, ct);
            }
            catch (TaskCanceledException)
            {
                currentFile.DownloadDetail.ErrorMessage = "Download was canceled";
                currentFile.DownloadDetail.LoadingStatus = DownloadStatus.Stop;

                throw;
            }
            catch (AggregateException ex)
            {
                currentFile.DownloadDetail.ErrorMessage = ex.InnerException.Message;
                currentFile.DownloadDetail.LoadingStatus = DownloadStatus.Error;

                throw;
            }
        }

        private void DeleteLocalFile()
        {
            if (isFileCreated)
            {
                File.Delete(currentFile.FullName);
            }
        }

        private async Task Download()
        {
            try
            {
                using (FileStream stream = new FileStream(currentFile.FullName, FileMode.Open, FileAccess.Write, FileShare.None, 65536))
                {
                    int maxReadSize = 102400;
                    int readByte = 0;
                    long downloadedSize = 0;
                    int roundCount = 0;
                    int byteCalRound = 0;
                    byte[] buffer = new byte[maxReadSize];
                    currentFile.DownloadDetail.LoadingStatus = DownloadStatus.Downloading;
                    DateTime lastUpdate = DateTime.Now;

                    do
                    {
                        readByte = await inStream.ReadAsync(buffer, 0, maxReadSize, ct);
                        byteCalRound += readByte;
                        downloadedSize += readByte;
                        roundCount++;

                        if (roundCount == 5)
                        {
                            var now = DateTime.Now;
                            var interval = (now - lastUpdate).TotalSeconds;
                            var speed = (int)Math.Floor(byteCalRound / interval);
                            lastUpdate = now;
                            currentFile.DownloadDetail.RecievedSize = downloadedSize;
                            currentFile.DownloadDetail.Throughput = speed;
                            currentFile.DownloadDetail.PercentProgress = downloadedSize;

                            byteCalRound = 0;
                            roundCount = 0;
                        }

                        await stream.WriteAsync(buffer, 0, readByte);
                    } while (readByte != 0);
                }
                
                // Download completed
                currentFile.DownloadDetail.LoadingStatus = DownloadStatus.Complete;
                eventAggregate.PublishOnUIThread("CanDownload");
                eventAggregate.PublishOnUIThread("CanStop");
            }
            catch (OperationCanceledException)
            {
                currentFile.DownloadDetail.LoadingStatus = DownloadStatus.Stop;

                throw;
            }
            catch (Exception ex)
            {
                currentFile.DownloadDetail.ErrorMessage = ex.Message;
                currentFile.DownloadDetail.LoadingStatus = DownloadStatus.Error;

                throw;
            }
        }
    }
}
