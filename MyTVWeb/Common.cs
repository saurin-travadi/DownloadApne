using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;

namespace MyTVWeb
{
    public abstract class BaseClass
    {
        protected MyWebClient web = new MyWebClient();
        public string show;

        public string IsDM { get; set; }

        public virtual Serial GetShows() { return null; }

        public virtual Serial GetShows(string pageUrl, string referer) { return null; }

        public virtual List<string> GetDates(string pageUrl) { return null; }

        public virtual List<string> ReadDatePage(string pageURL, string day, string format) { return null; }

        private bool IsMediaMP4(string url)
        {
            return new MyWebClient().IsMediaMP4(url);
        }

        public virtual List<string> ReadURL(string videoPageURL, string format, bool embed)
        {
            var returnURL = ReadSerialPage(videoPageURL, format);

            if (embed)
            {
                IsDM = "true";
                return returnURL;
            }
            else
            {
                return foo(returnURL, videoPageURL, format);
            }
        }

        public virtual string ReadVideoURL(string videoURL) { return null; }

        public string ReadFrame(string page)
        {
            List<string> returnUrl = new List<string>();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(web.GetData(page).ToString());

            var node = doc.DocumentNode.SelectNodes("//iframe");
            if (node == null)
                return "";

            foreach (var n in node)
            {
                if (!n.Attributes["src"].Value.Contains("ads") && !n.Attributes["src"].Value.Contains("facebook"))
                {
                    var url = n.Attributes["src"].Value;
                    return url;
                }
            }

            return "";
        }


        public List<string> foo(List<string> returnURL, string videoPageURL, string format)
        {

            IsDM = "true";
            if (format == "watchapne" || format == "playapne" || format == "newapne" || format == "tunelink")
            {
                var tempURL = returnURL[0];
                returnURL.Clear();

                if (format == "playapne")
                    returnURL.Add(ReadApnePage(tempURL, videoPageURL));
                else if (format == "newapne")
                    returnURL.Add(ReadNewApnePage(tempURL, videoPageURL));

                if (returnURL != null && returnURL.Count != 0)
                    return returnURL;
            }
            return null;
        }

        private string ReadApnePage(string url, string refererPage)
        {
            var strHTML = web.GetData(url, refererPage);
            Regex regex = new Regex(@"https:.*html", RegexOptions.IgnoreCase);
            var text = regex.Match(strHTML.ToString()).Value;

            web.GetData(text, url, "", "videoapne.co");
            return "";

        }

        private string ReadNewApnePage(string url, string refererPage)
        {
            IsDM = "true";
            var strHTML = web.GetData(url, refererPage);

            Regex regex = new Regex(@"iframe.*http.*""", RegexOptions.IgnoreCase);
            var text = regex.Matches(strHTML.ToString())[0].Value.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1];
            text = text.Replace("src=", "").Replace("\"", "");

            var html = web.GetData(text, "http://hindistopss.com");

            regex = new Regex(@"file.*"",", RegexOptions.Singleline);
            text = regex.Match(html.ToString()).Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)[0];
            text = text.Replace("file:", "").Replace("\"", "").Replace(" ", "");

