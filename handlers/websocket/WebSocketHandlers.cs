using System.IO;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using System.Threading;
using System.Net.WebSockets;


namespace BattleshipsServer
{
    public struct Message
    {

        public Message(string dataType, object data) {
            this.dataType = dataType;
            this.data = data;
        }

        public string dataType { get; set; }
        public object data { get; set; }
    }
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

        private void GetDeserialized<T>(ref T result, byte[] receiveBuffer) {
            MemoryStream ms = new MemoryStream(receiveBuffer);

            using (BsonDataReader reader = new BsonDataReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                result = serializer.Deserialize<T>(reader);
            }
        }

        private async void Send(string dataType, object toSend, WebSocket wsocket) {
            using (MemoryStream msa = new MemoryStream())
            using (BsonDataWriter datawriter = new BsonDataWriter(msa))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(datawriter, new Message(dataType, toSend));
                await wsocket.SendAsync(msa.ToArray(), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }

        partial void JoinGame(object sender, WebSocketContextEventArgs e);
    }
}