

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Bson;

using BattleshipsShared.Communication;

namespace BattleshipsServer
{           
    public class WebSocketContextEventArgs : System.EventArgs {
        public readonly WebSocketContext WSocketContext;
        public readonly WebSocketReceiveResult WSocketResult;
        public readonly byte[] receiveBuffer;
        public readonly Message message;

        /// <summary>Deserializes data comming from the clent into Message object</summary>
        /// <param name="receiveBuffer">Received buffer from the listener</param>
        /// <returns>Deselialized Message object</returns>
        private Message GetData(byte[] receiveBuffer) {
            MemoryStream ms = new MemoryStream(receiveBuffer);
            try
                {
                    using (BsonDataReader reader = new BsonDataReader(ms))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        return serializer.Deserialize<Message>(reader);
                    }
                }
                catch (Newtonsoft.Json.JsonSerializationException)
                {
                    return new Message(RequestType.Error, null);
                }
        }

        public WebSocketContextEventArgs (WebSocketContext WSocketContext, WebSocketReceiveResult WSocketResult, byte[] receiveBuffer) {
            this.WSocketContext = WSocketContext;
            this.WSocketResult = WSocketResult;
            this.receiveBuffer = receiveBuffer;
            this.message = this.GetData(receiveBuffer);
        }
    }

    public class WebSocketContextDisconnectEventArgs : System.EventArgs {
        public readonly WebSocketContext WSocketContext;
        public readonly bool unexpectedClosure;

        public WebSocketContextDisconnectEventArgs (WebSocketContext WSocketContext, bool unexpectedClosure) {
            this.WSocketContext = WSocketContext;;
            this.unexpectedClosure = unexpectedClosure;
        }
    }

    public class RequestProcessorEventArgs : System.EventArgs {
        public readonly HttpListenerContext context;

        public RequestProcessorEventArgs (HttpListenerContext listenerContext) {
            context = listenerContext;
        }

    }
    /// <summary>Responsible for listening and handling WebSocket and Http messages, uses events to handle responses</summary>
    class Server
    {        
        private int count = 0;

        public event EventHandler<WebSocketContextEventArgs> WebSocketRequest;
        public event EventHandler<WebSocketContextDisconnectEventArgs> WebSocketClose;

        public event EventHandler<RequestProcessorEventArgs> HttpRequest;

        protected virtual void OnHttpRequest (RequestProcessorEventArgs e) {
            HttpRequest?.Invoke(this, e);
        }

        protected virtual void OnWebSocketRequest (WebSocketContextEventArgs e) {
            WebSocketRequest?.Invoke(this, e);
        }

        protected virtual void OnWebSocketClose (WebSocketContextDisconnectEventArgs e) {
            WebSocketClose?.Invoke(this, e);
        }

        
        /// <summary>Starts listening for WebSocket and Http messages</summary>
        /// <param name="listenerPrefix">Uri to listen</param>
        public async void Start(string listenerPrefix)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(listenerPrefix);
            try
            {
                listener.Start();
            }
            catch (System.Net.HttpListenerException)
            {
                Console.WriteLine("Failed to listen on prefix '{0}' because it conflicts with an existing registration on the machine. Please select a diffrent port in server_address.txt", Settings.serverUri);
                return;
            }
            Console.WriteLine("Listening at {0} \nSession ID: {1}", Settings.serverUri, Settings.sessionId);
            Console.WriteLine("Press any key to stop...");
           
            while (true)
            {
                HttpListenerContext listenerContext = await listener.GetContextAsync();

                // force new user id after server restart
                if((listenerContext.Request.Headers["sessionId"] == null || listenerContext.Request.Headers["sessionId"] != "") && listenerContext.Request.Headers["sessionId"] != Settings.sessionId) {
                        listenerContext.Response.StatusCode = 403;
                        listenerContext.Response.Close();
                }
                else if (listenerContext.Request.IsWebSocketRequest)
                {
                    ProcessRequest(listenerContext);
                }
                else
                {
                    OnHttpRequest(new RequestProcessorEventArgs(listenerContext));
                    Console.WriteLine(listenerContext.Request.HttpMethod + " - " + listenerContext.Request.Url);
                }
            }
        }
        /// <summary>Decides how to response to WebSocket messages</summary>
        /// <param name="listenerContext">WebSocket request</param>
        private async void ProcessRequest(HttpListenerContext listenerContext)
        {
            
            WebSocketContext webSocketContext = null;
            // Set up WebSocket connection
            try
            {                
                webSocketContext = await listenerContext.AcceptWebSocketAsync(subProtocol: null);
                Console.WriteLine(webSocketContext.RequestUri);
                Interlocked.Increment(ref count);
                Console.WriteLine("Processed: {0}", count);
            }
            catch(Exception e)
            {
                listenerContext.Response.StatusCode = 500;
                listenerContext.Response.Close();
                Console.WriteLine("Exception: {0}", e);
                return;
            }
                                
            WebSocket webSocket = webSocketContext.WebSocket;
            
            // Process a message
            try
            {
                byte[] receiveBuffer = new byte[1024];

                while (webSocket.State == WebSocketState.Open)
                {                  
                    WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                    
                    if (receiveResult.MessageType == WebSocketMessageType.Close) {
                        OnWebSocketClose(new WebSocketContextDisconnectEventArgs(webSocketContext, false));
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    }
                    
                    else if (receiveResult.MessageType == WebSocketMessageType.Text) {    
                        OnWebSocketClose(new WebSocketContextDisconnectEventArgs(webSocketContext, false));
                        await webSocket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Cannot accept text frame", CancellationToken.None);
                    } else if (webSocketContext.Headers["Sec-WebSocket-Protocol"] != "bson") {
                        OnWebSocketClose(new WebSocketContextDisconnectEventArgs(webSocketContext, false));
                        await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Unsupported subprotocol", CancellationToken.None);
                    }

                    else {                        
                        OnWebSocketRequest(new WebSocketContextEventArgs(webSocketContext, receiveResult, receiveBuffer));
                    }

                }
            }
            catch(System.Net.WebSockets.WebSocketException e)
            {
                OnWebSocketClose(new WebSocketContextDisconnectEventArgs(webSocketContext, true));
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
            finally
            {
                // Clean up the WebSocket once it is closed/aborted.
                if (webSocket != null){
                    Console.WriteLine("Disconnected ID: " + webSocketContext.Headers["player"]);
                    OnWebSocketClose(new WebSocketContextDisconnectEventArgs(webSocketContext, true));
                    webSocket.Dispose();
                }
            }
        }
    }
}