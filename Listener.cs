

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace HttpListenerWebSocketEcho
{        
    class WEBS
    {
        public WEBS()
        {
            var server = new Server();
            server.Start("http://127.0.0.1:7850/wsDemo/");
            Console.WriteLine("Press any key to exit...");
            server.HttpRequest += new_http_request;
            server.NewWebSocketRequest += new_ws_request;
            server.WebSocketRequest += ws_request;
            Console.ReadKey();


            void new_http_request (object sender, RequestProcessorEventArgs e)
            {
                Console.WriteLine("Elo http");
            }

            void new_ws_request (object sender, RequestProcessorEventArgs e)
            {
                Console.WriteLine("Elo new WS");
            }

            void ws_request (object sender, WSReceiveResultEventArgs e)
            {
                Console.WriteLine("Elo WS");
            }

        }
    }

    //## The Server class        


    public class WSReceiveResultEventArgs : System.EventArgs {
        public readonly WebSocketReceiveResult receiveResult;

        public WSReceiveResultEventArgs (WebSocketReceiveResult WSReceiveResult) {
            receiveResult = WSReceiveResult;
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

        public event EventHandler<RequestProcessorEventArgs> NewWebSocketRequest;
        public event EventHandler<WSReceiveResultEventArgs> WebSocketRequest;
        public event EventHandler<WSReceiveResultEventArgs> WebSocketClose;

        public event EventHandler<RequestProcessorEventArgs> HttpRequest;


        protected virtual void OnNewWebSocketRequest (RequestProcessorEventArgs e) {
            NewWebSocketRequest?.Invoke(this, e);
        }

        protected virtual void OnHttpRequest (RequestProcessorEventArgs e) {
            HttpRequest?.Invoke(this, e);
        }

        protected virtual void OnWebSocketRequest (WSReceiveResultEventArgs e) {
            WebSocketRequest?.Invoke(this, e);
        }

        protected virtual void OnWebSocketClose (WSReceiveResultEventArgs e) {
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
                    OnNewWebSocketRequest(new RequestProcessorEventArgs(listenerContext));
                    ProcessRequest(listenerContext);
                }
                else
                {
                    OnHttpRequest(new RequestProcessorEventArgs(listenerContext));
                    Console.WriteLine(listenerContext.Request.HttpMethod);
                    listenerContext.Response.StatusCode = 400;
                    listenerContext.Response.Close();
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

                    OnWebSocketRequest(new WSReceiveResultEventArgs(receiveResult));

                    
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        OnWebSocketClose(new WSReceiveResultEventArgs(receiveResult));
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    }
                    
                    else if (receiveResult.MessageType == WebSocketMessageType.Text)
                    {    
                        OnWebSocketClose(new WSReceiveResultEventArgs(receiveResult));
                        await webSocket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Cannot accept text frame", CancellationToken.None);
                    }

                    else
                    {                        
                        await webSocket.SendAsync(new ArraySegment<byte>(receiveBuffer, 0, receiveResult.Count), WebSocketMessageType.Binary, receiveResult.EndOfMessage, CancellationToken.None);
                    }

                    Console.WriteLine(Convert.ToBase64String(receiveBuffer)[..4]);

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
                    webSocket.Dispose();
            }
        }
    }
}