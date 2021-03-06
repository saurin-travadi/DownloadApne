﻿using HtmlAgilityPack;
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
    public class GetShow : IHttpHandler
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
            var show = context.Request.QueryString["source"] ?? "YoDesi";
            var type = Type.GetType("MyTVWeb." + show);
            BaseClass myObj = Activator.CreateInstance(type) as BaseClass;

            var data = "";
            var objSerials = new List<Serial>();
            var objShows = myObj.GetShows();
            objShows.Source = show;
            objSerials.Add(objShows);

            var scriptFile = Path.Combine(context.Server.MapPath("."), "script.rjs");
            data = File.ReadAllText(scriptFile);

            data = data.Replace("%CHANNELS%", "");
            data = data.Replace("'%SHOWS%'", new JavaScriptSerializer().Serialize(objSerials));
            data = data.Replace("%DATES%", "");
            data = data.Replace("%URL%", "");
            data = data.Replace("%URL1%", "");
            data = data.Replace("%ISDM%", "");

            context.Response.AddHeader("Content-Type", "application/json\n\n");
            context.Response.Buffer = true;
            context.Response.Write(data);
            context.Response.Flush();
        }
    }

}