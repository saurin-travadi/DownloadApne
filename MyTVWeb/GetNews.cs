using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;

namespace MyTVWeb
{
    public class GetNews : IHttpHandler
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

            var className = context.Request.QueryString["source"] ?? "NDTV_24x7";
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

    public class Live : MoviesBase
    {
        MyWebClient webClient;
        public Live()
        {
            webClient = new MyWebClient();
        }

        public override List<Movie> GetMovies()
        {
            var _movies = new List<Movie>();
            _movies.Add(new Movie() { MovieName = "NDTV_24x7", MovieURL = GetNDTV_24x7() });
            _movies.Add(new Movie() { MovieName = "NDTV_India", MovieURL = "http://ndtvstream-lh.akamaihd.net/i/ndtv_india_1@300634/master.m3u8" });
            _movies.Add(new Movie() { MovieName = "Zee_News", MovieURL = GetZee_News() });
            _movies.Add(new Movie() { MovieName = "Times_NOW", MovieURL = "" });
            _movies.Add(new Movie() { MovieName = "India_TV", MovieURL = "http://indiatvnews-lh.akamaihd.net/i/ITV_1@199237/master.m3u8" });
            _movies.Add(new Movie() { MovieName = "Aaj_Tak", MovieURL = "https://mobiletak-vh.akamaihd.net/i/mobiletv/video/2018_05/vod_26_may_18_pmmodionsilverspoon_2048_,164,410,996,.mp4.csmil/master.m3u8" });
            //_movies.Add(new Movie() { MovieName = "Aaj_Tak_TEZ", MovieURL = "https://livestream.com/accounts/13995833/events/4198957/player?width=560&height=315&autoPlay=true&mute=false" });
            _movies.Add(new Movie() { MovieName = "Republic", MovieURL = "https://republicalive1-a.akamaihd.net/d78729de527e4b078b546385b060dc9b/ap-southeast-1/5384493731001/profile_2/chunklist.m3u8?hdnea=st=1526470012~exp=9007200781211003~acl=/d78729de527e4b078b546385b060dc9b/*/profile_2/chunklist.m3u8*~hmac=55a003108cba261962b84db2ef5173d1b0b700c4e82f36e6663ade4d9532b3bc" });

            return _movies;
        }

        private string GetZee_News()
        {
            try
            {
                //var doc = new HtmlDocument();
                //doc.LoadHtml(webClient.GetData("http://zeenews.india.com/live-tv").ToString());
                //var frame = doc.DocumentNode.SelectSingleNode("//iframe[@id='frmYT']").Attributes["src"].Value;

                var html = webClient.GetData("http://zeenews.dittotv.com/?header=no").ToString();
                var regex = new Regex(@"file:.*,", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                var obj = regex.Match(html.ToString()).Value;
                obj = obj.Replace("file:", "");
                obj = obj.Replace("'", "");
                obj = obj.Replace(",", "");
                return obj.Trim();
            }
            catch
            {
                return "http://dittotvnews.live-s.cdn.bitgravity.com/cdn-live/_definst_/dittotvnews/secure/zee_news_iwpl.smil/playlist.m3u8?e=1525740123&h=a0301ecd2d573852b7968e4e7d64a84e";
            }
        }

        private string GetNDTV_24x7()
        {
            try
            {
                var html = webClient.GetData("https://www.ndtv.com/video/live/channel/ndtv24x7").ToString();

                var regex = new Regex("html5playerdata.*.m3u8", RegexOptions.Singleline);
                html = regex.Match(html).Value;
                html = html + "\"}";
                html = html.Replace("html5playerdata=", "");

                dynamic obj = JsonConvert.DeserializeObject(html);
                var media = obj.media.Value;

                return media.Trim();
            }
            catch
            {
                return "https://ndtvstream-lh.akamaihd.net/i/ndtv_24x7_1@300633/master.m3u8";
            }
        }
    }

    public class NDTV_24x7 : MoviesBase
    {
        public NDTV_24x7()
        {
            base.RootURL = "https://www.ndtv.com/video/list/channel/ndtv-24x7";
        }

