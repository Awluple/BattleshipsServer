using System;
using System.Collections.Generic;

using BattleshipsServer.Board;

using System.Net.WebSockets;
using BattleshipsShared.Communication;

using BattleshipsShared.Models;

namespace BattleshipsServer
{
    /// <summary>Handles players shots, checks the result of the shots</summary>
    partial class WebSocketHandlers {
        partial void PlayerShot(object sender, WebSocketContextEventArgs e) {

            var ws = e.WSocketResult;
            Shot shot = this.GetObjectFromMessage<Shot>("shot", e.message);

            Game game = GamesManager.GetGame(Int32.Parse(e.WSocketContext.Headers["game"]));
            ShotStatus shotStatus = game.CheckShot(shot.row, shot.column, e.WSocketContext.Headers["player"]);

            Dictionary<string ,object> toSend;

            RequestType request;

            if(shotStatus == ShotStatus.Finished) { // send game finished message if one of the user has lost all ships
                GameResult result = new GameResult(shot.column, shot.row, ShotStatus.Destroyed, Int32.Parse(e.WSocketContext.Headers["player"]));
                toSend = new Dictionary<string, object> {
                    {"gameResult", result}
                };

                request = RequestType.GameResult;

            } else {
                ShotResult result = new ShotResult(shot.column, shot.row, shotStatus);
                toSend = new Dictionary<string, object> {
                    {"shotResult", result}
                };
                request = RequestType.ShotResult;
            }
            Send(request, toSend, game.GetSocket(PlayerNumber.PlayerOne));
            Send(request, toSend, game.GetSocket(PlayerNumber.PlayerTwo));
        }
    }
}
