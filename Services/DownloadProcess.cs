using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using NodaTime;
using TirkxDownloader.Framework;

namespace TirkxDownloader.Models
{
    public class DownloadProcess
    {
        private bool _isFileCreated;
        private long _fileSize;
        private long _maximumBytesPerSecond;
        private ThrottledStream _inStream;
        private DownloadInfo _currentItem;
        private CancellationToken _ct;
        private DetailProvider _detailProvider;
        private IEventAggregator _eventAggregate;

        public async Task StartDownloadProcess(long maximumBytesPerSecond, DownloadInfo downloadInf, IEventAggregator eventAggregate, 
            CancellationToken ct, DetailProvider detailProvider)
        {
            _maximumBytesPerSecond = maximumBytesPerSecond;
            _currentItem = downloadInf;
            _eventAggregate = eventAggregate;
            _ct = ct;
            _detailProvider = detailProvider;

            _currentItem.Status = DownloadStatus.Preparing;
            _eventAggregate.PublishOnUIThread("CanDownload");
            _eventAggregate.PublishOnUIThread("CanStop");

            try
            {
                await GetFileInfo();
                await CreateFile();
                await Download();
            }
            catch
            {
                DeleteLocalFile();
            }
            finally
            {
                _inStream.Close();
                _currentItem.OnDownloadComplete();
            }
        }

        private async Task GetFileInfo()
        {
            try
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(_currentItem.DownloadLink);
                request.Timeout = 300000;
                await FillCredential(request);

                var webResponse = await request.GetResponseAsync(_ct);
                _fileSize = webResponse.ContentLength;
                _inStream = new ThrottledStream(webResponse.GetResponseStream(), _maximumBytesPerSecond);
                _currentItem.InStream = _inStream;
            }
            catch (OperationCanceledException)
            {
                _currentItem.Status = DownloadStatus.Stop;
                _currentItem.ErrorMessage = "Download was canceled";

                throw;
            }
            catch (WebException ex)
            {
                _currentItem.ErrorMessage = ex.Message;
                _currentItem.Status = DownloadStatus.Error;

                throw;
            }
        }

        private async Task FillCredential(HttpWebRequest request)
        {
            await _detailProvider.FillCredential(request);
        }

        private async Task CreateFile()
        {
            try
            {
                if (File.Exists(_currentItem.FullName))
                {
                    var localFile = new FileInfo(_currentItem.FullName);
                    var fileName = Path.GetFileNameWithoutExtension(_currentItem.FileName);
                    var ext = localFile.Extension;
                    int count = 1;
                    var newFileName = "";

                    do
                    {
                        newFileName = fileName + "(" + count + ")";
                        count++;
                    } while (File.Exists(Path.Combine(_currentItem.SaveLocation, newFileName + ext)));

                    _currentItem.FileName = newFileName + ext;
                }

                using (var file = File.Create(_currentItem.FullName)) { }
                _isFileCreated = true;

            }
            catch (Exception ex)
            {
                _currentItem.ErrorMessage = ex.Message;
                _currentItem.Status = DownloadStatus.Error;

                throw;
            }
        }

        private void DeleteLocalFile()
        {
            if (_isFileCreated)
            {
                File.Delete(_currentItem.FullName);
            }
        }

        private async Task Download()
        {
            try
            {
                using (FileStream stream = new FileStream(_currentItem.FullName, FileMode.Open, FileAccess.Write, FileShare.None, 65536))
                {
                    int UpdateRound = 4;
                    int maxReadSize = 102400;
                    int readByte = 0;
                    long downloadedSize = 0;
                    int roundCount = 0;
                    int byteCalRound = 0;
                    byte[] buffer = new byte[maxReadSize];
                    _currentItem.Status = DownloadStatus.Downloading;
                    DateTime lastUpdate = DateTime.Now;

                    do
                    {
                        readByte = await _inStream.ReadAsync(buffer, 0, maxReadSize, _ct);
                        byteCalRound += readByte;
                        downloadedSize += readByte;
                        roundCount++;

                        if (roundCount == UpdateRound)
                        {
                            var now = DateTime.Now;
                            double interval = (now - lastUpdate).TotalSeconds;
                            int speed = (int)Math.Floor(byteCalRound / interval);
                            lastUpdate = now;
                            var etaTimespan = TimeSpan.FromSeconds((_fileSize - downloadedSize) / speed);
                            Duration eta = Duration.FromSeconds((long)etaTimespan.TotalSeconds);

                            _currentItem.Speed = speed;
                            _currentItem.ETA = eta;
                            _currentItem.RecievedSize = downloadedSize;
                            _currentItem.PercentProgress = downloadedSize;

                            Debug.WriteLineIf(speed < 0, string.Format(
                                "speed = {0}, interval = {1}, byteCalround = {2}, readByte = {3}", speed, interval, byteCalRound, readByte));
                            byteCalRound = 0;
                            roundCount = 0;
                            // Calculate update round from kilobyte per secound
                            UpdateRound = speed / 10240 + 1;
                        }

                        await stream.WriteAsync(buffer, 0, readByte);
                    } while (readByte != 0 && !_ct.IsCancellationRequested);

                    // update last value of downloadSize to ensure that file completed at 100%
                    _currentItem.RecievedSize = downloadedSize;
                    _currentItem.PercentProgress = downloadedSize;

                    if (_ct.IsCancellationRequested)
                    {
                        throw new OperationCanceledException();
                    }
                }
                
                // Download completed
                _currentItem.Status = DownloadStatus.Complete;
                _currentItem.CompleteOn = DateTime.Now;
                _eventAggregate.PublishOnUIThread("CanDownload");
                _eventAggregate.PublishOnUIThread("CanStop");
            }
            catch (OperationCanceledException)
            {
                _currentItem.Status = DownloadStatus.Stop;

                throw;
            }
            catch (Exception ex)
            {
                _currentItem.ErrorMessage = ex.Message;
                _currentItem.Status = DownloadStatus.Error;

                throw;
            }
        }
    }
}
