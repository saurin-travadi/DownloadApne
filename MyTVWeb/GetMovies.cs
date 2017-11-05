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

            var className = context.Request.QueryString["source"] ?? "BadTameezDil";
            var type = Type.GetType("MyTVWeb." + className);
            MoviesBase objMovie = Activator.CreateInstance(type) as MoviesBase;

            if (string.IsNullOrEmpty(context.Request.QueryString["m"]))
            {
                objMovie.Search = context.Request.QueryString["search"];
                objMovie.PageNumber = context.Request.QueryString["page"];
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


    public class WatchOnlineMovies : MoviesBase
    {
        public WatchOnlineMovies()
        {
            RootURL = "http://www.watchonlinemovies.com.pk/";
        }


        public override List<Movie> GetMovies()
        {
            var _movies = new List<Movie>();
            var doc = new HtmlDocument();

            var url = RootURL;
            if (string.IsNullOrEmpty(PageNumber))
                PageNumber = "1";

            url = RootURL + "page/" + PageNumber + "/";

            if (!string.IsNullOrEmpty(Search))
                url = RootURL + "/?s=" + Search;

            doc.LoadHtml(new MyWebClient().GetData(url).ToString());
            var thumbs = doc.DocumentNode.SelectNodes("//div[@class='boxtitle']");
            if (thumbs != null)
            {
                thumbs.ToList().ForEach(e =>
                {
                    var aEle = e.SelectSingleNode("a");
                    _movies.Add(new Movie()
                    {
                        MovieName = aEle.Attributes["title"].Value,
                        MovieURL = aEle.Attributes["href"].Value,
                        MovieImage = aEle.SelectSingleNode("img").Attributes["src"].Value,
                        Success = true
                    });
                });
            }

            return _movies;
        }

        public override List<Movie> GetMovie(string url)
        {
            try
            {
                var helper = new MyWebClient();
                helper.GetData(this.RootURL);
                var src = helper.GetData(url, "http://www.watchonlinemovies.com.pk/", "", this.RootURL).ToString();

                var html = new HtmlAgilityPack.HtmlDocument();
                html.LoadHtml(src);
                var elements = html.DocumentNode.SelectNodes("//iframe");
                foreach (var e in elements)
                {
                    var m = GetM(e.Attributes["src"].Value);
                    if (m != null)
                        return new List<Movie>() { m };
                }

                return new List<Movie>() { new Movie() { MovieURL = "", CookieString = "" } };
            }
            catch
            {
                return new List<Movie>();
            }
        }

        private Movie GetM(string url)
        {
            var helper = new MyWebClient();
            if (!url.ToLower().Contains(".mp4"))
            {
                var src = helper.GetData(url).ToString();
                var html = new HtmlAgilityPack.HtmlDocument();
                html.LoadHtml(src);
                var elements = html.DocumentNode.SelectNodes("//script");
                foreach (var e in elements)
                {
                    if (e.InnerText.ToLower().Contains(".mp4"))
                    {
                        var regexSrc = new Regex("http.*mp4", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                        var m = regexSrc.Matches(e.InnerText);

                        var cookies = helper.ckContainer.GetCookies(new Uri(RootURL));
                        var cookieString = "";
                        for (int i = 0; i < cookies.Count; i++)
                        {
                            if (cookieString != "")
                                cookieString += "^";
                            cookieString += string.Format("{0}={1};expires={2};path={3}", cookies[0].Name, cookies[0].Value, cookies[0].Expires, cookies[0].Path);
                        }

                        return new Movie() { MovieURL = m[0].Value, CookieString = cookieString, Success = true };

                    }
                }
            }

            return null;
        }

    }

    public class Join4Films : MoviesBase
    {
        public Join4Films()
        {
            RootURL = "http://www.join4films.com/category/bollywood/";
        }

        public override List<Movie> GetMovies()
        {
            var _movies = new List<Movie>();
            var doc = new HtmlDocument();
            doc.LoadHtml(new MyWebClient().GetData(RootURL).ToString());
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

        public override List<Movie> GetMovie(string url)
        {
            try
            {
                var helper = new MyWebClient();
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

                return new List<Movie>() { new Movie() { MovieURL = finalText, CookieString = cookieString } };
            }
            catch
            {
                return new List<Movie>();
            }
        }
    }

    public class Bolly2Tolly : MoviesBase
    {
        public Bolly2Tolly()
        {
            RootURL = "http://www.bolly2tolly.com/search/label/Hindi%20Movies";
        }

        public override List<Movie> GetMovies()
        {
            var url = RootURL;
            if (string.IsNullOrEmpty(PageNumber))
                PageNumber = "1";

            url = RootURL + "page/" + PageNumber + "/";

            if (!string.IsNullOrEmpty(Search))
                url = RootURL + "/?s=" + Search;

            var _movies = new List<Movie>();
            var doc = new HtmlDocument();
            doc.LoadHtml(new MyWebClient().GetData(url).ToString());
            var thumbs = doc.DocumentNode.SelectNodes("//div[@class='item']");
            if (thumbs != null)
            {
                thumbs.ToList().ForEach(e =>
                {
                    var aEle = e.SelectSingleNode("a");
                    _movies.Add(new Movie()
                    {
                        MovieName = aEle.SelectSingleNode("div/img").Attributes["alt"].Value,
                        MovieURL = aEle.Attributes["href"].Value,
                        MovieImage = aEle.SelectSingleNode("div/img").Attributes["src"].Value,
                        Success = true
                    });
                });
            }


            return _movies;
        }

        public override List<Movie> GetMovie(string url)
        {
            try
            {
                var helper = new MyWebClient();
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

                return new List<Movie>() { new Movie() { MovieURL = finalText, CookieString = cookieString } };
            }
            catch
            {
                return new List<Movie>();
            }
        }

    }

    public class BadTameezDil : MoviesBase
    {
        public BadTameezDil()
        {
            base.RootURL = "http://badtameezdil.net/category/bollywood-movies-2017/";
        }

        public BadTameezDil(string show)
        {
            base.RootURL = "http://badtameezdil.net/author/" + show + "/";
        }

        public override List<Movie> GetMovies()
        {
            var _movies = new List<Movie>();
            var doc = new HtmlDocument();

            var url = RootURL;
            if (string.IsNullOrEmpty(PageNumber))
                PageNumber = "1";

            url = RootURL + "page/" + PageNumber + "/";

            if (!string.IsNullOrEmpty(Search))
                url = RootURL + "/?s=" + Search;

            doc.LoadHtml(new MyWebClient().GetData(url).ToString());
            var thumbs = doc.DocumentNode.SelectNodes("//*[contains(@class,'post-thumbnail')]");
            if (thumbs != null)
            {
                thumbs.ToList().ForEach(e =>
                {
                    var aEle = e.SelectSingleNode("a");
                    _movies.Add(new Movie()
                    {
                        MovieName = aEle.SelectSingleNode("img").Attributes["alt"].Value,
                        MovieURL = aEle.Attributes["href"].Value,
                        MovieImage = aEle.SelectSingleNode("img").Attributes["src"].Value,
                        Success = true
                    });
                });
            }

            return _movies;
        }

        public override List<Movie> GetMovie(string url)
        {
            try
            {
                var helper = new MyWebClient();
                var movies = new List<Movie>();

                var src = helper.GetData(url).ToString();

                var html = new HtmlAgilityPack.HtmlDocument();
                html.LoadHtml(src);
                var elements = html.DocumentNode.SelectNodes("//iframe");
                foreach (var e in elements)
                {
                    var vUrl = e.Attributes["src"].Value;
                    movies.Add(GetM(vUrl));
                }
                return movies;
            }
            catch
            {
                return null;
            }
        }

        private Movie GetM(string url)
        {
            var helper = new MyWebClient();
            var src = helper.GetData(url).ToString();
            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(src);
            var elements = html.DocumentNode.SelectSingleNode("//video/source");
            if (elements != null)
            {
                var mUrl = elements.Attributes["src"].Value;
                return new Movie() { Success = true, MovieURL = mUrl, IsVideoURL = 1, Embed = true };
            }
            else
                return new Movie() { Success = true, MovieURL = url, IsVideoURL = 0, Embed = true };

        }
    }

    public class OnlineMoviesCinema : MoviesBase
    {
        public OnlineMoviesCinema()
        {
            base.RootURL = "http://onlinemoviescinema.com/movies/";
        }

        public OnlineMoviesCinema(string url)
        {
            base.RootURL = url;
        }

        public override List<Movie> GetMovies()
        {
            var _movies = new List<Movie>();
            var doc = new HtmlDocument();

            var url = RootURL;
            if (!string.IsNullOrEmpty(PageNumber) && Convert.ToInt16(PageNumber) > 1)
                url = RootURL + "page/" + PageNumber + "/";

            if (!string.IsNullOrEmpty(Search))
            {
                url = url + "?s=" + Search;
                url = url.Replace("/movies", "");
            }

            doc.LoadHtml(new MyWebClient().GetData(url).ToString());
            var root = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'video-section')]");

            var thumbs = root.SelectNodes("//*[contains(@class,'item-img')]");      //for non-search path
            if (!string.IsNullOrEmpty(Search))
                thumbs = root.SelectNodes("div [contains(@class,'item')]");
            if (thumbs != null)
            {
                thumbs.ToList().ForEach(e =>
                {
                    var aEle = e.SelectSingleNode("a");
                    _movies.Add(new Movie()
                    {
                        MovieName = aEle.SelectSingleNode("img").Attributes["alt"].Value,
                        MovieURL = aEle.Attributes["href"].Value,
                        MovieImage = aEle.SelectSingleNode("img").Attributes["src"].Value,
                        Success = true
                    });
                });
            }

            return _movies;
        }

        public override List<Movie> GetMovie(string url)
        {
            try
            {
                var helper = new MyWebClient();
                var movies = new List<Movie>();

                var src = helper.GetData(url).ToString();

                var html = new HtmlAgilityPack.HtmlDocument();
                html.LoadHtml(src);
                var elements = html.DocumentNode.SelectNodes("//iframe");
                foreach (var e in elements)
                {
                    var vUrl = e.Attributes["src"].Value;
                    movies.Add(GetM(vUrl));
                }
                return movies;
            }
            catch
            {
                return null;
            }
        }

        private Movie GetM(string url)
        {
            var helper = new MyWebClient();
            var src = helper.GetData(url).ToString();
            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(src);
            var elements = html.DocumentNode.SelectSingleNode("//video/source");
            if (elements != null)
            {
                var mUrl = elements.Attributes["src"].Value;
                return new Movie() { Success = true, MovieURL = mUrl, IsVideoURL = 1, Embed = false };
            }
            else
                return new Movie() { Success = true, MovieURL = url, IsVideoURL = 0, Embed = false };

        }
    }

}