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

namespace MyTVWeb
{
    public class GetURL : IHttpHandler
    {
        const string Apne = "http://apnetv.co/Hindi-Serial";
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
            urls = myObj.ReadDatePage(rootURL, day, format);
            IsDM = myObj.IsDM;

            context.Response.AddHeader("Content-Type", "application/json\n\n");
            context.Response.Buffer = true;

            var scriptFile = Path.Combine(context.Server.MapPath("."), "script.rjs");
            var data = File.ReadAllText(scriptFile);

            data = data.Replace("'%SHOWS%'", "'" + HttpUtility.UrlDecode(context.Request.QueryString["s"]) + "'");
            data = data.Replace("%CHANNELS%", "");
            data = data.Replace("%DATES%", "");
            data = data.Replace("%URL%", string.Join("|", urls));
            data = data.Replace("%URL1%", "");
            data = data.Replace("%ISDM%", IsDM);

            context.Response.Write(data);
            context.Response.Flush();
        }

    }

}