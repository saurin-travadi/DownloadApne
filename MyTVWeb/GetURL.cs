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
            var serial = context.Request.QueryString["s"];
            var day = context.Request.QueryString["d"];
            var url = context.Request.QueryString["url"];
            var format = context.Request.QueryString["f"];
            var source = context.Request.QueryString["source"];
            if (string.IsNullOrEmpty(source)) source = "Apne";

            var rootURL = source == "Apne" ? string.Format("{0}/{1}", Apne, serial) : url;
            var urls = ReadDatePage(rootURL, day, format);

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

        private List<string> ReadDatePage(string pageURL, string day, string format)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(GetWebContent(pageURL).ToString());
            var dates = doc.DocumentNode.SelectNodes("//*[contains(@class,'date_episodes')]");

            foreach (var date in dates)
            {
                var d = date.SelectSingleNode("span").InnerHtml;
                if (day.Equals(d))
                {
                    var videoPageURL = date.Attributes["href"].Value;
                    return ReadURL(videoPageURL, format.ToLower());
                }
            }

            return null;
        }

        private List<string> ReadURL(string videoPageURL, string format)
        {
            //Read MP4
            if (format == "video")
                return ReadDownloadLink(videoPageURL);

            if (format == "savebox")
                return ReadSaveBox(videoPageURL);

            IsDM = "true";
            if (format == "watchapne")
            {
                var returnURL = ReadSerialPage(videoPageURL, "watchapne");
                var tempURL = returnURL[0];
                returnURL.Clear();
                returnURL.Add(ReadWatchApnePage(tempURL));
                if (returnURL != null && returnURL.Count != 0)
                    return returnURL;
            }

            if (format == "telly")
            {
                var returnURL = ReadSerialPage(videoPageURL, "telly");
                var tempURL = returnURL[0];
                returnURL.Clear();
                returnURL.Add(ReadTellyPage(tempURL));
                if (returnURL != null && returnURL.Count != 0)
                    return returnURL;
            }

            return null;
        }

        private List<string> ReadDownloadLink(string url)
        {
            return ReadSerialPage(url, "download");
        }

        private string ReadTellyPage(string url)
        {
            GetWebContent(url);

            var strHTML = GetWebContent(url.Replace("http://apne.tv/redirector.php?r=", ""), url);

            Regex regex = new Regex(@".m3u8", RegexOptions.Multiline);
            var end = regex.Match(strHTML.ToString()).Index;

            regex = new Regex(@"file: ""http://", RegexOptions.Multiline);
            var start = regex.Match(strHTML.ToString()).Index;

            var finalText = strHTML.ToString().Substring(start, end - start);

            return finalText.Replace("file:", "").Replace("\"", "") + ".m3u8";
        }

        private string ReadWatchApnePage(string url)
        {
            GetWebContent(url);

            var strHTML = GetWebContent(url.Replace("http://apne.tv/redirector.php?r=", ""), url);

            Regex regex = new Regex(@"<iframe src='.*html'", RegexOptions.IgnoreCase);
            var text = regex.Match(strHTML.ToString()).Value;
            return text.ToLower().Replace("<iframe src=", "").Replace("'", "");
        }

        private List<string> ReadSaveBox(string url)
        {
            var urls = ReadSerialPage(url, "savebox");
            if (urls != null && urls.Count > 0)
            {
                if (IsMediaMP4(urls[0]))
                    return urls;

                else
                {
                    Thread thread = new Thread(delegate ()
                    {
                        using (WebBrowser browser = new WebBrowser())
                        {
                            browser.ScriptErrorsSuppressed = true;
                            browser.ScrollBarsEnabled = false;
                            browser.AllowNavigation = true;
                            browser.Navigate(urls[0]);
                            browser.Width = 1024;
                            browser.Height = 768;
                            //browser.DocumentCompleted += Browser_DocumentCompleted;
                            while (browser.ReadyState != WebBrowserReadyState.Complete)
                            {
                                System.Windows.Forms.Application.DoEvents();
                            }
                            var result = browser.Document.InvokeScript("window.xdmdhuA");
                            result = browser.Document.InvokeScript("x67ajvA");
                            result = browser.Document.InvokeScript("eval", new object[] { "x67ajvA" });

                        }
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();


                    //var doc = new HtmlAgilityPack.HtmlDocument();
                    //doc.LoadHtml(GetWebContent(urls[0]).ToString());

                    //var scripts = doc.DocumentNode.Descendants().Where(n => n.Name == "script");

                    //// Return the data of spect and stringify it into a proper JSON object
                    //var scriptText = "";
                    //scripts.ToList().ForEach(script =>
                    //{
                    //    scriptText += script.InnerHtml;
                    //});

                    //var engine = new Jurassic.ScriptEngine();
                    //var result = engine.Evaluate("(function() { " + scriptText + " if(x67ajvA!=null) return x67ajvA; })()");
                    //var json = JSONObject.Stringify(engine, result);

                }
            }

            return null;
        }

        private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var browser = sender as WebBrowser;
            var result = browser.Document.InvokeScript("eval", new object[] { "x67ajvA" });
            //var val = (sender as WebBrowser).Document.InvokeScript("x67ajvA");
        }

        private List<string> ReadSerialPage(string pageURL, string type)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(GetWebContent(pageURL).ToString());
            var shows = doc.DocumentNode.SelectNodes("//*[contains(@class,'channel_cont')]");

            var url = new List<string>();
            foreach (var show in shows)
            {
                if (show.SelectSingleNode("div/h2") != null)
                {
                    var text = show.SelectSingleNode("div/h2").InnerHtml;
                    if (text.ToLower().Contains(type))
                    {
                        url.Add(show.SelectSingleNode("ul/li/a").Attributes["href"].Value);
                    }
                }
            }

            return url;
        }

        CookieContainer ckContainer = null;
        private StringBuilder GetWebContent(string url, string referer = "")
        {
            HttpWebRequest req = WebRequest.CreateHttp(url);
            if (ckContainer != null)
                req.CookieContainer = ckContainer;

            if (referer != "")
                req.Referer = referer;

            HttpWebResponse res = req.GetResponse() as HttpWebResponse;
            if (ckContainer == null) ckContainer = new CookieContainer();
            ckContainer.Add(res.Cookies);

            using (Stream stream = res.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                return new StringBuilder(reader.ReadToEnd());
            }
        }


        private bool IsMediaMP4(string url)
        {
            HttpWebResponse response = null;
            var isMediaMP4 = true;

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                if (isMediaMP4)
                    isMediaMP4 = response.ContentType.ToLower().Contains("mp4");
            }
            catch
            {
                isMediaMP4 = false;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }

            return isMediaMP4;
        }

    }

}