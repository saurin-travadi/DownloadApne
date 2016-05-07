using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MyTVWeb
{
    public class GetChannels : IHttpHandler
    {
        const string CHANNELS = "Sony TV|Star Plus|Zee TV|Sab TV|Life OK|Colors";
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.AddHeader("Content-Type", "application/json\n\n");
            context.Response.Buffer = true;

            var scriptFile = Path.Combine(context.Server.MapPath("."), "script.rjs");
            var data = File.ReadAllText(scriptFile);


            var channels = string.Join(",", CHANNELS.Split(new char[] { '|' }).ToList().ConvertAll(e => string.Format("'{0}'", e)));
            data = data.Replace("%SHOWS%", "");
            data = data.Replace("%CHANNELS%", channels);
            data = data.Replace("%URL%", "");

            context.Response.Write(data);
            context.Response.Flush();
        }
    }
}