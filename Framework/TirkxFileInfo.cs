using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TirkxDownloader.Framework
{
    public class TirkxFileInfo
    {
        [JsonProperty("FileName")]
        public string FileName { get; private set; }
        [JsonProperty("DownloadLink")]
        public string DownloadLink { get; private set; }
    }
}
