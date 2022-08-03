using System;
using System.Collections.Generic;

using BattleshipsServer.Board;

using BattleshipsShared.Models;
using BattleshipsShared.Communication;

using Newtonsoft.Json.Linq;

namespace BattleshipsServer
{

    partial class WebSocketHandlers {
        partial void JoinGame(object sender, WebSocketContextEventArgs e) {

            if(e.message.requestType != RequestType.JoinGame) {
                return;
            }

            var ws = e.WSocketResult;
            JObject obj = (JObject)e.message.data;

            Dictionary<string, object> data = obj.ToObject<Dictionary<string, object>>();
            JObject obje = (JObject)data["gameJoinInfo"];
            GameJoinInfo requestedGame = obje.ToObject<GameJoinInfo>();

            JoinConfirmation confirmation = GamesManager.AddPlayer(requestedGame.id, requestedGame.player, e.WSocketContext);

            Dictionary<string ,object> toSend = new Dictionary<string, object> {
                {"confirmation", confirmation}
            };
            Send(RequestType.JoinConfirmation, toSend, e.WSocketContext.WebSocket);

            Player? player = GamesManager.gamesList[requestedGame.id].GetOpponent(requestedGame.player);
            if(player != null) {
                Send(RequestType.OpponentFound, toSend, player.Value.WSocket.WebSocket);
            }
        }
    }
}