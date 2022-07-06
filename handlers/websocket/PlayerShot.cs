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

            Game game = GamesManager.GetGame(shot.gameId);
            ShotStatus shotStatus = game.CheckShot(shot.row, shot.column, shot.userId.ToString());

            ShotResult result = new ShotResult(shot.column, shot.row, shotStatus);
            Console.WriteLine($"{shot.column}, {shot.row}, {shot.userId.ToString()}, {result.shotStatus}");
            
            Dictionary<string ,object> toSend = new Dictionary<string, object> {
                {"shotResult", result}
            };

            Send(RequestType.ShotResult, toSend, game.GetSocket(PlayerNumber.PlayerOne));
            Send(RequestType.ShotResult, toSend, game.GetSocket(PlayerNumber.PlayerTwo));
        }
    }
}
