using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using Caliburn.Micro;
using HtmlAgilityPack;

namespace TirkxDownloader.Models
{
    class TirkxDownloadParser
    {
        private readonly string MainDownloadThreadLink = "http://forum.tirkx.com/main/forumdisplay.php?74";
        private readonly string ForumLink = "http://forum.tirkx.com/main/";

        public BindableCollection<object> LoadAnimeList(int page)
        {
            throw new NotImplementedException();
            

        }

        private List<string> LoadThreadLink(int page)
        {
            List<string> link = new List<string>();
            HtmlWeb tirkx = new HtmlWeb();
            var tirkxDownloadPage = tirkx.Load(MainDownloadThreadLink);
            var downloadLink = tirkxDownloadPage.DocumentNode.SelectNodes("//h3[@class='threadtitle']/a");

            foreach (var tag in downloadLink)
            {
                link.Add(tag.Attributes["href"].Value);
            }

            return link;
        }
    }
}