            return text;
        }

        public virtual List<string> ReadSerialPage(string pageURL, string type)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(web.GetData(pageURL).ToString());
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
                else if (show.SelectSingleNode("div/h2/div/img") != null)
                {
                    var img = show.SelectSingleNode("div/h2/div/img").Attributes["src"].Value;
                    if (img.ToLower().Contains("daily.jpg") && type == "Dailymotion")
                    {
                        show.SelectNodes("ul/li/a").ToList().ForEach(e => url.Add(e.Attributes["href"].Value));
                    }
                }
            }

            return url;
        }
    }


    [Serializable]
    public class Serial
    {
        public string Source { get; set; }
        public List<Channel> Channel { get; set; }
    }

    [Serializable]
    public class Channel
    {
        public string Name { get; set; }
        public string ChannelURL { get; set; }
        public List<Show> Shows { get; set; }
        public string ImageURL { get; set; }
    }

    [Serializable]
    public class Show
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public string ImageURL { get; set; }
    }

    public class GetApne : BaseClass
    {
        public override Serial GetShows()
        {
            web = new MyWebClient();
            return GetShows("http://apnetv.co", "http://apnetv.co");
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

        public Serial GetShowsRSS(string pageUrl, string referer)
        {
            return null;
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
            doc.LoadHtml(new MyWebClient().GetData(pageURL).ToString());
            var dates = doc.DocumentNode.SelectNodes("//*[contains(@class,'date_episodes')]");

            for (int i = 0; i < dates.Count; i++)
            {
                var date = dates[0];
                var d = date.InnerText;
                if (d.Contains(day) || (day.Equals("") && i == 0))
                {
                    var videoPageURL = date.Attributes["href"].Value;
                    videoPageURL = videoPageURL.Replace("https", "http");
                    return ReadURL(videoPageURL, format.ToLower(), false);
                }
            }

            return null;
        }

        public override List<string> ReadSerialPage(string pageURL, string type)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(web.GetData(pageURL).ToString());
            var shows = doc.DocumentNode.SelectNodes("//*[contains(@class,'bottom_episode_list')]/ul/li");

            var url = new List<string>();
            foreach (var show in shows)
            {
                if (show.SelectSingleNode("a").InnerText.ToLower().Contains(type))
                {
                    url.Add(show.SelectSingleNode("a").Attributes["href"].Value);
                }
            }

            return url;
        }
    }

    public class GetBollyStop : BaseClass
    {
        public override Serial GetShows()
        {
            web = new MyWebClient();
            return GetShows("http://bollystop.cc/", "http://bollystop.cc/");
        }

        public Serial GetShowsRSS(string pageUrl, string referer)
        {
            var channel = new Channel();
            channel.Shows = new List<Show>();

            var doc = new HtmlDocument();
            var str = web.GetData(pageUrl, referer).ToString();
            doc.LoadHtml(str);

            channel.Name = doc.DocumentNode.SelectNodes("//*[contains(@class,'news-box hot')]/h2/a")[0].Attributes["title"].Value;
            var list = doc.DocumentNode.SelectNodes("//*[contains(@class,'serials_div')]")[0];
            foreach (var node in list.SelectNodes("div[contains(@class,'four-col')]"))
            {
                var show = new Show();
                var title = node.SelectSingleNode("a[contains(@class,'serial_title')]");
                show.Name = title.InnerText;
                show.URL = title.Attributes["href"].Value;

                var img = node.SelectSingleNode("a[contains(@class,'serial_img')]/img");
                show.ImageURL = img.Attributes["src"].Value;

                channel.Shows.Add(show);
            }


            return new Serial() { Source = "", Channel = new List<Channel>() { channel } };
        }

        public override Serial GetShows(string pageUrl, string referer)
        {
            var objSerial = new Serial();
            var serial = new Serial();

            var doc = new HtmlDocument();
            var str = web.GetData(pageUrl, referer).ToString();
            doc.LoadHtml(str);
            var list = doc.DocumentNode.SelectNodes("//*[contains(@class,'tabscontent')]/h2");
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
                        try
                        {
                            var img = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'tabs')]/li[contains(.,'" + node.InnerText.Replace("Shows", "").Trim() + "')]");
                            channel.ImageURL = img.SelectSingleNode("img").Attributes["src"].Value;
                        }
                        catch { }

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
            doc.LoadHtml(web.GetData(pageURL).ToString());
            var root = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'recently_added')]");
            var dates = root.SelectNodes("ul/li/p").ToList();
            for (int i = 0; i < dates.Count; i = i + 2)
            {
                var date = dates[i];
                var d = date.InnerText;
                if (day.Equals(d) || (day.Equals("") && i == 0))
                {
                    var videoPageURL = dates[i + 1].SelectSingleNode("a").Attributes["href"].Value;

                    return ReadURL(videoPageURL, format.ToLower(), false);
                }
            }

            return null;
        }
    }

    public class GetDesiRulez : BaseClass
    {
        string baseURL;
        public GetDesiRulez()
        {
            web = new MyWebClient();
            baseURL = "http://www.desirulez.cc/";
        }

        public override Serial GetShows()
        {
            if (HttpContext.Current.Cache["DesiRulezShows"] != null)
                return HttpContext.Current.Cache["DesiRulezShows"] as Serial;
            else
            {
                var objSerial = new Serial();
                var serial = new Serial();

                var doc = new HtmlDocument();
                var str = web.GetData(baseURL).ToString();
                doc.LoadHtml(str);
                var list = doc.DocumentNode.SelectSingleNode("//*[contains(@id,'cat41')]");
                var nodes = list.SelectNodes("div[contains(@class,'forumbitBody')]/ol/li");
                var objChannel = new List<Channel>();
                foreach (var node in nodes)
                {
                    var a = node.SelectSingleNode("div/div/div/div/div/h2/a");
                    var title = a.InnerText;
                    var href = a.Attributes["href"];

                    var channel = new Channel();
                    channel.Shows = new List<Show>();
                    channel.Name = title;

                    var lst = node.SelectNodes("div/div/div/div/div/div/ol/div/div");
                    if (lst != null)
                    {
                        lst.ToList().ForEach(each =>
                        {
                            var sh = each.SelectNodes("div/ul/li/a");
                            if (sh != null)
                            {
                                sh.ToList().ForEach(e =>
                                {
                                    channel.Shows.Add(new Show() { Name = e.InnerText, URL = e.Attributes["href"].Value });
                                });
                            }
                        });
                        try
                        {
                            var img = node.SelectSingleNode("div/div/div/div/p/img");
                            channel.ImageURL = img.Attributes["src"].Value;
                        }
                        catch { }

                        objChannel.Add(channel);
                    }
                }

                serial.Channel = objChannel;
                HttpContext.Current.Cache.Add("DesiRulezShows", serial, null, System.DateTime.Now.AddHours(2), TimeSpan.Zero, CacheItemPriority.Normal, null);

                return serial;
            }
        }

        public override List<string> GetDates(string pageUrl)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(web.GetData(pageUrl).ToString());
            var dates = doc.DocumentNode.SelectNodes("//h3[contains(@class,'threadtitle')]");
            return dates.ToList().ConvertAll(e => e.SelectSingleNode("a").InnerText);
        }

        public override List<string> ReadDatePage(string pageURL, string day, string format)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(web.GetData(pageURL).ToString());
            var dates = doc.DocumentNode.SelectNodes("//h3[contains(@class,'threadtitle')]");
            for (int i = 0; i < dates.Count; i++)
            {
                var date = dates[i];
                var d = date.SelectSingleNode("a").InnerText;
                if (day.Equals(d) || (day.Equals("") && i == 0))
                {
                    var videoPageURL = date.SelectSingleNode("a").Attributes["href"].Value;
                    return ReadURL(videoPageURL, format.ToLower());
                }
            }

            return null;
        }

        private List<string> ReadURL(string videoPageURL, string format)
        {
            var urls = new List<string>();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(web.GetData(videoPageURL).ToString());
            var formatNode = doc.DocumentNode.SelectSingleNode("//blockquote[contains(@class,'postcontent restore')]/div");
            if (formatNode == null)
                return new List<string>();

            var selectedNodes = formatNode.ChildNodes.Where(w => !w.Name.Equals("br") && !w.Name.Equals("#text") && !w.Name.Equals("font"));
            var interestedNodes = false;
            foreach (var node in selectedNodes)
            {
                if (node.Name.Equals("b"))
                {
                    if (node.InnerText.ToLower().Contains(format))
                        interestedNodes = true;
                    else
                        interestedNodes = false;
                }
                else if (interestedNodes && node.Name.Equals("a"))
                {
                    urls.Add(node.Attributes["href"].Value);
                }
            }

            return urls;
        }

        public override List<string> ReadURL(string videoPageURL, string format, bool embed)
        {
            format = format.ToLower();
            var url = new List<string>();
            var temp = ReadFrame(videoPageURL);
            if (temp == "")
                return new List<string>();

            if (format.Contains("tvlogy"))
            {
                var uri = new Uri(temp);
                var id = (uri.Query.Split('='))[1];

                var urlVideo = uri.Scheme + "://" + uri.Host + "/hls/" + id + "/" + id + ".m3u8";
                url.Add(urlVideo);
            }
            else if (format.Contains("dailymotion"))
            {
                var strHTML = web.GetData(temp).ToString();

                Regex regex = new Regex(@"src.*m3u8", RegexOptions.IgnoreCase);
                var text = regex.Match(strHTML.ToString()).Value;
                url.Add(text.Substring(6));
            }

            return url;
        }

    }

    public class YoDesi : BaseClass
    {
        string baseURL;
        public YoDesi()
        {
            web = new MyWebClient();
            baseURL = "http://www.yodesitv.info/";
        }

        public override Serial GetShows()
        {
            //if (HttpContext.Current.Cache["YoDesiShows"] != null)
            //    return HttpContext.Current.Cache["YoDesiShows"] as Serial;
            //else
            {
                var objSerial = new Serial();
                var serial = new Serial();

                var doc = new HtmlDocument();
                var str = web.GetData(baseURL).ToString();
                doc.LoadHtml(str);
                var titles = doc.DocumentNode.SelectNodes("//div[contains(@class,'home-channel-title')]").ToList().ConvertAll(c => c.InnerText.Replace("\t", "").Replace("\n", ""));
                var nodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'one_sixth column-last')]/a").ToList().ConvertAll(c => c.Attributes["href"].Value);

                var objChannel = new List<Channel>();
                for (int cnt = 0; cnt < titles.Count; cnt++)
                {
                    doc.LoadHtml(web.GetData(nodes[cnt]).ToString());
                    var shows = doc.DocumentNode.SelectSingleNode("//div[contains(@id,'tab-0-title-1')]").SelectNodes("div/p/a").ToList().ConvertAll(c => new Show() { URL = c.Attributes["href"].Value, Name = c.InnerText.Replace("\t", "").Replace("\n", "") });

                    var channel = new Channel();
                    channel.Shows = new List<Show>();
                    channel.Name = titles[cnt];
                    channel.Shows.AddRange(shows);

                    objChannel.Add(channel);
                }

                serial.Channel = objChannel;
                HttpContext.Current.Cache.Add("YoDesiShows", serial, null, System.DateTime.Now.AddHours(2), TimeSpan.Zero, CacheItemPriority.Normal, null);

                return serial;
            }
        }

        public override List<string> GetDates(string pageUrl)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(web.GetData(pageUrl).ToString());
            var dates = doc.DocumentNode.SelectNodes("//div[contains(@class,'post-image')]");
            return dates.ToList().ConvertAll(e => e.Attributes["title"].Value);
        }

        public override List<string> ReadDatePage(string pageURL, string day, string format)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(web.GetData(pageURL).ToString());
            var dates = doc.DocumentNode.SelectNodes("//a[contains(@class,'post-image')]");
            for (int i = 0; i < dates.Count; i++)
            {
                var date = dates[i];
                var d = date.Attributes["title"].Value;
                if (day.Equals(d) || (day.Equals("") && i == 0))
                {
                    var videoPageURL = date.Attributes["href"].Value;
                    return ReadURL(videoPageURL, format.ToLower());
                }
            }

            return new List<string>();
        }

        private List<string> ReadURL(string videoPageURL, string format)
        {
            IsDM = "true";
            var url = new List<string>();

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(web.GetData(videoPageURL).ToString());
            var formatNode = doc.DocumentNode.SelectNodes("//div[contains(@class,'buttons btn_green')]/span");
            if (formatNode != null && formatNode.Where(w => w.InnerText.ToLower().Contains(format)).Count() > 0)
            {
                var node = formatNode.Where(w => w.InnerText.ToLower().Contains(format)).First();
                var next = node.ParentNode.NextSibling.NextSibling;
                //if (embed)
                return next.SelectNodes("a").ToList().ConvertAll(c => c.Attributes["href"].Value);
                //else
                //{
                //    return GetVideoURL(next.SelectNodes("a").ToList().ConvertAll(c => c.Attributes["href"].Value));
                //}
            }
            return url;
        }

        public override List<string> ReadURL(string videoPageURL, string format, bool embed)
        {
            format = format.ToLower();
            var url = new List<string>();
            var temp = ReadFrame(videoPageURL);

            if (format.Contains("vkprime"))
            {
                var strHTML = web.GetData(temp).ToString();

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(strHTML);
                var domain = doc.DocumentNode.SelectSingleNode("//div[contains(@id,'vplayer')]/img").Attributes["src"].Value;

                //var brow = new WebBrow() { URL = "https://beautifier.io", DATA = strHTML };
                //brow.GetVideo();

                Regex regex = new Regex(@"eval.*vplayer", RegexOptions.IgnoreCase);
                var text = regex.Match(strHTML.ToString()).Value;
                var parts = text.Split('|').Reverse().ToList();
                var ind = parts.FindIndex(f => f.Equals("sources"));
                if (ind > 0)
                {
                    var id = parts[ind + 1];
                    var host = new Uri(domain).Host;
                    var scheme = new Uri(domain).Scheme;
                    url.Add(scheme + "://" + host + "/" + id + "/v.mp4");
                }
                else
                    url.Add("");
            }
            else
                url.Add("");

            return url;
        }

        private List<string> GetVideoURL(List<string> pageURL)
        {
            Regex regex = new Regex(@"iframe.*http.*html", RegexOptions.IgnoreCase);
            return pageURL.ConvertAll(page =>
            {
                var pagesource = web.GetData(page).ToString();
                var text = regex.Match(pagesource.ToString()).Value;
                text = text.Split(new string[] { "src" }, StringSplitOptions.RemoveEmptyEntries)[1];
                return text.Replace("='", "");
            });
        }

        public override string ReadVideoURL(string videoURL)
        {
            return "abc";
        }
    }
}