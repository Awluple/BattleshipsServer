

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

    public class RequestProcessorEventArgs : System.EventArgs {
        public readonly HttpListenerContext context;

        public RequestProcessorEventArgs (HttpListenerContext listenerContext) {
            context = listenerContext;
        }

    }

    class Server
    {        
        private int count = 0;

        // public event EventHandler<WebSocketContextEventArgs> NewWebSocketRequest;
        public event EventHandler<WebSocketContextEventArgs> WebSocketRequest;
        public event EventHandler<WebSocketContextEventArgs> WebSocketClose;

        public event EventHandler<RequestProcessorEventArgs> HttpRequest;


        // protected virtual void OnNewWebSocketRequest (WebSocketContextEventArgs e) {
        //     NewWebSocketRequest?.Invoke(this, e);
        // }

        protected virtual void OnHttpRequest (RequestProcessorEventArgs e) {
            HttpRequest?.Invoke(this, e);
        }

        protected virtual void OnWebSocketRequest (WebSocketContextEventArgs e) {
            WebSocketRequest?.Invoke(this, e);
        }

        protected virtual void OnWebSocketClose (WebSocketContextEventArgs e) {
            WebSocketClose?.Invoke(this, e);
        }

        

        public async void Start(string listenerPrefix)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(listenerPrefix);
            listener.Start();
            Console.WriteLine("Listening...");
           
            while (true)
            {
                HttpListenerContext listenerContext = await listener.GetContextAsync();
                if (listenerContext.Request.IsWebSocketRequest)
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

        private async void ProcessRequest(HttpListenerContext listenerContext)
        {
            
            WebSocketContext webSocketContext = null;
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

            try
            {
                //### Receiving
                byte[] receiveBuffer = new byte[1024];

                // While the WebSocket connection remains open run a simple loop that receives data and sends it back.
                while (webSocket.State == WebSocketState.Open)
                {
                    
                    WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                    // Console.WriteLine("Req ID: " + webSocketContext.Headers["player"]);
                    
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        OnWebSocketClose(new WebSocketContextEventArgs(webSocketContext, receiveResult, receiveBuffer));
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    }
                    
                    else if (receiveResult.MessageType == WebSocketMessageType.Text)
                    {    
                        OnWebSocketClose(new WebSocketContextEventArgs(webSocketContext, receiveResult, receiveBuffer));
                        await webSocket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Cannot accept text frame", CancellationToken.None);
                    } else if (webSocketContext.Headers["Sec-WebSocket-Protocol"] != "bson") {
                        OnWebSocketClose(new WebSocketContextEventArgs(webSocketContext, receiveResult, receiveBuffer));
                        await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Unsupported subprotocol", CancellationToken.None);
                    }

                    else
                    {                        
                        OnWebSocketRequest(new WebSocketContextEventArgs(webSocketContext, receiveResult, receiveBuffer));
                        // await webSocket.SendAsync(new ArraySegment<byte>(receiveBuffer, 0, receiveResult.Count), WebSocketMessageType.Binary, receiveResult.EndOfMessage, CancellationToken.None);
                    }

                    // Console.WriteLine(Convert.ToBase64String(receiveBuffer)[..4]);

                }
            }
            catch(Exception e)
            {
                // Just log any exceptions to the console.
                Console.WriteLine("Exception: {0}", e);
            }
            finally
            {
                // Clean up by disposing the WebSocket once it is closed/aborted.
                if (webSocket != null)
                    Console.WriteLine("Disconnected ID: " + webSocketContext.Headers["player"]);
                    webSocket.Dispose(); 
            }
        }
    }
}