using System;
using System.Collections.Generic;

using BattleshipsServer.Board;

using BattleshipsShared.Models;
using BattleshipsShared.Communication;

using Newtonsoft.Json.Linq;

namespace BattleshipsServer
{
    /// <summary>Sends a message if a connection with one of the users has been lost and unregisteds a game</summary>
    partial class WebSocketHandlers {
        partial void ConnectionLost(object sender, WebSocketContextDisconnectEventArgs e) {
            int gameId = Int32.Parse(e.WSocketContext.Headers["game"]);

            Game game = GamesManager.GetGame(gameId);

            if(game == null || !game.isPlayer(e.WSocketContext.Headers["player"])) {
                 return;
            }

            if(game.finished) {
                Send(RequestType.OpponentLeft, new Dictionary<string, object>(), game.GetOpponent(e.WSocketContext.Headers["player"]).Value.WSocket.WebSocket);
                GamesManager.UnregisterGame(Int32.Parse(e.WSocketContext.Headers["game"]));
            } else if (game.GetOpponent(e.WSocketContext.Headers["player"]) == null) {
                GamesManager.UnregisterGame(Int32.Parse(e.WSocketContext.Headers["game"]));
            } else if(e.unexpectedClosure) {
                GamesManager.UnregisterGame(Int32.Parse(e.WSocketContext.Headers["game"]));
                Send(RequestType.OpponentConnectionLost, new Dictionary<string, object>(), game.GetOpponent(e.WSocketContext.Headers["player"]).Value.WSocket.WebSocket);
            }

        }
    }
}