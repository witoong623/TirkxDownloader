using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TirkxDownloader.Framework;
using TirkxDownloader.Framework.Interface;
using TirkxDownloader.Models;

namespace TirkxDownloader.Services
{
    public enum HostingProvider { General, GoogleDrive }
    /// <summary>
    /// Represent helper method for downloading file
    /// </summary>
    public class FileHostingUtil
    {
        private AuthorizationManager _authenticationManager;
        private GoogleFileHosting _googleHost;

        #region constructor
        public FileHostingUtil(AuthorizationManager authenticationManager, GoogleFileHosting googleHost)
        {
            _authenticationManager = authenticationManager;
            _googleHost = googleHost;
        }
        #endregion

        public async Task<IDownloadItem> CreateDownloadFile(HttpDownloadLink httpMessage, CancellationToken ct)
        {
            IDownloadItem item;

            if (CheckFileHosting(httpMessage.DownloadLink) == HostingProvider.GoogleDrive)
            {
                item = new GoogleDriveDownloadItem()
                {
                    DownloadLink = httpMessage.DownloadLink
                };

                await GetGoogleDriveFileInformation(item, ct);
            }
            else
            {
                item = new GeneralDownloadItem()
                {
                    FileName = httpMessage.FileName,
                    DownloadLink = httpMessage.DownloadLink
                };

                await GetGeneralFileInfomation(item, ct);
            }

            return item;
        }

        public Task<Stream> GetStreamAsync(IDownloadItem item, CancellationToken ct)
        {
            var googleDriveFile = item as GoogleFileHosting.IGoogleDriveFile;

            if (googleDriveFile != null)
            {
                return _googleHost.GetHttpResponseStreamAsync(googleDriveFile, ct);
            }
            else
            {
                return GetHttpResponseStreamAsync(item, ct);
            }
        }

        public HostingProvider CheckFileHosting(string downloadUrl)
        {
            if (downloadUrl.Contains("google.com"))
            {
                return HostingProvider.GoogleDrive;
            }
            else
            {
                return HostingProvider.General;
            }
        }

        #region general hosting private method
        /// <summary>
        /// Get file information from regular file hosting
        /// </summary>
        /// <param name="item">downloadable item</param>
        /// <param name="ct">cancellation token</param>
        /// <returns>Task represent async operation</returns>
        private async Task GetGeneralFileInfomation(IDownloadItem item, CancellationToken ct)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(item.DownloadLink);
            request.Method = "HEAD";
            request.Timeout = 300000;
            await FillCredential(request);
            var response = await request.GetResponseAsync(ct);

            try
            {
                item.FileSize = response.ContentLength / 1048576;
                item.FileSizeInBytes = response.ContentLength;
            }
            catch (NotSupportedException) { }
            finally
            {
                response.Close();
            }
        }

        /// <summary>
        /// Get response stream when download file from general hosting
        /// </summary>
        /// <param name="item">downloadable item</param>
        /// <param name="ct">cancellation token</param>
        /// <returns>Task contain response Stream</returns>
        private async Task<Stream> GetHttpResponseStreamAsync(IDownloadItem item, CancellationToken ct)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(item.DownloadLink);
            request.Timeout = 300000;
            await FillCredential(request);
            var webResponse = await request.GetResponseAsync(ct);

            return webResponse.GetResponseStream();
        }

        private async Task FillCredential(HttpWebRequest request)
        {
            string targetDomain = null;
            string hostingDomain = request.Host;
            // load credential
            AuthorizationInfo authorizationInfo = _authenticationManager.GetCredential(hostingDomain);

            if (authorizationInfo == null)
            {
                try
                {
                    using (StreamReader str = new StreamReader("Target domain.dat", Encoding.UTF8))
                    {
                        string storedDomian;

                        while ((storedDomian = await str.ReadLineAsync()) != null)
                        {
                            if (hostingDomain.IsSameDomain(storedDomian))
                            {
                                targetDomain = storedDomian;
                            }
                        }
                    }

                    authorizationInfo = _authenticationManager.GetCredential(targetDomain);
                }
                catch (FileNotFoundException)
                {
                    return;
                }
            }

            if (authorizationInfo != null)
            {
                var netCredential = new NetworkCredential(authorizationInfo.Username, authorizationInfo.Password, hostingDomain);
                request.Credentials = netCredential;
            }
        }
        #endregion

        /// <summary>
        /// Get file information from google drive file metadata
        /// </summary>
        /// <param name="item">downloadable item</param>
        /// <param name="ct">cancellation token</param>
        /// <returns>Task represent async operation</returns>
        private async Task GetGoogleDriveFileInformation(IDownloadItem item, CancellationToken ct)
        {
            string fileId = _googleHost.GetFileId(item.DownloadLink);
            var googleDriveFile = item as GoogleFileHosting.IGoogleDriveFile;
            googleDriveFile.FileId = fileId;

            await _googleHost.FillDownloadItem(item, fileId, ct);
        }
    }
}
