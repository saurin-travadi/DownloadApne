using Jurassic.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace MyTVWeb
{
    public class GetURLRSS : IHttpHandler
    {
        const string Apne = "http://apne.tv/Hindi-Serial";
        const string BollyStop = "http://bollystop.tv";
        const string DesiRulez = "http://desirulez.cc";

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
            List<string> urls = new List<string>();

            var serial = context.Request.QueryString["s"];
            var day = context.Request.QueryString["d"];
            if (string.IsNullOrEmpty(day))
                day = "";

            var show = context.Request.QueryString["source"];
            if (string.IsNullOrEmpty(show))
                show = "YoDesi";

            var format = context.Request.QueryString["f"];
            if (string.IsNullOrEmpty(format))
                format = "VKPrime";

            var page = context.Request.QueryString["page"];
            if (string.IsNullOrEmpty(page))
                page = "1";

            var url = context.Request.QueryString["url"];
            BaseClass myObj;
            if (url.StartsWith("stream"))
            {
                if (page == "1")
                {
                    urls.Add(url);
                }
                else
                {
                    urls.Add(url.Replace("stream_", ""));
                }
            }
            else
            {
                var type = Type.GetType("MyTVWeb." + show);
                myObj = Activator.CreateInstance(type) as BaseClass;
                myObj.show = serial;

                if (page == "1")
                    urls = myObj.ReadDatePage(url, day, format);
                else
                    urls = myObj.ReadURL(url, format, true);
            }


            context.Response.Clear();
            context.Response.AddHeader("Content-Type", "application/rss+xml");
            context.Response.Buffer = true;

            var xrFeed = new XmlTextWriter(context.Response.OutputStream, Encoding.UTF8);
            xrFeed.WriteStartDocument();
            xrFeed.WriteStartElement("rss");
            xrFeed.WriteAttributeString("version", "2.0");
            xrFeed.WriteStartElement("channel");
            xrFeed.WriteElementString("title", "Latest Items");
            xrFeed.WriteElementString("link", "http://www.mytvsite.azurewebsites.net");
            xrFeed.WriteElementString("description", "The latest items");

            var cnt = 1;
            urls.ToList().ForEach(e =>
            {
                xrFeed.WriteStartElement("part");
                xrFeed.WriteAttributeString("url", e);
                xrFeed.WriteAttributeString("title", "Part " + cnt++.ToString());
                xrFeed.WriteEndElement();
            });

            xrFeed.WriteEndElement();
            xrFeed.WriteEndDocument();
            xrFeed.Flush();
            xrFeed.Close();

            context.Response.End();
        }
    }

}