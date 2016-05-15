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
            var url = context.Request.QueryString["url"];

            if(string.IsNullOrEmpty(url))
                url = string.Format("http://apne.tv/Hindi-Serial/{0}", show);

            var dates = GetDates(url);

            var scriptFile = Path.Combine(context.Server.MapPath("."), "script.rjs");
            var data = File.ReadAllText(scriptFile);

            data = data.Replace("%SHOWS%", "'" + context.Request.QueryString["s"] + "'");
            data = data.Replace("%CHANNELS%", "");
            data = data.Replace("%DATES%", string.Join(",", dates.ToArray()));
            data = data.Replace("%URL%", "");
            data = data.Replace("%ISDM%", "");

            context.Response.AddHeader("Content-Type", "application/json\n\n");
            context.Response.Buffer = true;
            context.Response.Write(data);
            context.Response.Flush();
        }

        private List<string> GetDates(string pageUrl)
        {

            var doc = new HtmlDocument();
            doc.LoadHtml(GetWebContent(pageUrl).ToString());
            var dates = doc.DocumentNode.SelectNodes("//*[contains(@class,'date_episodes')]");      //apne
            if (dates != null)
                return dates.ToList().ConvertAll(e => string.Format("'{0}'", e.SelectSingleNode("span").InnerHtml));

            dates = doc.DocumentNode.SelectNodes("//*[contains(@class,'episode_date')]");      //bollystop
            if(dates!=null)
                return dates.ToList().ConvertAll(e => string.Format("'{0}'", e.SelectSingleNode("strong").InnerHtml));

            return null;

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