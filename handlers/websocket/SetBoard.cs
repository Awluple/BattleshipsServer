using System;
using System.Collections.Generic;

using BattleshipsServer.Board;

using System.Net.WebSockets;
using BattleshipsShared.Communication;

using Newtonsoft.Json.Linq;

using BattleshipsShared.Models;

namespace BattleshipsServer
{
    /// <summary>Handles setting a game board for users</summary>
    partial class WebSocketHandlers {
        partial void SetBoard(object sender, WebSocketContextEventArgs e) {

            var ws = e.WSocketResult;
            UserBoard board = this.GetObjectFromMessage<UserBoard>("userBoard", e.message);

            bool gameReady = false;
            Game game = null;
            bool boardSet = GamesManager.SetBoard(board, Int32.Parse(e.WSocketContext.Headers["game"]), e.WSocketContext.Headers["player"], out gameReady, out game);

            Random rnd = new Random();
            Dictionary<string ,object> toSend = new Dictionary<string, object> {
                {"startingPlayer",  (object)(rnd.Next(1) == 0 ? e.WSocketContext.Headers["player"] : "0")} // a player who recives it's id has the first turn
            };
            if(boardSet && gameReady) {
                Send(RequestType.GameReady, toSend, game.GetSocket(PlayerNumber.PlayerOne));
                Send(RequestType.GameReady, toSend, game.GetSocket(PlayerNumber.PlayerTwo));
            }
        }
    }
}
