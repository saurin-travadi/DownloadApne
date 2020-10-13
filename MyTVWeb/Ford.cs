using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MyTVWeb
{
    public class Ford : IHttpHandler
    {
        const string URL = "https://www.motorcraftservice.com/User/Login";
        private MyHttpWebClient m_MyWebClient;

        public Ford()
        {
            m_MyWebClient = new MyHttpWebClient();
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            //get country page
            var res = m_MyWebClient.GetData("https://www.motorcraftservice.com/Home/SetCountry");

            //post country selection
            res = m_MyWebClient.GetData("https://www.motorcraftservice.com/Home/SetCountry", "", "selectedCountry=153&selectedLanguage=EN-US");

            //get login page
            res = m_MyWebClient.GetData("https://www.motorcraftservice.com/User/Login", "https://www.motorcraftservice.com/Home/Index");

            //get capatch
            var captchaFound = false;
            var matchCollection = Regex.Matches(res.ToString(), "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase);
            foreach (Match c in matchCollection)
            {
                if (c.Value.ToLower().Contains("captcha"))
                {
                    var src = matchCollection[2].Groups[1].Value;
                    var arr = m_MyWebClient.GetImageData(string.Format("https://www.motorcraftservice.com{0}", src), "https://www.motorcraftservice.com/User/Login");
                    File.WriteAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img.png"), arr);
                    captchaFound = true;
                    break;
                }
            }

            //post login
            res=m_MyWebClient.GetData("https://www.motorcraftservice.com/User/Login", "https://www.motorcraftservice.com/User/Login", "UserName=mycdsteam&Password=nPr3l8yB5&CaptchaInputText=PPU&CaptchaDeText=f24d6bd96de24f3fb1787dfba93d03dc&__RequestVerificationToken=JModvKv3o0RKjuJy2tvwgMgWJX3aGxFcHV2ZOONR_vGE0Fu8I0TBw2vmv9BXjJbJrIorSfThxzHYNMzuA8UOJKOyZlE1");


        }
    }

    public class MyHttpWebClient
    {
        private string m_ResponseURL;
        private string m_QueryAndPath;

        private CookieContainer ckContainer = null;

        public StringBuilder GetData(string url, string referer = "", string postData = "", string host = "")
        {
            HttpWebRequest req = WebRequest.CreateHttp(url);
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; rv:47.0) Gecko/20100101 Firefox/47.0";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            if (host != "" && req.Host == "")
                req.Host = host;

            if (ckContainer != null)
                req.CookieContainer = ckContainer;

            if (referer != "")
                req.Referer = referer;

            if (postData != "")
            {
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                var bytes = ASCIIEncoding.ASCII.GetBytes(postData);
                req.ContentLength = bytes.Length;
                req.GetRequestStream().Write(bytes, 0, bytes.Length);
            }

            HttpWebResponse res = req.GetResponse() as HttpWebResponse;
            if (ckContainer == null) ckContainer = new CookieContainer();

            ckContainer.Add(res.Cookies);
            m_ResponseURL = res.ResponseUri.AbsoluteUri;
            m_QueryAndPath = res.ResponseUri.PathAndQuery;

            if (!string.IsNullOrEmpty(res.Headers["Transfer-Encoding"]) && res.Headers["Transfer-Encoding"] == "chunked")
            {
                StringBuilder sb = new StringBuilder();
                Byte[] buf = new byte[8192];
                Stream resStream = res.GetResponseStream();
                int count = 0;
                do
                {
                    count = resStream.Read(buf, 0, buf.Length);
                    if (count != 0)
                    {
                        sb.Append(Encoding.UTF8.GetString(buf, 0, count));
                    }
                } while (count > 0);
                return sb;
            }

            using (Stream stream = res.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                return new StringBuilder(reader.ReadToEnd());
            }
        }

        public byte[] GetImageData(string url, string referer = "")
        {
            HttpWebRequest req = WebRequest.CreateHttp(url);
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; rv:47.0) Gecko/20100101 Firefox/47.0";
            req.Accept = "image/webp,*/*";

            if (ckContainer != null)
                req.CookieContainer = ckContainer;

            if (referer != "")
                req.Referer = referer;

            HttpWebResponse res = req.GetResponse() as HttpWebResponse;
            if (ckContainer == null) ckContainer = new CookieContainer();

            ckContainer.Add(res.Cookies);
            m_ResponseURL = res.ResponseUri.AbsoluteUri;
            m_QueryAndPath = res.ResponseUri.PathAndQuery;

            MemoryStream stream = new MemoryStream();
            res.GetResponseStream().CopyTo(stream);
            byte[] data = stream.ToArray();

            return data;
        }
    }

}