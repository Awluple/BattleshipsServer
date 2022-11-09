using System;
using System.Collections.Generic;

using BattleshipsServer.Board;

using BattleshipsShared.Models;
using BattleshipsShared.Communication;

using Newtonsoft.Json.Linq;

namespace BattleshipsServer
{
    /// <summary>Handles games join requests</summary>
    partial class WebSocketHandlers {
        partial void JoinGame(object sender, WebSocketContextEventArgs e) {

            var ws = e.WSocketResult;
            GameJoinInfo requestedGame = this.GetObjectFromMessage<GameJoinInfo>("gameJoinInfo", e.message);

            JoinConfirmation confirmation = GamesManager.AddPlayer(requestedGame.id, requestedGame.player, e.WSocketContext);

            Dictionary<string ,object> toSend = new Dictionary<string, object> {
                {"confirmation", confirmation}
            };
            Send(RequestType.JoinConfirmation, toSend, e.WSocketContext.WebSocket);

            if(confirmation.succeed == false) return;

            Player? player = GamesManager.GetGame(requestedGame.id).GetOpponent(requestedGame.player);
            if(player != null) {
                Send(RequestType.OpponentFound, toSend, player.Value.WSocket.WebSocket);
            }
        }
    }
}