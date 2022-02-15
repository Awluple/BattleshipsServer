
namespace BattleshipsServer
{
    partial class WebSocketHandlers
    {
        private Server server;

        public WebSocketHandlers(Server WebSocketHandlers) {
            server = WebSocketHandlers;
            this.Start();
        }

        public void Start() {
            this.server.WebSocketRequest += this.JoinGame;
        }

        partial void JoinGame(object sender, WebSocketContextEventArgs e);
    }
}