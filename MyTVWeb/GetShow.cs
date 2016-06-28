using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace MyTVWeb
{
    public class GetShow : IHttpHandler
    {
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            var show = context.Request.QueryString["s"];
            var objSerials = new List<Serial>();
            objSerials.Add(new GetApne().GetShows());

            var scriptFile = Path.Combine(context.Server.MapPath("."), "script.rjs");
            var data = File.ReadAllText(scriptFile);

            data = data.Replace("%CHANNELS%", "");
            data = data.Replace("'%SHOWS%'", new JavaScriptSerializer().Serialize(objSerials));
            data = data.Replace("%DATES%", "");
            data = data.Replace("%URL%", "");
            data = data.Replace("%ISDM%", "");

            context.Response.AddHeader("Content-Type", "application/json\n\n");
            context.Response.Buffer = true;
            context.Response.Write(data);
            context.Response.Flush();
        }
    }

    public class GetApne
    {
        public Serial GetShows()
        {
            //return GetShows("http://apne.tv/Hindi-Serials", "http://apne.tv");
            return GetShows("http://apne.tv", "http://apne.tv");
        }

        public Serial GetShows(string pageUrl, string referer)
        {
            var objSerial = new Serial();
            var serial = new Serial() { Source = "Apne" };

            var doc = new HtmlDocument();
            var str = GetWebContent(pageUrl, referer).ToString();
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

        CookieContainer ckContainer = null;
        private StringBuilder GetWebContent(string url, string referer = "")
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
}