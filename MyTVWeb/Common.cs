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
                var tempURL = new List<string>();
                returnURL.ForEach(e=> tempURL.Add(ReadTellyPage(e)));
                if (tempURL!= null && tempURL.Count != 0)
                    return tempURL;
            }

            return null;
        }

        private List<string> ReadSaveBox(string url)
        {
            IsDM = "false";
            var urls = ReadSerialPage(url, "savebox");
            if (urls != null && urls.Count > 0)
            {
                if (IsMediaMP4(urls[0]))
                    return urls;
                else
                {
                    IsDM = "true";
                    var u2 = ReadOpenLoad(urls[0]);
                    return new string[] { urls[0], u2 }.ToList();
                }
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

            Regex regex = new Regex(@"<iframe>.*</iframe>", RegexOptions.IgnoreCase);
            var text = regex.Match(strHTML.ToString()).Value;
            return text.ToLower().Replace("<iframe src=", "").Replace("'", "");
        }

        private string ReadOpenLoad(string url)
        {
            var arrEncrypt = new char[] { 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm' }.ToList();
            var arrDecrypt = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' }.ToList();


            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(new Common().GetData(url).ToString());
            var text = doc.DocumentNode.SelectSingleNode("//*[contains(@id,'hiddenurl')]").InnerText;

            /*
            var strHTML = new Common().GetData(url);
            //<span id="hiddenurl">XAZ-m61-p8f~1470792817~73.211.0.0~tBTXYxri</span>
            Regex regex = new Regex(@"<span id=""hiddenurl"">.*</span>", RegexOptions.IgnoreCase);
            var text = regex.Match(strHTML.ToString()).Value;
            text = text.Replace(@"""", "'").Replace("<span id='hiddenurl'>", "").Replace("</span>", "");
            */

            var tempArr = text.ToCharArray().ToList().ConvertAll(c =>
            {
                var index = arrEncrypt.FindIndex(f => f.Equals(c));
                if (index > -1)
                    return arrDecrypt[index];
                return c;
            });


            return "https://openload.co/stream/" + string.Join("", tempArr).Trim() + "?mime=true";

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
                        show.SelectNodes("ul/li/a").ToList().ForEach(e => url.Add(e.Attributes["href"].Value));
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
        public string ChannelURL { get; set; }
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
                    var videoPageURL = dates[i + 1].SelectSingleNode("a").Attributes["href"].Value;
                    return ReadURL(videoPageURL, format.ToLower());
                }
            }

            return null;
        }
    }

    public class GetYoDesi: BaseClass
    {
        private Common web = new Common();

        public override Serial GetShows()
        {
            web = new Common();
            return GetShows("http://www.yodesi.net", "");
        }

        public override Serial GetShows(string pageUrl, string referer)
        {
            var objSerial = new Serial();
            var serial = new Serial();

            var doc = new HtmlDocument();
            var str = web.GetData(pageUrl, referer).ToString();
            doc.LoadHtml(str);
            //var list = doc.DocumentNode.SelectNodes("//nav/ul/li/a");
            var list = doc.DocumentNode.SelectNodes("//*[contains(@class,'menu-item')]/a");
            var objChannel = new List<Channel>();
            foreach (var node in list)
            {
                var text = node.InnerHtml.ToLower();
                if (!(text.Contains("home") || text.Contains("other channels")))
                {
                    var channel = new Channel();
                    channel.Shows = new List<Show>();
                    channel.Name = node.InnerText;
                    channel.ChannelURL = node.Attributes["href"].Value;

                    objChannel.Add(channel);
                }
            }

            GetShowDetails(objChannel);

            serial.Channel = objChannel;

            return serial;
        }

        private void GetShowDetails(List<Channel> objChannel)
        {
            objChannel.ForEach(e =>
            {
                var str = web.GetData(e.ChannelURL, "").ToString();
                var doc = new HtmlDocument();

                doc.LoadHtml(str);
                var showList = doc.DocumentNode.SelectNodes("//*[contains(@class,'latestPost-content')]/a");
                foreach (var showNode in showList)
                {
                    e.Shows.Add(new Show() { Name = showNode.Attributes["title"].Value, URL = showNode.Attributes["href"].Value });
                }
            });
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

        public new List<string> ReadURL(string videoPageURL, string format)
        {
            var str = web.GetData(videoPageURL, "").ToString();

            return null;
        }



    }

    public class Common
    {
        public CookieContainer ckContainer = null;
        public StringBuilder GetData(string url, string referer = "", string postData = "", string host = "")
        {
            HttpWebRequest req = WebRequest.CreateHttp(url);
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; rv:47.0) Gecko/20100101 Firefox/47.0";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            if (host != "" && req.Host == "")
                req.Host = host;

            if (ckContainer != null)
                req.CookieContainer = ckContainer;

            if (referer != "")
                req.Referer = referer;

            if (postData != "")
            {
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                var bytes = ASCIIEncoding.ASCII.GetBytes(postData);
                req.ContentLength = bytes.Length;
                req.GetRequestStream().Write(bytes, 0, bytes.Length);
            }

            HttpWebResponse res = req.GetResponse() as HttpWebResponse;
            if (ckContainer == null) ckContainer = new CookieContainer();
            ckContainer.Add(res.Cookies);

            if(!string.IsNullOrEmpty(res.Headers["Transfer-Encoding"]) && res.Headers["Transfer-Encoding"]=="chunked")
            {
                StringBuilder sb = new StringBuilder();
                Byte[] buf = new byte[8192];
                Stream resStream = res.GetResponseStream();
                int count = 0;
                do
                {
                    count = resStream.Read(buf, 0, buf.Length);
                    if (count != 0)
                    {
                        sb.Append(Encoding.UTF8.GetString(buf, 0, count)); 
                    }
                } while (count > 0);
                return sb;
            }

            using (Stream stream = res.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                return new StringBuilder(reader.ReadToEnd());
            }
        }
    }
}