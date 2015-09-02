using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
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

        public static T[] Dequeue<T>(this Queue<T> queue, int count)
        {
            T[] list = new T[count];

            for (int i = 0; i < count; i++)
            {
                list[i] = queue.Dequeue();
            }

            return list;
        }

        /// <summary>
        /// Compares the string against a given pattern.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="pattern">The pattern to match, where "*" means any sequence of characters, and "?" means any single character.</param>
        /// <returns><c>true</c> if the string matches the given pattern; otherwise <c>false</c>.</returns>
        public static bool Like(this string str, string pattern)
        {
            return new Regex(
                "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            ).IsMatch(str);
        }
    }
}
