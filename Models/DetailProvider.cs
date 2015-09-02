using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TirkxDownloader.Framework;
using TirkxDownloader.Models;

namespace TirkxDownloader.Models
{
    public class DetailProvider
    {
        private AuthorizationManager _authenticationManager;

        public DetailProvider(AuthorizationManager authenticationManager)
        {
            _authenticationManager = authenticationManager;
        }

        public async Task GetFileDetail(DownloadInfo detail, CancellationToken ct)
        {
            try
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(detail.DownloadLink);
                request.Method = "HEAD";
                var response = await request.GetResponseAsync(ct);
                detail.FileSize = response.ContentLength;
                response.Close();
            }
            catch (OperationCanceledException) { }
        }

        public async Task FillCredential(HttpWebRequest request)
        {
            string targetDomain = "";
            string domain = request.Host;
            AuthorizationInfo authorizationInfo = _authenticationManager.GetCredential(domain);

            using (StreamReader str = new StreamReader("Target domain.dat", Encoding.UTF8))
            {
                string storedDomian;

                while ((storedDomian = await str.ReadLineAsync()) != null)
                {
                    if (storedDomian.Like(domain))
                    {
                        targetDomain = storedDomian;
                        
                        // if it isn't wildcards, it done
                        if (!storedDomian.Contains("*"))
                        {
                            break;
                        }
                    }
                }
            }

            AuthorizationInfo credential = _authenticationManager.GetCredential(targetDomain);

            if (credential != null)
            {
                var netCredential = new NetworkCredential(credential.Username, credential.Password, domain);
                request.Credentials = netCredential;
            }
            else
            {
                // if no credential was found, try to use default credential
                request.UseDefaultCredentials = true;
            }
        }
    }
}
