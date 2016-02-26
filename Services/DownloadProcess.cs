using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using NodaTime;
using TirkxDownloader.Framework;
using TirkxDownloader.Framework.Interface;
using TirkxDownloader.Models;
using TirkxDownloader.Framework.Message;

namespace TirkxDownloader.Services
{
    public class DownloadProcess : IHandle<MaxBpsUpdate>
    {
        private bool _isThrottle;
        private bool _isFileCreated;
        private long _fileSize;
        private long _bytesCalBps;
        private long _maximumBytesPerSecond;
        private ThrottledStream _inStream;
        private IDownloadItem _currentItem;
        private CancellationToken _ct;
        private FileHostingUtil _hostUtil;
        private IEventAggregator _eventAggregate;
        private Stopwatch _stopWatch;

        public async Task StartDownloadProcess(long maximumBytesPerSecond, IDownloadItem downloadInf, IEventAggregator eventAggregate, 
            CancellationToken ct, FileHostingUtil detailProvider)
        {
            _maximumBytesPerSecond = maximumBytesPerSecond;
            _currentItem = downloadInf;
            _eventAggregate = eventAggregate;
            _ct = ct;
            _hostUtil = detailProvider;

            _currentItem.Status = DownloadStatus.Preparing;
            _eventAggregate.PublishOnUIThread("CanDownload");
            _eventAggregate.PublishOnUIThread("CanStop");

            _isThrottle = _maximumBytesPerSecond > 0 ? true : false;

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
                var stream = await _hostUtil.GetStreamAsync(_currentItem, _ct);
                _fileSize = _currentItem.FileSizeInBytes;
                _inStream = new ThrottledStream(stream, _maximumBytesPerSecond);
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
                    byte[] buffer = new byte[maxReadSize];
                    _currentItem.Status = DownloadStatus.Downloading;

                    if (_isThrottle)
                    {
                        _stopWatch = new Stopwatch();
                    }
                    else
                    {
                        _stopWatch = Stopwatch.StartNew();
                    }

                    do
                    {
                        readByte = await _inStream.ReadAsync(buffer, 0, maxReadSize, _ct);
                        _bytesCalBps += readByte;
                        downloadedSize += readByte;
                        roundCount++;

                        if (roundCount == UpdateRound)
                        {
                            long speed;

                            if (!_isThrottle)
                            {
                                speed = _bytesCalBps * 1000L / _stopWatch.ElapsedMilliseconds;
                            }
                            else
                            {
                                speed = _inStream.bps;
                            }

                            var etaTimespan = TimeSpan.FromSeconds((_fileSize - downloadedSize) / speed);
                            Duration eta = Duration.FromSeconds((long)etaTimespan.TotalSeconds);

                            _currentItem.Speed = (int)speed;
                            _currentItem.ETA = eta;
                            _currentItem.RecievedSize = downloadedSize;
                            _currentItem.PercentProgress = downloadedSize;
                            
                            roundCount = 0;
                            // Calculate update round from kilobyte per secound
                            UpdateRound = (int)speed / 10240 + 1;
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

        private void Reset()
        {
            _bytesCalBps = 0;
            _stopWatch.Restart();
        }

        public void Handle(MaxBpsUpdate message)
        {
            _maximumBytesPerSecond = message.MaximumBytesPerSecond;
            _inStream.MaximumBytesPerSecond = message.MaximumBytesPerSecond;
            Reset();

            if (message.MaximumBytesPerSecond > 0)
            {
                _isThrottle = true;
                
                if (_stopWatch.IsRunning)
                {
                    _stopWatch.Reset();
                }
            }
            else
            {
                _isThrottle = false;
            }
        }
    }
}
