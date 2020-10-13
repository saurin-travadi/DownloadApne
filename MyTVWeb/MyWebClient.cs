using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace MyTVWeb
{
    public class MyWebClient
    {
        private string m_ResponseURL;
        private string m_QueryAndPath;

        public CookieContainer ckContainer = null;
        public String ResponseURL { get { return m_ResponseURL; } }
        public String QueryNPath { get { return m_QueryAndPath; } }

        public StringBuilder GetData(string url, string referer = "", string postData = "", string host = "")
        {
            HttpWebRequest req = WebRequest.CreateHttp(url);
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; rv:47.0) Gecko/20100101 Firefox/47.0";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.Headers.Add("Cookie", "lang=1; file_id=21322; aff=1; ref_url=" + url);
            req.Headers.Add("DNT", "1");
            req.Headers.Add("Upgrade-Insecure-Requests", "1");

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

            if (ckContainer == null) ckContainer = new CookieContainer();

            HttpWebResponse res = req.GetResponse() as HttpWebResponse;
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

        public bool IsMediaMP4(string url)
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