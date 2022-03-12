using System.Net;
using System.IO;

namespace BattleshipsServer
{
    partial class HttpHandlers
    {
        private Server server;

        public HttpHandlers(Server httpServer) {
            server = httpServer;
            this.Start();
        }

        public void Start() {
            this.server.HttpRequest += this.ReturnGamesList;
            this.server.HttpRequest += this.CreateGame;
            this.server.HttpRequest += this.GiveUserId;
        }

        partial void ReturnGamesList(object sender, RequestProcessorEventArgs e);
        partial void CreateGame(object sender, RequestProcessorEventArgs e);
        partial void GiveUserId(object sender, RequestProcessorEventArgs e);

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