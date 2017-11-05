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

namespace MyTVWeb
{
    public class GetVideo : IHttpHandler
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
            List<Movie> data = null;
            var scriptFile = Path.Combine(context.Server.MapPath("."), "movie.rjs");
            var json = File.ReadAllText(scriptFile);

            var show = context.Request.QueryString["source"]??"sonytv";
            BadTameezDil objMovie = new BadTameezDil(show);
            if (string.IsNullOrEmpty(context.Request.QueryString["m"]))
            {
                objMovie.Search = context.Request.QueryString["search"];
                objMovie.PageNumber = context.Request.QueryString["page"];

                data = objMovie.GetMovies();
                json = json.Replace("'%MOVIE%'", new JavaScriptSerializer().Serialize(data));
                json = json.Replace("%COOKIE%", "");
            }
            else
            {
                var movie = objMovie.GetMovie(context.Request.QueryString["m"]);
                json = json.Replace("%MOVIE%", new JavaScriptSerializer().Serialize(data));
                json = json.Replace("%COOKIE%", "");
            }

            context.Response.AddHeader("Content-Type", "application/json\n\n");
            context.Response.Buffer = true;
            context.Response.Write(json);
            context.Response.Flush();
        }
    }

}