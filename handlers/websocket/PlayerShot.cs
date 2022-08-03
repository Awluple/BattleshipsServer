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
        partial void PlayerShot(object sender, WebSocketContextEventArgs e) {
            if(e.message.requestType != RequestType.PlayerShot) {
                return;
            }

            var ws = e.WSocketResult;
            JObject obj = (JObject)e.message.data;

            Dictionary<string, object> data = obj.ToObject<Dictionary<string, object>>();
            JObject obje = (JObject)data["shot"];
            Shot shot = obje.ToObject<Shot>();

            Game game = GamesManager.GetGame(Int32.Parse(e.WSocketContext.Headers["game"]));
            ShotStatus shotStatus = game.CheckShot(shot.row, shot.column, e.WSocketContext.Headers["player"]);

            Dictionary<string ,object> toSend;

            RequestType request;

            if(shotStatus == ShotStatus.Finished) {
                GameResult result = new GameResult(shot.column, shot.row, ShotStatus.Destroyed, Int32.Parse(e.WSocketContext.Headers["player"]));
                toSend = new Dictionary<string, object> {
                    {"gameResult", result}
                };

                request = RequestType.GameResult;

            } else {
                ShotResult result = new ShotResult(shot.column, shot.row, shotStatus);
                Console.WriteLine($"{shot.column}, {shot.row}, {e.WSocketContext.Headers["player"]}, {result.shotStatus}");
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
