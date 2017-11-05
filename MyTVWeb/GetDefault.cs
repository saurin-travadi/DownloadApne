using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace MyTVWeb
{
    public class GetDefault : IHttpHandler
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
            Stream st = context.Request.GetBufferedInputStream();
            if (st != null)            //POST
            {
                var s = new StreamReader(st).ReadToEnd();
            }
            

            context.Response.AddHeader("Content-Type", "application/json\n\n");
            context.Response.Buffer = true;
            context.Response.Write("");
            context.Response.Flush();
        }
    }

}