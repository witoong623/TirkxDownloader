using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using System.Threading.Tasks;
using Caliburn.Micro;
using HtmlAgilityPack;
using TirkxDownloader.Framework;

namespace TirkxDownloader.Models
{
    /// <summary>
    /// Tirkx download thread paser - currently is abandoned
    /// </summary>
    public class TirkxDownloadParser
    {
        private int ParsedCount;
        private string MainDownloadThreadLink = "http://forum.tirkx.com/main/forumdisplay.php?74";
        public static readonly string ForumLink = "http://forum.tirkx.com/main/showthread.php?";
        private string DatePattern = @"\d{2}?-\d{2}?-\d{4}?";
        private string NamePattern = @"\[(.*?)\]";
        private string ThreadLinkPattern = @"_\d{1,}$";

        public async Task<BindableCollection<Anime>> LoadAnimeList()
        {
            var animeList = await LoadThreadLink();

            try
            {
                foreach (Anime anime in animeList)
                {
                    WebProcessor web = new WebProcessor();
                    var threadElement = await web.GetGeneratedHTML(anime.ThreadLink);
                    var downloadElement = threadElement.GetElementById("downloadsList");
                    var lastAddedKey = "";

                    for (int i = 0; i < downloadElement.Children.Count; i++)
                    {
                        // downloadElement.Children[i] is div that warp each line
                        if (downloadElement.Children[i].GetAttribute("class").Equals("dl_seper"))
                        {
                            // downloadElement.Children[i].Children[0] is "a" tag that indicate fansub thread link
                            lastAddedKey = downloadElement.Children[i].Children[0].InnerText;
                            anime.DownloadLink.Add(lastAddedKey, new List<AnimeFileInfo>());
                        }
                        else
                        {
                            string downloadLink = downloadElement.Children[i].Children[0].Children[0].GetAttribute("href");
                            string fileName = downloadElement.Children[i].Children[0].Children[0].InnerText;
                            anime.DownloadLink[lastAddedKey].Add(new AnimeFileInfo { FileName = fileName, Link = downloadLink });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Message {0} \n {1}", ex.Message, ex.StackTrace));
            }

            return new BindableCollection<Anime>(animeList);
        }

        #region Helper method

        private async Task<List<Anime>> LoadThreadLink()
        {
            var load = Task.Run<List<Anime>>(() =>
            {
                var animeEntries = new List<Anime>();
                var tirkx = new HtmlWeb();
                var tirkxDownloadPage = tirkx.Load(MainDownloadThreadLink);
                var threadNodes = tirkxDownloadPage.DocumentNode.SelectNodes("//li[contains(@class, 'threadbit')]");

                foreach (var eachThread in threadNodes)
                {
                    var aTag = eachThread.SelectSingleNode(".//h3[@class='threadtitle']/a");
                    string name = ParseName(aTag.InnerText);
                    string link = ParseThreadLink(eachThread.Attributes["id"].Value);
                    string rawDate = eachThread.SelectSingleNode("//span[@class='label']/a").Attributes["title"].Value;
                    DateTime startDate = DateTime.Parse(Regex.Match(rawDate, DatePattern).Value);

                    animeEntries.Add(new Anime { Name = name, ThreadStartDate = startDate, ThreadLink = link });
                }

                return animeEntries;
            });

            return await load;
        }

        private string ParseName(string aInnerText)
        {
            string removeText;
            var matchPattern = Regex.Match(aInnerText, NamePattern);

            if (matchPattern.Success)
            {
                removeText = matchPattern.Value;
            }
            else
            {
                return aInnerText;
            }

            int start = aInnerText.IndexOf(removeText);
            string afterRemove = aInnerText.Remove(start, removeText.Length);

            return afterRemove.Trim();
        }

        private string ParseThreadLink(string liAttrValue)
        {
            string match = Regex.Match(liAttrValue, ThreadLinkPattern).Value;

            return match.Remove(0, 1);
        }

        #endregion
    }
}
