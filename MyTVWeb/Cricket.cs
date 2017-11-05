using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;

namespace MyTVWeb
{
    public class Cricket : IHttpHandler
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

            var className = context.Request.QueryString["source"] ?? "Highlights";
            var type = Type.GetType("MyTVWeb." + className);
            MoviesBase objMovie = Activator.CreateInstance(type) as MoviesBase;

            if (string.IsNullOrEmpty(context.Request.QueryString["m"]))
            {
                objMovie.Search = context.Request.QueryString["search"] ?? "";
                objMovie.PageNumber = context.Request.QueryString["page"] ?? "";
                data = objMovie.GetMovies();
            }
            else
            {
                data = objMovie.GetMovie(context.Request.QueryString["m"]);
            }
            json = json.Replace("'%MOVIE%'", new JavaScriptSerializer().Serialize(data));
            json = json.Replace("%COOKIE%", "");

            context.Response.AddHeader("Content-Type", "application/json\n\n");
            context.Response.Buffer = true;
            context.Response.Write(json);
            context.Response.Flush();

        }

    }

    public class Highlights : MoviesBase
    {
        public override List<Movie> GetMovies()
        {
            string url = "http://www.crickethighlights2.com/";

            var _movies = new List<Movie>();
            var doc = new HtmlDocument();
            doc.LoadHtml(new MyWebClient().GetData(url).ToString());
            var thumbs = doc.DocumentNode.SelectNodes("//article");
            if (thumbs != null)
            {
                thumbs.ToList().ForEach(e =>
                {
                    var aEle = e.SelectSingleNode("h2/a");
                    _movies.Add(new Movie()
                    {
                        MovieName = aEle.InnerText,
                        MovieURL = aEle.Attributes["href"].Value,
                        MovieImage = e.SelectSingleNode("figure/a/img").Attributes["src"].Value,
                        Success = true
                    });
                });
            }
            return _movies;
        }

        public override List<Movie> GetMovie(string url)
        {
            var movies = new List<Movie>();

            try
            {
                var helper = new MyWebClient();
                var src = helper.GetData(url).ToString();

                var html = new HtmlDocument();
                html.LoadHtml(src);
                var element = html.DocumentNode.SelectSingleNode("//ul[@id='tabgarb']");
                if (element != null)
                {
                    var elements = element.SelectNodes("li");
                    elements.ToList().ForEach(e =>
                    {
                        if (e.Attributes["class"] != null && e.Attributes["class"].Value == "tabgarbactive")
                        {
                            movies.Add(new Movie() { Success = true, MovieURL = html.DocumentNode.SelectSingleNode("//iframe").Attributes["src"].Value, IsVideoURL = 0, Embed = false });
                        }
                        else
                        {
                            movies.Add(GetM(e.SelectSingleNode("a").Attributes["href"].Value));
                        }
                    });
                }

                return movies;
            }
            catch
            {
                return new List<Movie>();
            }
        }

        private Movie GetM(string url)
        {
            try
            {
                var helper = new MyWebClient();
                var src = helper.GetData(url).ToString();

                var html = new HtmlDocument();
                html.LoadHtml(src);

                return new Movie() { Success = true, MovieURL = html.DocumentNode.SelectSingleNode("//iframe").Attributes["src"].Value, IsVideoURL = 0, Embed = false };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class WebCric : MoviesBase
    {
        public override List<Movie> GetMovies()
        {
            string url = "http://pc.webcric.com/";

            var _movies = new List<Movie>();
            var doc = new HtmlDocument();
            doc.LoadHtml(new MyWebClient().GetData(url).ToString());
            var thumbs = doc.DocumentNode.SelectNodes("//div[@class='card-block']");
            if (thumbs != null)
            {
                thumbs.ToList().ForEach(e =>
                {
                    var aEle = e.SelectSingleNode("h4/small");

                    var ele = aEle.SelectNodes("h3/a");
                    if(ele!=null)
                    {
                        ele.ToList().ForEach(ee =>
                        {
                            _movies.Add(new Movie()
                            {
                                MovieName = aEle.InnerText.Replace("Live Cricket Streaming", "").Trim(),
                                MovieURL = ee.Attributes["href"].Value,
                                Success = true
                            });
                        });
                    }
                });
            }
            return _movies;
        }

        public override List<Movie> GetMovie(string url)
        {
            var movies = new List<Movie>();

            try
            {
                var helper = new MyWebClient();
                var src = helper.GetData(url).ToString();

                var html = new HtmlDocument();
                html.LoadHtml(src);

                var element = html.DocumentNode.SelectSingleNode("//iframe").Attributes["src"].Value;
                if (element != null)
                {
                    movies.Add(new Movie() { IsVideoURL = 0, MovieURL = element, Embed = false });                    
                }

                return movies;
            }
            catch
            {
                return new List<Movie>();
            }
        }
    }
}