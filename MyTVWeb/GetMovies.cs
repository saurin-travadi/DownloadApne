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
                data = new Bolly2Tolly().GetMovies();
                json = json.Replace("'%MOVIE%'", new JavaScriptSerializer().Serialize(data));
                json = json.Replace("%COOKIE%", "");
            }
            else {
                var movie = new Join4Films().GerMovie(context.Request.QueryString["m"]);
                json = json.Replace("%MOVIE%", movie.MovieURL);
                json = json.Replace("%COOKIE%", movie.CookieString);
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
        public string CookieString { get; set; }
    }

    public class Join4Films
    {
        private string RootURL
        {
            get
            {
                return "http://www.join4films.com/category/bollywood/";
            }
        }

        public List<Movie> GetMovies()
        {
            var _movies = new List<Movie>();
            var doc = new HtmlDocument();
            doc.LoadHtml(new Common().GetData(RootURL).ToString());
            var thumbs = doc.DocumentNode.SelectNodes("//*[contains(@class,'postthumb')]");
            if (thumbs != null)
            {
                thumbs.ToList().ForEach(e =>
                {
                    var aEle = e.SelectSingleNode("a");
                    _movies.Add(new Movie()
                    {
                        MovieName = aEle.SelectSingleNode("img").Attributes["alt"].Value,
                        MovieURL = aEle.Attributes["href"].Value,
                    });
                });
            }

            return _movies;
        }

        public Movie GerMovie(string url)
        {
            try
            {
                var helper = new Common();
                helper.GetData(RootURL);

                var data = helper.GetData(url).ToString();
                var regex = new Regex(@"file: ""http://", RegexOptions.Multiline);
                var start = regex.Match(data.ToString()).Index;

                regex = new Regex(".mp4", RegexOptions.Multiline);
                var end = regex.Match(data.ToString()).Index;

                var finalText = data.ToString().Substring(start, end - start);
                finalText = finalText.Replace(@"file: """, "").Trim() + ".mp4";

                var cookies = helper.ckContainer.GetCookies(new Uri(RootURL));
                var cookieString = "";
                for (int i = 0; i < cookies.Count; i++)
                {
                    if (cookieString != "")
                        cookieString += "^";
                    cookieString += string.Format("{0}={1};expires={2};path={3}", cookies[0].Name, cookies[0].Value, cookies[0].Expires, cookies[0].Path);
                }

                return new Movie() { MovieURL = finalText, CookieString = cookieString };
            }
            catch
            {
                return null;
            }
        }
    }

    public class Bolly2Tolly
    {
        private string RootURL
        {
            get
            {
                return "http://www.bolly2tolly.com/search/label/Hindi%20Movies";
            }
        }

        public List<Movie> GetMovies()
        {
            var _movies = new List<Movie>();
            var doc = new HtmlDocument();
            while (true)
            {
                doc.LoadHtml(new Common().GetData(RootURL).ToString());
                var thumbs = doc.DocumentNode.SelectNodes("//*[contains(@class,'post-body')]");
                if (thumbs != null)
                {
                    
                    thumbs.ToList().ForEach(e =>
                    {
                        var iframe = e.SelectSingleNode("following::iframe");
                        var name = e.SelectSingleNode("following::h2");
                        if (!e.SelectSingleNode("following::h2").InnerText.ToLower().Contains("quick links"))
                        {
                            _movies.Add(new Movie()
                            {
                                MovieName = name.InnerText,
                                MovieURL = iframe.Attributes["src"].Value,
                            });
                        }
                    });
                }

                break;
            }

            return _movies;
        }

        public Movie GerMovie(string url)
        {
            try
            {
                var helper = new Common();
                helper.GetData(RootURL);

                var data = helper.GetData(url).ToString();
                var regex = new Regex(@"file: ""http://", RegexOptions.Multiline);
                var start = regex.Match(data.ToString()).Index;

                regex = new Regex(".mp4", RegexOptions.Multiline);
                var end = regex.Match(data.ToString()).Index;

                var finalText = data.ToString().Substring(start, end - start);
                finalText = finalText.Replace(@"file: """, "").Trim() + ".mp4";

                var cookies = helper.ckContainer.GetCookies(new Uri(RootURL));
                var cookieString = "";
                for (int i = 0; i < cookies.Count; i++)
                {
                    if (cookieString != "")
                        cookieString += "^";
                    cookieString += string.Format("{0}={1};expires={2};path={3}", cookies[0].Name, cookies[0].Value, cookies[0].Expires, cookies[0].Path);
                }

                return new Movie() { MovieURL = finalText, CookieString = cookieString };
            }
            catch
            {
                return null;
            }
        }

    }

}