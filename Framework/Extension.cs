using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace TirkxDownloader.Framework
{
    public static class Extension
    {
        public static async Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest request, CancellationToken ct)
        {
            using (ct.Register(() => request.Abort(), useSynchronizationContext: false))
            {
                try
                {
                    var response = await request.GetResponseAsync();
                    ct.ThrowIfCancellationRequested();

                    return (HttpWebResponse)response;
                }
                catch (WebException webEx)
                {
                    if (ct.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(webEx.Message, webEx, ct);
                    }

                    throw;
                }
            }
        }
    }
}
