using HtmlAgilityPack;
using System.Net;
using System.Text;

namespace MyTV
{
    public class MyTVService : IMyTV
    {
        const string rootURL = "http://apne.tv/Hindi-Serial";
        const string SerialsName = "Crime-Patrol|Sankat-Mochan-Mahabali-Hanumaan|Yeh-Rishta-Kya-Kehlata-Hai";

        public string[] GetSerials()
        {
            return SerialsName.Split(new char[] { '|' });
        }

        public string GetURL(string serial, string day)
        {
            var url = ReadPage(serial, day);
            if (url.ToLower().Contains("mp4"))
                return "data=" + url;
            else
                return "data=";
        }

        private string ReadPage(string serial, string day)
        {
            var pageURL = string.Format("{0}/{1}", rootURL, serial);

            var doc = new HtmlDocument();
            doc.LoadHtml(GetWebContent(pageURL).ToString());
            var dates = doc.DocumentNode.SelectNodes("//*[contains(@class,'date_episodes')]");

            foreach (var date in dates)
            {
                var d = date.SelectSingleNode("span").InnerHtml;
                if (day.Equals(d))
                {
                    var videoURL = date.Attributes["href"].Value;
                    return ReadSerialPage(videoURL);
                }
            }

            return "";
        }

        private StringBuilder GetWebContent(string url)
        {
            using (var web = new WebClient())
            {
                return new StringBuilder(web.DownloadString(url));
            }
        }

        private string ReadSerialPage(string pageURL)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(GetWebContent(pageURL).ToString());
            var shows = doc.DocumentNode.SelectNodes("//*[contains(@class,'channel_cont')]");

            foreach (var show in shows)
            {
                var text = show.SelectSingleNode("div/h2").InnerHtml;
                if (text.ToLower().Contains("download") && text.ToLower().Contains("single"))
                {
                    return show.SelectSingleNode("ul/li/a").Attributes["href"].Value;
                }
            }

            return "";
        }
    }
}
