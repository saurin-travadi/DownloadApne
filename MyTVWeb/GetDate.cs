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
            var url = context.Request.QueryString["url"];
            var show = context.Request.QueryString["source"] ?? "GetDesiRulez";
            if (string.IsNullOrEmpty(url)) url = string.Format("http://apne.tv/Hindi-Serial/{0}", show);

            var type = Type.GetType("MyTVWeb." + show);
            BaseClass myObj = Activator.CreateInstance(type) as BaseClass;

            myObj.show = context.Request.QueryString["s"];
            var dates = myObj.GetDates(url);
            var scriptFile = Path.Combine(context.Server.MapPath("."), "script.rjs");
            var data = File.ReadAllText(scriptFile);

            data = data.Replace("'%SHOWS%'", "'" + context.Request.QueryString["s"] + "'");
            data = data.Replace("%CHANNELS%", "");
            data = data.Replace("%DATES%", string.Join(",", dates.ToArray()));
            data = data.Replace("%URL%", "");
            data = data.Replace("%ISDM%", "");

            context.Response.AddHeader("Content-Type", "application/json\n\n");
            context.Response.Buffer = true;
            context.Response.Write(data);
            context.Response.Flush();
        }



    }
}