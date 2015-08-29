using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TirkxDownloader.Framework;

namespace TirkxDownloader.Models
{
    public class DetailProvider
    {
        public async Task GetFileDetail(DownloadInfo detail, CancellationToken ct)
        {
            try
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(detail.DownloadLink);
                request.Method = "HEAD";
                var response = await request.GetResponseAsync(ct);
                detail.FileSize = response.ContentLength;
            }
            catch (OperationCanceledException) { }
        }

        public bool FillCredential(string doamin)
        {
            throw new NotImplementedException();
        }
    }
}
