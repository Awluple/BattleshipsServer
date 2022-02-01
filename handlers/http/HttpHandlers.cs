
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
        }

        partial void ReturnGamesList(object sender, RequestProcessorEventArgs e);
        partial void CreateGame(object sender, RequestProcessorEventArgs e);
    }
}