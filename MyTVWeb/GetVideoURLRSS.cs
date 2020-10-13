using Jurassic.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Xml;

namespace MyTVWeb
{
    public class GetVideoURLRSS : IHttpHandler
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
            var serial = context.Request.QueryString["s"];
            var day = context.Request.QueryString["d"];
            if (day == "undefined" || string.IsNullOrEmpty(day)) day = "";

            var url = context.Request.QueryString["url"];
            var show = context.Request.QueryString["source"];
            if (string.IsNullOrEmpty(show)) show = "YoDesi";

            var format = context.Request.QueryString["f"];
            if (format == "undefined" || string.IsNullOrEmpty(format))
                format = "Dailymotion";


            var type = Type.GetType("MyTVWeb." + show);
            BaseClass myObj = Activator.CreateInstance(type) as BaseClass;
            myObj.show = serial;

            var rootURL = show == "GetApne" ? string.Format("{0}/{1}", Apne, serial) : url;
            var retrunUrl = myObj.ReadVideoURL(rootURL);
            IsDM = myObj.IsDM;

            context.Response.Clear();
            context.Response.AddHeader("Content-Type", "application/rss+xml");
            context.Response.Buffer = true;

            var xrFeed = new XmlTextWriter(context.Response.OutputStream, Encoding.UTF8);
            xrFeed.WriteStartDocument();
            xrFeed.WriteStartElement("rss");
            xrFeed.WriteAttributeString("version", "2.0");
            xrFeed.WriteStartElement("channel");
            xrFeed.WriteElementString("title", "Latest Items");
            xrFeed.WriteElementString("link", retrunUrl);
            xrFeed.WriteElementString("description", "The latest items");
            xrFeed.WriteEndElement();
            xrFeed.WriteEndDocument();
            xrFeed.Flush();
            xrFeed.Close();

            context.Response.End();
        }
    }
}