using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace MyTVWeb
{
    public class GetDate : IHttpHandler
    {
        const string RootURL = "http://apne.tv/Hindi-Serial";

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            var dates = GetDates(context.Request.QueryString["s"]);

            context.Response.AddHeader("Content-Type", "application/json\n\n");
            context.Response.Buffer = true;

            var scriptFile = Path.Combine(context.Server.MapPath("."), "script.rjs");
            var data = File.ReadAllText(scriptFile);

            data = data.Replace("%SHOWS%", "'" + context.Request.QueryString["s"] + "'");
            data = data.Replace("%CHANNELS%", "");
            data = data.Replace("%DATES%", string.Join(",", dates.ToArray()));
            data = data.Replace("%URL%",  "" );
            data = data.Replace("%ISDM%",  "" );
            
            context.Response.Write(data);
            context.Response.Flush();
        }

        private List<string> GetDates(string serial)
        {
            var pageURL = string.Format("{0}/{1}", RootURL, serial);

            var doc = new HtmlDocument();
            doc.LoadHtml(GetWebContent(pageURL).ToString());
            var dates = doc.DocumentNode.SelectNodes("//*[contains(@class,'date_episodes')]");

            return dates.ToList().ConvertAll(e => string.Format("'{0}'", e.SelectSingleNode("span").InnerHtml));

            //foreach (var date in dates)
            //{
            //    var d = date.SelectSingleNode("span").InnerHtml;
            //}

            //return "";
        }

        private StringBuilder GetWebContent(string url)
        {
            using (var web = new WebClient())
            {
                return new StringBuilder(web.DownloadString(url));
            }
        }

    }
}