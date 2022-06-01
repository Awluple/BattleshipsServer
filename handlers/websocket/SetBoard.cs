using System;
using System.Collections.Generic;

using BattleshipsServer.Board;

using System.Net.WebSockets;
using BattleshipsShared.Communication;

using Newtonsoft.Json.Linq;

using BattleshipsShared.Models;

namespace BattleshipsServer
{

    partial class WebSocketHandlers {
        partial void SetBoard(object sender, WebSocketContextEventArgs e) {
            if(e.message.requestType != RequestType.SetBoard) {
                return;
            }

            var ws = e.WSocketResult;
            JObject obj = (JObject)e.message.data;

            Dictionary<string, object> data = obj.ToObject<Dictionary<string, object>>();
            JObject obje = (JObject)data["userBoard"];
            UserBoard board = obje.ToObject<UserBoard>();

            bool gameReady = false;
            WebSocket playerSocket = null;
            bool boardSet = GamesManager.SetBoard(board, e.WSocketContext.Headers["player"], out gameReady, out playerSocket);
            Dictionary<string ,object> toSend = new Dictionary<string, object> {
            };

            if(boardSet && gameReady) {
                Send(RequestType.OpponentFound, toSend, playerSocket);
            }
        }
    }
}
