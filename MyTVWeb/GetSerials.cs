using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MyTVWeb
{
    public class GetSerials : IHttpHandler
    {
        const string SHOWS = "Sony TV:Crime-Patrol|Sony TV:Sankat-Mochan-Mahabali-Hanumaan|Star Plus:Yeh-Rishta-Kya-Kehlata-Hai|Sony TV:C.I.D|Sony TV:Suryaputra-Karn|Star Plus:Siya-Ke-Ram|Colors:Comedy-Nights-With-Kapil|Life OK:Comedy-Classes|Life OK:Kalash|Sab TV:Taarak-Mehta-Ka-Ooltah|Zee TV:Jamai-Raja";
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            var tv = context.Request.QueryString["s"];

            context.Response.AddHeader("Content-Type", "application/json\n\n");
            context.Response.Buffer = true;

            var scriptFile = Path.Combine(context.Server.MapPath("."), "script.rjs");
            var data = File.ReadAllText(scriptFile);


            var shows = string.Join(",", SHOWS.Split(new char[] { '|' }).ToList()
                            .Where(w => w.Split(new char[] { ':' })[0].ToLower() == tv.ToLower()).ToList()
                            .ConvertAll(e => string.Format("'{0}'", e.Split(new char[] { ':' })[1]))
                            );

            data = data.Replace("%SHOWS%", shows);
            data = data.Replace("%CHANNELS%", "'" + tv + "'");
            data = data.Replace("%URL%", "");

            context.Response.Write(data);
            context.Response.Flush();
        }
    }
}