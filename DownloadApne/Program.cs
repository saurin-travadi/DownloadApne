using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Threading;

namespace DownloadApne
{
    public class Program
    {
        const string DownloadFolder = @"C:\Users\saurin\Videos";
        const string SerialsName = "Crime-Patrol|Sankat-Mochan-Mahabali-Hanumaan|Yeh-Rishta-Kya-Kehlata-Hai";

        System.Windows.Forms.Form frm = null;
        System.Windows.Forms.ListBox lstBox = null;
        Boolean IsRunning = false;

        public static void Main(string[] arg)
        {
            var prg = new Program();
            prg.frm = new System.Windows.Forms.Form();


            prg.frm.Activated += prg.frm_Activated;
            prg.frm.ShowDialog();
        }

        void frm_Activated(object sender, EventArgs e)
        {
            lstBox = new System.Windows.Forms.ListBox();
            lstBox.Dock = System.Windows.Forms.DockStyle.Fill;
            frm.Controls.Add(lstBox);

            IsRunning = true;
            SerialsName.Split(new char[] { '|' }).ToList().ForEach(delegate(string s)
            {
                lstBox.Items.Insert(0, "Reading " + s);
                try
                {
                    ReadPage("http://apne.tv/Hindi-Serial", s);
                }
                catch (Exception ex)
                {
                    lstBox.Items.Insert(0, "Error " + ex.Message);
                } 
            });
            IsRunning = false;
        }

        public void ReadPage(string rootURL, string serial)
        {
            var pageURL = string.Format("{0}/{1}", rootURL, serial);
            var doc = new HtmlDocument();
            doc.LoadHtml(GetWebContent(pageURL).ToString());
            var dates = doc.DocumentNode.SelectNodes("//*[contains(@class,'date_episodes')]");

            
            lstBox.Items.Insert(0, "Found " + dates.Count.ToString() + " dates for " + serial);

            foreach (var date in dates)
            {
                var d = date.SelectSingleNode("span").InnerHtml;

                lstBox.Items.Insert(0, "Reading date " + d + " for " + serial);
                var folder = GetDownloadFolderFullPath(d);
                
                var localFile = Path.Combine(folder, string.Format("{0}", serial));

                System.Windows.Forms.Application.DoEvents();

                if (!IsFileDownloaded(localFile))
                {
                    lstBox.Items.Insert(0, "Reading page for " + d + " for " + serial);

                    //Download file
                    var videoURL = date.Attributes["href"].Value;
                    Console.WriteLine(videoURL);

                    var videoFile = ReadSerialPage(videoURL);
                    DownloadFile(videoFile, localFile);
                }
            }

            System.Windows.Forms.Application.Exit();
        }

        private string ReadSerialPage(string pageURL)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(GetWebContent(pageURL).ToString());
            var shows = doc.DocumentNode.SelectNodes("//*[contains(@class,'channel_cont')]");

            foreach (var show in shows)
            {
                var text = show.SelectSingleNode("div/h2").InnerHtml;
                if (text.ToLower().Contains("download") && text.ToLower().Contains("single"))
                {
                    return show.SelectSingleNode("ul/li/a").Attributes["href"].Value;
                }
            }

            return "";
        }

        private StringBuilder GetWebContent(string url)
        {
            using (var web = new WebClient())
            {
                return new StringBuilder(web.DownloadString(url));
            }
        }

        private string GetDownloadFolderFullPath(string folder)
        {
            var path = Path.Combine(DownloadFolder, folder);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        private bool IsFileDownloaded(string file)
        {
            return File.Exists(file+".mp4");
        }

        List<VideoStream> videoList = new List<VideoStream>();
        private void DownloadFile(string url, string localFile)
        {
            lstBox.Items.Insert(0, "Downloading " + localFile + ".mp4");

            var web = new WebClient();
            //web.DownloadFile(url, localFile + ".mp4");

            //MultiPartDownload(url, localFile);
        }


        private void MultiPartDownload(string url, string localFile)
        {
            WebRequest request = WebRequest.Create(new Uri(url));
            request.Method = "HEAD";
            var response = request.GetResponse();
            var responseLength = int.Parse(response.Headers.Get("Content-Length"));

            long segment = (long)Math.Ceiling((double)response.ContentLength / 10);
            for (long i = 0; i < response.ContentLength; i = i + segment)
            {
                videoList.Add(new VideoStream() { VideoURL = url, Start = i, Length = segment, LocalFile = localFile + "_" + i.ToString() });
            }
            
            List<Thread> threadList = new List<Thread>();
            foreach(VideoStream v in videoList)
            {
                Thread aThread = new Thread(v.ReadStream); //this method will be called when the thread starts
                threadList.Add(aThread);
                aThread.Start();
            }

            foreach(Thread aThread in threadList)
            {
                aThread.Join();//will wait until the thread is finished
            }
            
            //Join file 
        }
    }

    public class VideoStream
    {
        public string VideoURL { get; set; }
        public long Start { get; set; }
        public long Length { get; set; }
        public string LocalFile { get; set; }

        public void ReadStream()
        {
            Console.WriteLine(string.Format("URL = {0}, Start= {1}", VideoURL, Start));

            var request = WebRequest.Create(VideoURL);
            request.Method = "GET";
            (request as HttpWebRequest).AddRange(Start, Start + Length);
            
            var response = request.GetResponse().GetResponseStream();
            FileStream fs = new FileStream(LocalFile+".mp4", FileMode.Create);
            response.CopyTo(fs);
            fs.Close();
            fs.Dispose();
        }
    }
}
