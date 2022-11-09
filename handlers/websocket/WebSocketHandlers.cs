using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using System.Threading;
using System.Net.WebSockets;

using Newtonsoft.Json.Linq;

using BattleshipsShared.Communication;

namespace BattleshipsServer
{
    /// <summary>Holds WebSocket communication functions and sends WebSocket messages</summary>
    /// <param name="server">Server object to attach events</param>
    partial class WebSocketHandlers
    {

        private Dictionary<RequestType, Action<object, WebSocketContextEventArgs>> routes = new Dictionary<RequestType, Action<object, WebSocketContextEventArgs>>();
        private delegate void Route(object sender, WebSocketContextEventArgs  e);
        private Server server;

        public WebSocketHandlers(Server WebSocketHandlers) {
            server = WebSocketHandlers;
            this.Start();
        }

        public void Start() {
            this.server.WebSocketClose += this.ConnectionLost;
            
            routes.Add(RequestType.JoinGame, this.JoinGame);
            routes.Add(RequestType.SetBoard, this.SetBoard);
            routes.Add(RequestType.PlayerShot, this.PlayerShot);
            routes.Add(RequestType.RematchProposition, this.Rematch);
            this.server.WebSocketRequest += RequestHandler;
        }

        /// <summary>Searches for a function to call in the routes dictionary based of message RequestType</summary>
        private void RequestHandler(object sender, WebSocketContextEventArgs e) {
            Action<object, WebSocketContextEventArgs> route;
            if(routes.TryGetValue(e.message.requestType, out route)) {
                route(sender, e);
            }
        }

        /// <summary>Gets an object from WebSocket message</summary>
        /// <param name="dataKey">Key holding data in a message</param>
        /// <param name="message">Recieved message</param>
        public T GetObjectFromMessage<T>(string dataKey, Message message) {
            JObject obj = (JObject)message.data;

            Dictionary<string, object> data = obj.ToObject<Dictionary<string, object>>();
            JObject obje = (JObject)data[dataKey];
            T retrievedObject = obje.ToObject<T>();
            return retrievedObject;
        }

        /// <summary>Sends a WebSocket message</summary>
        /// <param name="dataType">Type of the message</param>
        /// <param name="toSend">Data to send</param>
        /// <param name="wsocket">WebSocket object to which data is to be sent</param>
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