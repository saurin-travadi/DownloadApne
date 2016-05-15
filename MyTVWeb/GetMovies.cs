using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;

namespace MyTVWeb
{
    public class GetMovies : IHttpHandler
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

            if (string.IsNullOrEmpty(context.Request.QueryString["m"]))
            {
                data = new Join4Films().GetMovies();
                json = json.Replace("%MOVIE%", new JavaScriptSerializer().Serialize(data));
            }
            else {
                var url = new Join4Films().GerMovie(context.Request.QueryString["m"]);
                json = json.Replace("%MOVIE%", "'" + url + "'");
            }

            context.Response.AddHeader("Content-Type", "application/json\n\n");
            context.Response.Buffer = true;
            context.Response.Write(json);
            context.Response.Flush();
        }

    }

    public class Movie
    {
        public string MovieName { get; set; }
        public string MovieURL { get; set; }
    }

    public class Join4Films
    {
        public List<Movie> GetMovies()
        {
            var _movies = new List<Movie>();
            var url = "http://www.join4films.com/category/bollywood/";

            var doc = new HtmlDocument();
            doc.LoadHtml(Helper.GetWebContent(url).ToString());
            var thumbs = doc.DocumentNode.SelectNodes("//*[contains(@class,'postthumb')]");
            if (thumbs != null)
            {
                thumbs.ToList().ForEach(e =>
                {
                    var aEle = e.SelectSingleNode("a");
                    _movies.Add(new Movie()
                    {
                        MovieName = aEle.SelectSingleNode("img").Attributes["alt"].Value,
                        MovieURL = aEle.Attributes["href"].Value
                    });
                });
            }

            return _movies;
        }

        public string GerMovie(string url)
        {
            try
            {
                var _movies = new List<Movie>();

                var data = Helper.GetWebContent(url).ToString();
                var regex = new Regex(@"file: ""http://", RegexOptions.Multiline);
                var start = regex.Match(data.ToString()).Index;

                regex = new Regex(".mp4", RegexOptions.Multiline);
                var end = regex.Match(data.ToString()).Index;

                var finalText = data.ToString().Substring(start, end - start);
                finalText = finalText.Replace(@"file: """, "").Trim() + ".mp4";

                return finalText;
            }
            catch
            {
                return "";
            }
        }
    }

    public static class Helper
    {
        public static StringBuilder GetWebContent(string url)
        {
            using (var web = new WebClient())
            {
                return new StringBuilder(web.DownloadString(url));
            }
        }
    }
}