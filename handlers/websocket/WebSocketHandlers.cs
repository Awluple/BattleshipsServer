using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using System.Threading;
using System.Net.WebSockets;

using BattleshipsShared.Communication;

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
            this.server.WebSocketRequest += this.SetBoard;
            this.server.WebSocketRequest += this.PlayerShot;
            this.server.WebSocketRequest += this.Rematch;
            this.server.WebSocketClose += this.ConnectionLost;
        }

        private void GetDeserialized<T>(byte[] receiveBuffer, out T result) {
            MemoryStream ms = new MemoryStream(receiveBuffer);

            using (BsonDataReader reader = new BsonDataReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                result = serializer.Deserialize<T>(reader);
            }
        }

        private async void Send(RequestType dataType, Dictionary<string ,object> toSend, WebSocket wsocket) {
            if(wsocket == null) return;
            using (MemoryStream msa = new MemoryStream())
            using (BsonDataWriter datawriter = new BsonDataWriter(msa))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(datawriter, new Message(dataType, toSend));
                await wsocket.SendAsync(msa.ToArray(), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }

        partial void JoinGame(object sender, WebSocketContextEventArgs e);
        partial void SetBoard(object sender, WebSocketContextEventArgs e);
        partial void PlayerShot(object sender, WebSocketContextEventArgs e);
        partial void Rematch(object sender, WebSocketContextEventArgs e);
        partial void ConnectionLost(object sender, WebSocketContextDisconnectEventArgs e);
    }
}