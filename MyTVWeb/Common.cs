using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MyTVWeb
{
    public abstract class BaseClass
    {
        public string IsDM { get; set; }

        public virtual Serial GetShows() { return null; }

        public virtual Serial GetShows(string pageUrl, string referer) { return null; }

        public virtual List<string> GetDates(string pageUrl) { return null; }

        public virtual List<string> ReadDatePage(string pageURL, string day, string format) { return null; }

        private bool IsMediaMP4(string url)
        {
            HttpWebResponse response = null;
            var isMediaMP4 = true;

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                if (isMediaMP4)
                    isMediaMP4 = response.ContentType.ToLower().Contains("mp4");
            }
            catch
            {
                isMediaMP4 = false;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }

            return isMediaMP4;
        }

        public List<string> ReadURL(string videoPageURL, string format)
        {
            //Read MP4
            if (format == "video")
                return ReadDownloadLink(videoPageURL);

            if (format == "savebox")
                return ReadSaveBox(videoPageURL);

            IsDM = "true";
            if (format == "watchapne")
            {
                var returnURL = ReadSerialPage(videoPageURL, "watchapne");
                var tempURL = returnURL[0];
                returnURL.Clear();
                returnURL.Add(ReadWatchApnePage(tempURL));
                if (returnURL != null && returnURL.Count != 0)
                    return returnURL;
            }

            if (format == "telly")
            {
                var returnURL = ReadSerialPage(videoPageURL, "telly");
                var tempURL = returnURL[0];
                returnURL.Clear();
                returnURL.Add(ReadTellyPage(tempURL));
                if (returnURL != null && returnURL.Count != 0)
                    return returnURL;
            }

            return null;
        }

        private List<string> ReadSaveBox(string url)
        {
            IsDM = "false";
            var urls = ReadSerialPage(url, "savebox");
            if (urls != null && urls.Count > 0)
            {
                if (!IsMediaMP4(urls[0])) IsDM = "true";
                return urls;
            }

            return null;
        }

        private List<string> ReadDownloadLink(string url)
        {
            IsDM = "false";
            return ReadSerialPage(url, "download");
        }

        private string ReadTellyPage(string url)
        {
            new Common().GetData(url);

            var strHTML = new Common().GetData(url.Replace("http://apne.tv/redirector.php?r=", ""), url);

            Regex regex = new Regex(@".m3u8", RegexOptions.Multiline);
            var end = regex.Match(strHTML.ToString()).Index;

            regex = new Regex(@"file: ""http://", RegexOptions.Multiline);
            var start = regex.Match(strHTML.ToString()).Index;

            var finalText = strHTML.ToString().Substring(start, end - start);

            return finalText.Replace("file:", "").Replace("\"", "") + ".m3u8";
        }

        private string ReadWatchApnePage(string url)
        {
            IsDM = "true";
            var strHTML = new Common().GetData(url);

            //var strHTML = new Common().GetData(url.Replace("http://apne.tv/redirector.php?r=", ""), url);

            Regex regex = new Regex(@"<iframe>.*</iframe>", RegexOptions.IgnoreCase);
            var text = regex.Match(strHTML.ToString()).Value;
            return text.ToLower().Replace("<iframe src=", "").Replace("'", "");
        }

        private List<string> ReadSerialPage(string pageURL, string type)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(new Common().GetData(pageURL).ToString());
            var shows = doc.DocumentNode.SelectNodes("//*[contains(@class,'channel_cont')]");

            var url = new List<string>();
            foreach (var show in shows)
            {
                if (show.SelectSingleNode("div/h2") != null)
                {
                    var text = show.SelectSingleNode("div/h2").InnerHtml;
                    if (text.ToLower().Contains(type))
                    {
                        url.Add(show.SelectSingleNode("ul/li/a").Attributes["href"].Value);
                    }
                }
            }

            return url;
        }
    }


    public class Serial
    {
        public string Source { get; set; }
        public List<Channel> Channel { get; set; }
    }

    public class Channel
    {
        public string Name { get; set; }
        public List<Show> Shows { get; set; }
    }

    public class Show
    {
        public string Name { get; set; }
        public string URL { get; set; }
    }

    public class GetApne : BaseClass
    {
        private Common web = new Common();

        public override Serial GetShows()
        {
            web = new Common();
            return GetShows("http://apne.tv", "http://apne.tv");
        }

        public override Serial GetShows(string pageUrl, string referer)
        {
            var objSerial = new Serial();
            var serial = new Serial();

            var doc = new HtmlDocument();
            var str = web.GetData(pageUrl, referer).ToString();
            doc.LoadHtml(str);
            var list = doc.DocumentNode.SelectNodes("//h2");

            var objChannel = new List<Channel>();
            foreach (var node in list)
            {
                if (!node.InnerText.ToLower().Contains("select channel"))
                {
                    var channel = new Channel();
                    channel.Shows = new List<Show>();
                    channel.Name = node.InnerText;
                    node.SelectNodes("following::ul[1]/li").ToList().ForEach(each =>
                    {
                        channel.Shows.Add(new Show() { Name = each.InnerText, URL = each.SelectSingleNode("a").Attributes["href"].Value });
                    });
                    objChannel.Add(channel);
                }
            }
            serial.Channel = objChannel;

            return serial;
        }

        public override List<string> GetDates(string pageUrl)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(web.GetData(pageUrl).ToString());
            var dates = doc.DocumentNode.SelectNodes("//*[contains(@class,'date_episodes')]");
            if (dates != null)
                return dates.ToList().ConvertAll(e => string.Format("'{0}'", e.SelectSingleNode("span").InnerHtml));

            return null;
        }

        public override List<string> ReadDatePage(string pageURL, string day, string format)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(new Common().GetData(pageURL).ToString());
            var dates = doc.DocumentNode.SelectNodes("//*[contains(@class,'date_episodes')]");

            foreach (var date in dates)
            {
                var d = date.SelectSingleNode("span").InnerHtml;
                if (day.Equals(d))
                {
                    var videoPageURL = date.Attributes["href"].Value;
                    return ReadURL(videoPageURL, format.ToLower());
                }
            }

            return null;
        }





    }

    public class GetBollyStop : BaseClass
    {
        private Common web = new Common();

        public override Serial GetShows()
        {
            web = new Common();
            return GetShows("http://bollystop.com/", "http://bollystop.com/");
        }

        public override Serial GetShows(string pageUrl, string referer)
        {
            var objSerial = new Serial();
            var serial = new Serial();

            var doc = new HtmlDocument();
            var str = web.GetData(pageUrl, referer).ToString();
            doc.LoadHtml(str);
            var list = doc.DocumentNode.SelectNodes("//h2");

            var objChannel = new List<Channel>();
            foreach (var node in list)
            {
                try
                {
                    if (!node.InnerText.ToLower().Contains("select channel") && !node.InnerText.ToLower().Contains("latest released movies"))
                    {
                        var channel = new Channel();
                        channel.Shows = new List<Show>();
                        channel.Name = node.InnerText;
                        node.SelectNodes("following::ul[1]/li").ToList().ForEach(each =>
                        {
                            channel.Shows.Add(new Show() { Name = each.InnerText, URL = each.SelectSingleNode("a").Attributes["href"].Value });
                        });
                        objChannel.Add(channel);
                    }
                }
                catch
                {
                }
            }
            serial.Channel = objChannel;

            return serial;
        }

        public override List<string> GetDates(string pageUrl)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(web.GetData(pageUrl).ToString());
            var dates = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'recently_added')]");
            if (dates != null)
            {
                return dates.SelectNodes("ul/li/p[position() mod 2 = 1]").ToList().ConvertAll(e => string.Format("'{0}'", e.InnerText));
            }

            return null;
        }

        public override List<string> ReadDatePage(string pageURL, string day, string format)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(new Common().GetData(pageURL).ToString());
            var root = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'recently_added')]");
            var dates = root.SelectNodes("ul/li/p").ToList();
            for (int i = 0; i < dates.Count; i = i + 2)
            {
                var date = dates[i];
                var d = date.InnerText;
                if (day.Equals(d))
                {
                    var videoPageURL = dates[i+1].SelectSingleNode("a").Attributes["href"].Value;
                    return ReadURL(videoPageURL, format.ToLower());
                }
            }

            return null;
        }
    }

    public class Common
    {
        public CookieContainer ckContainer = null;
        public StringBuilder GetData(string url, string referer = "")
        {
            HttpWebRequest req = WebRequest.CreateHttp(url);
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
            req.Accept = "text/html,application/xhtml+xml,application/xml; q=0.9,image/webp,*/*;q=0.8";

            if (ckContainer != null)
                req.CookieContainer = ckContainer;

            if (referer != "")
                req.Referer = referer;

            HttpWebResponse res = req.GetResponse() as HttpWebResponse;
            if (ckContainer == null) ckContainer = new CookieContainer();
            ckContainer.Add(res.Cookies);

            using (Stream stream = res.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                return new StringBuilder(reader.ReadToEnd());
            }
        }
    }
}