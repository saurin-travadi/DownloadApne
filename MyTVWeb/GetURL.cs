using Jurassic.Library;
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
        const string Apne = "http://apne.tv/Hindi-Serial";
        const string BollyStop = "http://bollystop.com";

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
            List<string> urls=new List<string>();

            var serial = context.Request.QueryString["s"];
            var day = context.Request.QueryString["d"];
            var url = context.Request.QueryString["url"];
            var format = context.Request.QueryString["f"];
            var show = context.Request.QueryString["source"];
            if (string.IsNullOrEmpty(show)) show = "Apne";

            BaseClass myObj = new GetApne();
            if (show.Equals("GetBollyStop"))
                myObj = new GetBollyStop();

            if (show.Equals("GetYoDesi"))
            {
                new GetYoDesi().ReadURL(url,format);
                IsDM = myObj.IsDM;
            }
            else
            {
                var rootURL = show == "GetApne" ? string.Format("{0}/{1}", Apne, serial) : url;
                urls = myObj.ReadDatePage(rootURL, day, format);
                IsDM = myObj.IsDM;
            }

            context.Response.AddHeader("Content-Type", "application/json\n\n");
            context.Response.Buffer = true;

            var scriptFile = Path.Combine(context.Server.MapPath("."), "script.rjs");
            var data = File.ReadAllText(scriptFile);

            data = data.Replace("'%SHOWS%'", "'" + HttpUtility.UrlDecode(context.Request.QueryString["s"]) + "'");
            data = data.Replace("%CHANNELS%", "");
            data = data.Replace("%DATES%", "");
            if (urls != null && urls.Count > 0)
            {
                data = data.Replace("%URL%", urls[0].Trim());
                if (urls.Count > 1)
                    data = data.Replace("%URL1%", urls[1].Trim());
                else
                    data = data.Replace("%URL1%", "");
            }
            else
            {
                data = data.Replace("%URL%", "");
                data = data.Replace("%URL1%", "");
            }

            data = data.Replace("%ISDM%", IsDM);

            context.Response.Write(data);
            context.Response.Flush();
        }

    }

}