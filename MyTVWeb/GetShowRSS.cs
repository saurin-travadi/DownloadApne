using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;

namespace MyTVWeb
{
    public class GetShowRSS : IHttpHandler
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
            var show = context.Request.QueryString["source"] ?? "YoDesi";
            var type = Type.GetType("MyTVWeb." + show);
            BaseClass myObj = Activator.CreateInstance(type) as BaseClass;

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

            Serial objShows = new Serial();     // = myObj.GetShows();
            AddURLs(objShows);

            objShows.Channel.ForEach(delegate (Channel ch)
            {
                xrFeed.WriteStartElement("item");
                xrFeed.WriteElementString("title", ch.Name);
                xrFeed.WriteElementString("description", "");
                xrFeed.WriteElementString("category", "");
                xrFeed.WriteElementString("author", "");
                xrFeed.WriteElementString("link", ch.ChannelURL);
                xrFeed.WriteElementString("guid", Guid.NewGuid().ToString());
                xrFeed.WriteElementString("pubDate", System.DateTime.Now.ToString("R"));
                xrFeed.WriteElementString("thumbnail", ch.ImageURL);
                ch.Shows.ForEach(delegate (Show sh)
                {
                    xrFeed.WriteStartElement("show");
                    xrFeed.WriteAttributeString("name", sh.Name);
                    xrFeed.WriteAttributeString("url", sh.URL);
                    //xrFeed.WriteElementString("name", sh.Name);
                    //xrFeed.WriteElementString("url", sh.URL);
                    xrFeed.WriteEndElement();
                });

                xrFeed.WriteEndElement();
            });

            xrFeed.WriteEndElement();
            xrFeed.WriteEndDocument();
            xrFeed.Flush();
            xrFeed.Close();

            context.Response.End();
        }

        private void AddURLs(Serial objShows)
        {
            var files = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory)).GetFiles("url*.txt");
            files.ToList().ForEach(file =>
            {
                var shows = new List<Show>();
                var txt = File.ReadAllLines(file.FullName);
                for (int i = 0; i < txt.Length; i = i + 2)
                {
                    shows.Add(new Show() { Name = txt[i].Split(',').Reverse().First(), URL = "stream_" + txt[i + 1] });
                }
                if (objShows.Channel == null)
                    objShows.Channel = new List<Channel>();

                objShows.Channel.Insert(0, new Channel() { Name = file.Name, ChannelURL = "stream", ImageURL = "noimage", Shows = shows });
            });
        }
    }
}