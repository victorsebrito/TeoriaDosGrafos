using Grapevine;
using Grapevine.Server;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace TeoriaDosGrafos
{
    public sealed class APIResources : RESTResource
    {
        [RESTRoute (Method = HttpMethod.GET, PathInfo = "")]
        public void HandleAllGetRequests(HttpListenerContext context)
        {
            this.SendTextResponse(context, "GET is a success!");
        }

        [RESTRoute (Method = HttpMethod.POST, PathInfo = @"^/api/grafo/?$")]
        public void NewGraph(HttpListenerContext context)
        {
            Dictionary<string, string> loArgs = GetDictionaryFromContext(context);
            
            if (loArgs.ContainsKey("file"))
            {

            }
            else
            {
                MainClass.graph = new Graph();
            }
        }

        public Dictionary<string, string> GetDictionaryFromContext(HttpListenerContext context)
        {
            string body = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();
            NameValueCollection nvc = HttpUtility.ParseQueryString(body);
            return nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
        }
    }
}
