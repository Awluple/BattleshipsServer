using System;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace BattleshipsServer
{

    public struct Path: IEquatable<Path> {
    public string path;
    public string httpMethod;

    public Path(string path, string method) {
        this.path = path;
        this.httpMethod = method;
    }

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Path && Equals((Path)obj);
    }

    public bool Equals(Path other) {
        return this.path == other.path && this.httpMethod == other.httpMethod;
    }

    public override int GetHashCode() {
        return (this.path + this.httpMethod).GetHashCode();
    }
    }
    /// <summary>Holds Http communication functions and sends http messages</summary>
    /// <param name="server">Server object to attach events</param>
    partial class HttpHandlers
    {
        private Server server;
        private Dictionary<Path, Action<object, RequestProcessorEventArgs>> routes = new Dictionary<Path, Action<object, RequestProcessorEventArgs>>();
        private delegate void Route(object sender, RequestProcessorEventArgs  e);
        
        public HttpHandlers(Server httpServer) {
            server = httpServer;
            this.Start();
        }

        public void Start() {
            routes.Add(new Path("GET", Settings.serverUri), this.ReturnGamesList);
            routes.Add(new Path("POST", Settings.serverUri), this.CreateGame);
            routes.Add(new Path("GET", Settings.serverUri + "userid"), this.GiveUserId);
            this.server.HttpRequest += this.RequestHandler;
        }

        /// <summary>Searches for a function to call in the routes dictionary based of HttpMethod and Url</summary>
        private void RequestHandler(object sender, RequestProcessorEventArgs e) {
            Action<object, RequestProcessorEventArgs> route;
            if(routes.TryGetValue(new Path(e.context.Request.HttpMethod, e.context.Request.Url.ToString()), out route)) {
                route(sender, e);
            }
        }

        partial void ReturnGamesList(object sender, RequestProcessorEventArgs e);
        partial void CreateGame(object sender, RequestProcessorEventArgs e);
        partial void GiveUserId(object sender, RequestProcessorEventArgs e);

        /// <summary>Sends a http message</summary>
        /// <param name="context">HttpListenerContext to which the message should be send</param>
        /// <param name="bOutput">Data to send</param>
        private async void Send(HttpListenerContext context, byte[] bOutput) {
            context.Response.ContentType = "Application/json";
            context.Response.ContentLength64 = bOutput.Length;

            Stream OutputStream = context.Response.OutputStream;
            context.Response.StatusCode = 200;
            await OutputStream.WriteAsync(bOutput, 0, bOutput.Length);
            context.Response.Close();
        }
    }
}