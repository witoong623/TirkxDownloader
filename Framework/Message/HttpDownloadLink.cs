using Newtonsoft.Json;

namespace TirkxDownloader.Framework
{
    /// <summary>
    /// Use to hold link from browser extension
    /// </summary>
    public class HttpDownloadLink
    {
        [JsonProperty("FileName")]
        public string FileName { get; private set; }
        [JsonProperty("DownloadLink")]
        public string DownloadLink { get; private set; }
    }
}
