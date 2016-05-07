using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MyTVWeb
{
    public class GetURL : IHttpHandler
    {
        const string RootURL = "http://apne.tv/Hindi-Serial";
        //const string RootURL = "http://bollystop.com";

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        private string IsDM { get; set; }

        public void ProcessRequest(HttpContext context)
        {
            var url = ReadDatePage(context.Request.QueryString["s"], context.Request.QueryString["d"]);

            context.Response.AddHeader("Content-Type", "application/json\n\n");
            context.Response.Buffer = true;

            var scriptFile = Path.Combine(context.Server.MapPath("."), "script.rjs");
            var data = File.ReadAllText(scriptFile);

            data = data.Replace("%SHOWS%", "'" + context.Request.QueryString["s"] + "'");
            data = data.Replace("%CHANNELS%", "");
            data = data.Replace("%DATES%", "");
            data = data.Replace("%URL%", url[0].Trim());
            if(url.Count>1)
                data = data.Replace("%URL1%", url[1].Trim());
            else
                data = data.Replace("%URL1%", "");

            data = data.Replace("%ISDM%", IsDM);

            context.Response.Write(data);
            context.Response.Flush();
        }

        private List<string> ReadDatePage(string serial, string day)
        {
            var returnURL = new List<string>();
            var pageURL = string.Format("{0}/{1}", RootURL, serial);

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(GetWebContent(pageURL).ToString());
            var dates = doc.DocumentNode.SelectNodes("//*[contains(@class,'date_episodes')]");

            foreach (var date in dates)
            {
                var d = date.SelectSingleNode("span").InnerHtml;
                if (day.Equals(d))
                {
                    var videoPageURL = date.Attributes["href"].Value;
                    
                    //Read MP4
                    returnURL = ReadSerialPage(videoPageURL,"download");

                    if (returnURL.Count==0 || returnURL[0].ToLower().Contains("amazon.com"))
                    {
                        //Read Flash
                        returnURL = ReadSerialPage(videoPageURL, "telly");
                        var tempURL = returnURL[0];
                        returnURL.Clear();
                        returnURL.Add(ReadSerialAltPage(tempURL));
                        IsDM = "true";
                    }
                    else
                    {
                        IsDM = "false";
                    }
                }
            }

            return returnURL;
        }

        private string ReadSerialAltPage(string url)
        {
            //var url = "http://apne.tv/redirector.php?r=http://myapne.com/Hindi-Serial/discussions/C.I.D&s=735911";
            GetWebContent(url);

            var strHTML = GetWebContent(url.Replace("http://apne.tv/redirector.php?r=", ""), url);
            
            Regex regex = new Regex(@".m3u8", RegexOptions.Multiline);
            var end = regex.Match(strHTML.ToString()).Index;

            regex = new Regex(@"file: ""http://", RegexOptions.Multiline);
            var start = regex.Match(strHTML.ToString()).Index;

            var finalText = strHTML.ToString().Substring(start, end - start);

            return finalText.Replace("file:", "").Replace("\"", "") + ".m3u8";
        }

        private List<string> ReadSerialPage(string pageURL, string type= "download")
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(GetWebContent(pageURL).ToString());
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

        CookieContainer ckContainer = null;
        private StringBuilder GetWebContent(string url, string referer ="")
        {
            HttpWebRequest req = WebRequest.CreateHttp(url);
            if (ckContainer != null)
                req.CookieContainer = ckContainer;

            if(referer!="")
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

        private StringBuilder GetWebContent1(string url)
        {
            using (var web = new WebClient())
            {
                return new StringBuilder(web.DownloadString(url));
            }
        }

    }
}