        public override List<Movie> GetMovies()
        {
            var _movies = new List<Movie>();
            var doc = new HtmlDocument();

            if (PageNumber == null)
                PageNumber = "1";

            var url = string.Format("{0}/page/{1}", RootURL, PageNumber);
            doc.LoadHtml(new MyWebClient().GetData(url).ToString());
            var thumbs = doc.DocumentNode.SelectNodes("//*[contains(@class,'thumbnail')]");
            if (thumbs != null)
            {
                thumbs.ToList().ForEach(e =>
                {
                    _movies.Add(new Movie()
                    {
                        MovieName = e.SelectSingleNode("a/picture/source/img").Attributes["title"].Value,
                        MovieURL = e.SelectSingleNode("a").Attributes["href"].Value,
                        MovieImage = e.SelectSingleNode("a/picture/source/img").Attributes["src"].Value,
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
                var regex = new Regex(@"media.*m3u8", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                var obj = regex.Match(src.ToString()).Value;
                obj = obj.Replace(@"\/\/", "//");
                obj = obj.Replace("\\", "");
                obj = obj.Replace("\"", "'");
                obj = obj.Replace("media':'", "");
                movies.Add(new Movie() { Success = true, MovieURL = obj, IsVideoURL = 1, Embed = true });

                return movies;
            }
            catch
            {
                return null;
            }
        }
    }

    public class NDTV_India : MoviesBase
    {
        public string Source { get; set; }
        public NDTV_India()
        {
            base.RootURL = "https://www.ndtv.com/video/list/channel/ndtv-india";
        }

        public override List<Movie> GetMovies()
        {
            var _movies = new List<Movie>();
            var doc = new HtmlDocument();

            if (PageNumber == null)
                PageNumber = "1";

            var url = string.Format("{0}/page/{1}", RootURL, PageNumber);
            doc.LoadHtml(new MyWebClient().GetData(url).ToString());
            var thumbs = doc.DocumentNode.SelectNodes("//*[contains(@class,'thumbnail')]");
            if (thumbs != null)
            {
                thumbs.ToList().ForEach(e =>
                {
                    _movies.Add(new Movie()
                    {
                        MovieName = e.SelectSingleNode("a/picture/source/img").Attributes["title"].Value,
                        MovieURL = e.SelectSingleNode("a").Attributes["href"].Value,
                        MovieImage = e.SelectSingleNode("a/picture/source/img").Attributes["src"].Value,
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
                var regex = new Regex(@"media.*m3u8", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                var obj = regex.Match(src.ToString()).Value;
                obj = obj.Replace(@"\/\/", "//");
                obj = obj.Replace("\\", "");
                obj = obj.Replace("\"", "'");
                obj = obj.Replace("media':'", "");
                movies.Add(new Movie() { Success = true, MovieURL = obj, IsVideoURL = 1, Embed = true });

                return movies;
            }
            catch
            {
                return null;
            }
        }
    }

    public class Zee_News : MoviesBase
    {
        public Zee_News()
        {
            this.RootURL = "http://zeenews.india.com";
        }

        public override List<Movie> GetMovies()
        {
            var _movies = new List<Movie>();
            var doc = new HtmlDocument();

            if (PageNumber == null)
                PageNumber = "1";

            doc.LoadHtml(new MyWebClient().GetData(RootURL + "/video/page/" + PageNumber).ToString());
            var thumbs = doc.DocumentNode.SelectNodes("//*[contains(@class,'i-list')]");
            if (thumbs != null)
            {
                thumbs.ToList().ForEach(e =>
                {
                    _movies.Add(new Movie()
                    {
                        MovieName = (e.SelectSingleNode("div/a/h3")).InnerHtml,
                        MovieURL = e.SelectSingleNode("div/a").Attributes["href"].Value,
                        MovieImage = e.SelectSingleNode("div/a/div/img").Attributes["src"].Value,
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

                var src = helper.GetData(this.RootURL + url.Trim()).ToString();

                var regex = new Regex(@"embedUrl.*m3u8", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                var obj = regex.Match(src.ToString()).Value;
                obj = obj.Replace("\"", "");
                obj = obj.Replace("embedUrl:", "").Trim();

                movies.Add(new Movie() { Success = true, MovieURL = obj, IsVideoURL = 1, Embed = true });

                return movies;
            }
            catch
            {
                return null;
            }
        }
    }

    public class Times_NOW : MoviesBase
    {
        public Times_NOW()
        {
            this.RootURL = "http://api-test.timesnow.tv/services/";
        }

        public override List<Movie> GetMovies()
        {
            var movies = new List<Movie>();
            var sources = new string[] {
                //"http://api-test.timesnow.tv/services/latestStories.php?type=1&subType=1&catId=0&storyId=0",
                //"http://api-test.timesnow.tv/services/popularStories.php?type=1&subType=1",
                "http://api-test.timesnow.tv/services/trendingStorieslive.php?storyId=0&subType=3&type=1",
                "http://api-test.timesnow.tv/services/mustWatch.php",
                "http://api-test.timesnow.tv/services/debates.php"
                //,"http://api-test.timesnow.tv/services/networkController.php?action=latest"
            };

            string json = "";
            sources.ToList().ForEach(e =>
            {
                try
                {
                    json = new MyWebClient().GetData(e).ToString();
                }
                finally
                {
                    if (json != "")
                    {
                        dynamic obj = JsonConvert.DeserializeObject(json);
                        if (obj.response.latestStories != null)
                            obj = obj.response.latestStories;
                        else if (obj.response.debates != null)              //works
                            obj = obj.response.debates;
                        else if (obj.response.popularDebates != null)
                            obj = obj.response.popularDebates;
                        else if (obj.response.trendingStories != null)      //works
                            obj = obj.response.trendingStories;
                        else if (obj.response.popularStories != null)
                            obj = obj.response.popularStories;

                        foreach (dynamic objItem in obj)
                        {
                            movies.Add(new Movie() { MovieName = objItem.title, MovieImage = objItem.thumbnailImage, MovieURL = objItem.shortUrl });
                        }
                        json = "";
                    }
                }
            });

            return movies;
        }

        public override List<Movie> GetMovie(string url)
        {
            try
            {
                var helper = new MyWebClient();
                var movies = new List<Movie>();

                string vidURL = "";
                var str = helper.GetData(url).ToString();
                var vid = helper.QueryNPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (vid[1] == "debate")
                {
                    str = helper.GetData(string.Format("http://api-test.timesnow.tv/services/debatesConsumption.php?debateId={0}", vid[3])).ToString();
                    dynamic obj = JsonConvert.DeserializeObject(str);
                    if (obj.response.debate != null)
                        vidURL = obj.response.debate.videoUrl;
                }
                else if (vid[1] == "video")
                {
                    str = helper.GetData(string.Format("http://api-test.timesnow.tv/services/consumption.php?id={0}&pf=1", vid[3])).ToString();
                    dynamic obj = JsonConvert.DeserializeObject(str);
                    if (obj[0].response.articleDetails != null)
                        vidURL = obj[0].response.articleDetails.videoUrl;
                }


                movies.Add(new Movie() { Success = true, MovieURL = vidURL, IsVideoURL = 1, Embed = true });

                return movies;
            }
            catch
            {
                return null;
            }
        }

    }

    public class India_TV : MoviesBase
    {
        public India_TV()
        {
            base.RootURL = "http://www.indiatvnews.com/video";
        }

        public override List<Movie> GetMovies()
        {
            var _movies = new List<Movie>();
            var doc = new HtmlDocument();

            doc.LoadHtml(new MyWebClient().GetData(RootURL).ToString());
            var thumbs = doc.DocumentNode.SelectNodes("//*[contains(@class,'thumb')]");
            if (thumbs != null)
            {
                thumbs.ToList().ForEach(e =>
                {
                    if (e.SelectSingleNode("img") != null && !_movies.Exists(x => x.MovieName == e.SelectSingleNode("img").Attributes["alt"].Value))
                    {
                        _movies.Add(new Movie()
                        {
                            MovieName = e.SelectSingleNode("img").Attributes["alt"].Value,
                            MovieURL = e.Attributes["href"].Value,
                            MovieImage = e.SelectSingleNode("img").Attributes["src"].Value,
                            Success = true
                        });
                    }
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
                var regex = new Regex(@"videoId.push.* loadPlayer", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                var obj = regex.Match(src.ToString()).Value;
                obj = obj.Replace("videoId.push(", "");
                obj = obj.Replace("VIDGYOR.loadPlayer", "");
                obj = obj.Replace(");", "");
                obj = obj.Replace("\"", "").Trim();

                url = "http://indiatv-vh.akamaihd.net/i/vod/" + obj + "_,22,21,20,.mp4.csmil/master.m3u8";

                movies.Add(new Movie() { Success = true, MovieURL = url, IsVideoURL = 1, Embed = true });

                return movies;
            }
            catch
            {
                return null;
            }
        }
    }


    public class Aaj_Tak : MoviesBase
    {
        public override List<Movie> GetMovies()
        {
            return new List<Movie>();
        }
    }





}