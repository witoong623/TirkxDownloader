using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using TirkxDownloader.Framework.Interface;

namespace TirkxDownloader.Services
{
    /// <summary>
    /// Represent Google Drive hight level API for downloading
    /// </summary>
    public class GoogleFileHosting : IAsyncInitialization
    {
        private string[] Scopes = { DriveService.Scope.DriveReadonly };
        private const string APPLICATION_NAME = "TirkxDownloader";
        private DriveService _driveService;

        public Task Initialization { get; private set; }

        public GoogleFileHosting()
        {
            Initialization = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            UserCredential credentials;

            string credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Credentials");

            credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets()
                {
                    ClientId = APIKey.Google_Client_Id,
                    ClientSecret = APIKey.Google_Client_Secret
                },
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath));


            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials,
                ApplicationName = APPLICATION_NAME
            });
        }

        /// <summary>
        /// Fill information to DownloadInfo model
        /// </summary>
        /// <param name="item">DownloadInfo model instance</param>
        /// <param name="fileId">file id</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Task represent async operation</returns>
        internal async Task FillDownloadItem(IDownloadItem item, string fileId, CancellationToken ct)
        {
            await Initialization;

            Google.Apis.Drive.v2.Data.File gFile = await _driveService.Files.Get(fileId).ExecuteAsync(ct);
            item.FileName = gFile.OriginalFilename;
            item.FileSizeInBytes = gFile.FileSize ?? 0;
            item.FileSize = gFile.FileSize / 1048576 ?? 0;
        }

        /// <summary>
        /// Get http response stream async
        /// </summary>
        /// <param name="item">DownloadInfo instance</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Task constain http response stream</returns>
        public async Task<Stream> GetHttpResponseStreamAsync(IGoogleDriveFile item, CancellationToken ct)
        {
            Google.Apis.Drive.v2.Data.File file = await _driveService.Files.Get(item.FileId).ExecuteAsync(ct);

            return await _driveService.HttpClient.GetStreamAsync(file.DownloadUrl);
        }

        /// <summary>
        /// Get Google Drive file id from uri
        /// </summary>
        /// <param name="uri">uri string</param>
        /// <returns>file id</returns>
        public string GetFileId(string uri)
        {
            var wholeMatch = Regex.Match(uri, @"https:\/\/.*?\.google\.com\/uc\?export=download&confirm=.*?&id=(.*)");

            if (wholeMatch.Success)
            {
                return wholeMatch.Groups[1].Value;
            }
            else
            {
                throw new ArgumentException("uri isn't google drive download link");
            }
        }

        /// <summary>
        /// Get Google Drive public file list id from uri
        /// </summary>
        /// <param name="uri">uri</param>
        /// <returns>list id</returns>
        public string GetListId(string uri)
        {
            var wholeMatch = Regex.Match(uri, @"https:\/\/drive\.google\.com\/folderview\?id=(.*?)&usp=drive_web");
            
            if (wholeMatch.Success)
            {
                return wholeMatch.Groups[1].Value;
            }
            else
            {
                throw new ArgumentException("uri isn't google drive file list");
            }
        }

        public interface IGoogleDriveFile
        {
            string FileId { get; set; }
        }
    }
}
