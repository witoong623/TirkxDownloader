using System;
using System.Collections.Generic;
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
        private bool _isFileCreated;
        private HttpWebResponse _webResponse;
        private Stream _inStream;
        private DownloadInfo _currentFile;
        private CounterWarpper _counter;
        private CancellationToken _ct;
        private long _length;
        private IEventAggregator _eventAggregate;

        public async Task StartProgress(DownloadInfo downloadInf, CounterWarpper counter, IEventAggregator eventAggregate, CancellationToken ct)
        {
            this._counter = counter;
            _currentFile = downloadInf;
            this._eventAggregate = eventAggregate;
            this._ct = ct;

            _currentFile.Status = DownloadStatus.Preparing;
            this._counter.Increase();
            this._eventAggregate.PublishOnUIThread("CanDownload");
            this._eventAggregate.PublishOnUIThread("CanStop");

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
                _inStream.Close();
                this._counter.Decrease();
            }

            return;
        }

        private async Task GetFileInfo()
        {
            try
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(_currentFile.DownloadLink);
                FillCredential(request);

                _webResponse = await request.GetResponseAsync(_ct);
                _currentFile.FileSize = _webResponse.ContentLength;
                _length = _webResponse.ContentLength;
                _inStream = _webResponse.GetResponseStream();
            }
            catch (OperationCanceledException)
            {
                _currentFile.Status = DownloadStatus.Stop;
                _currentFile.ErrorMessage = "Download was canceled";

                throw;
            }
            catch (WebException ex)
            {
                _currentFile.ErrorMessage = ex.Message;
                _currentFile.Status = DownloadStatus.Error;

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
                        if (File.Exists(_currentFile.FullName))
                        {
                            var localFile = new FileInfo(_currentFile.FullName);
                            var fileName = Path.GetFileNameWithoutExtension(_currentFile.FileName);
                            var ext = localFile.Extension;
                            int count = 1;
                            var newFileName = "";

                            do
                            {
                                newFileName = fileName + "(" + count + ")";
                                count++;
                            } while (File.Exists(Path.Combine(_currentFile.SaveLocation, newFileName + ext)));

                            _currentFile.FileName = newFileName + ext;
                        }

                        using (var file = File.Create(_currentFile.FullName)) { }
                        _isFileCreated = true;

                    }
                    catch (Exception ex)
                    {
                        _currentFile.ErrorMessage = ex.Message;
                        _currentFile.Status = DownloadStatus.Error;
                        DeleteLocalFile();
                    }
                }, _ct);
            }
            catch (TaskCanceledException)
            {
                _currentFile.ErrorMessage = "Download was canceled";
                _currentFile.Status = DownloadStatus.Stop;

                throw;
            }
            catch (AggregateException ex)
            {
                _currentFile.ErrorMessage = ex.InnerException.Message;
                _currentFile.Status = DownloadStatus.Error;

                throw;
            }
        }

        private void DeleteLocalFile()
        {
            if (_isFileCreated)
            {
                File.Delete(_currentFile.FullName);
            }
        }

        private async Task Download()
        {
            try
            {
                using (FileStream stream = new FileStream(_currentFile.FullName, FileMode.Open, FileAccess.Write, FileShare.None, 65536))
                {
                    int maxReadSize = 102400;
                    int readByte = 0;
                    long downloadedSize = 0;
                    int roundCount = 0;
                    int byteCalRound = 0;
                    byte[] buffer = new byte[maxReadSize];
                    _currentFile.Status = DownloadStatus.Downloading;
                    DateTime lastUpdate = DateTime.Now;

                    do
                    {
                        readByte = await _inStream.ReadAsync(buffer, 0, maxReadSize, _ct);
                        byteCalRound += readByte;
                        downloadedSize += readByte;
                        roundCount++;

                        if (roundCount == 5)
                        {
                            var now = DateTime.Now;
                            var interval = (now - lastUpdate).TotalSeconds;
                            var speed = (int)Math.Floor(byteCalRound / interval);
                            lastUpdate = now;
                            _currentFile.RecievedSize = downloadedSize;
                            _currentFile.Throughput = speed;
                            _currentFile.PercentProgress = downloadedSize;

                            byteCalRound = 0;
                            roundCount = 0;
                        }

                        await stream.WriteAsync(buffer, 0, readByte);
                    } while (readByte != 0);
                }
                
                // Download completed
                _currentFile.Status = DownloadStatus.Complete;
                _eventAggregate.PublishOnUIThread("CanDownload");
                _eventAggregate.PublishOnUIThread("CanStop");
            }
            catch (OperationCanceledException)
            {
                _currentFile.Status = DownloadStatus.Stop;

                throw;
            }
            catch (Exception ex)
            {
                _currentFile.ErrorMessage = ex.Message;
                _currentFile.Status = DownloadStatus.Error;

                throw;
            }
        }
    }
}
