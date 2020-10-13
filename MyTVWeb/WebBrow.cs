using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace MyTVWeb
{
    public class WebBrow
    {
        public string URL { get; set; }
        public string DATA { get; set; }

        public void GetVideo()
        {
            var thread = new Thread(() => StartThread());
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private void StartThread()
        {
            var web = new System.Windows.Forms.WebBrowser();
            web.ScriptErrorsSuppressed = true;
            web.Url = new System.Uri(URL);

            while (web.ReadyState != WebBrowserReadyState.Complete)
                Application.DoEvents();

            web.Document.GetElementById("source").InnerText = DATA;
            web.Document.InvokeScript("beautify()");
            var a = web.Document.GetElementById("source").InnerText;

            var html = web.DocumentText;
        }

        private void Web_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            WebBrowser web = (WebBrowser)sender;
        }
    }
}
