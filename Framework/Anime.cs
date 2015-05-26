using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TirkxDownloader.Models;

namespace TirkxDownloader.Framework
{
    public class Anime
    {
        private string threadLink;
        private DateTime threadStartDate;

        public string Name { get; set; }
        public Dictionary<string, List<AnimeFileInfo>> DownloadLink { get; set; }

        public string ThreadLink
        {
            get { return threadLink; }
            set
            {
                threadLink = TirkxDownloadParser.ForumLink + value;
            }
        }

        public DateTime ThreadStartDate
        {
            get { return threadStartDate; }
            set
            {
                threadStartDate = value;
            }
        }

        public Anime()
        {
            DownloadLink = new Dictionary<string, List<AnimeFileInfo>>();
        }
    }
}
