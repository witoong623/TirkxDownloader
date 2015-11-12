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
        private DownloadInfo _currentFile;
        private CounterWarpper _counter;
        private CancellationToken _ct;
        private DetailProvider _detailProvider;
        private IEventAggregator _eventAggregate;

        public async Task StartDownloadProcess(long maximumBytesPerSecond, DownloadInfo downloadInf, CounterWarpper counter, IEventAggregator eventAggregate, 
            CancellationToken ct, DetailProvider detailProvider)
        {
            _maximumBytesPerSecond = maximumBytesPerSecond;
            _counter = counter;
            _currentFile = downloadInf;
            _eventAggregate = eventAggregate;
            _ct = ct;
            _detailProvider = detailProvider;

            _currentFile.Status = DownloadStatus.Preparing;
            _counter.Increase();
            _eventAggregate.PublishOnUIThread("CanDownload");
            _eventAggregate.PublishOnUIThread("CanStop");

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

            _inStream.Close();
            _counter.Decrease();
        }

        private async Task GetFileInfo()
        {
            try
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(_currentFile.DownloadLink);
                request.Timeout = 300000;
                await FillCredential(request);

                var webResponse = await request.GetResponseAsync(_ct);
                _fileSize = webResponse.ContentLength;
                _inStream = new ThrottledStream(webResponse.GetResponseStream(), _maximumBytesPerSecond);
                _currentFile.InStream = _inStream;
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

        private async Task FillCredential(HttpWebRequest request)
        {
            await _detailProvider.FillCredential(request);
        }

        private async Task CreateFile()
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
                    int UpdateRound = 4;
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

                        if (roundCount == UpdateRound)
                        {
                            var now = DateTime.Now;
                            double interval = (now - lastUpdate).TotalSeconds;
                            int speed = (int)Math.Floor(byteCalRound / interval);
                            lastUpdate = now;
                            var etaTimespan = TimeSpan.FromSeconds((_fileSize - downloadedSize) / speed);
                            Duration eta = Duration.FromSeconds((long)etaTimespan.TotalSeconds);

                            _currentFile.Speed = speed;
                            _currentFile.ETA = eta;
                            _currentFile.RecievedSize = downloadedSize;
                            _currentFile.PercentProgress = downloadedSize;

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
                    _currentFile.RecievedSize = downloadedSize;
                    _currentFile.PercentProgress = downloadedSize;
                }
                
                // Download completed
                _currentFile.Status = DownloadStatus.Complete;
                _currentFile.CompleteOn = DateTime.Now;
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
