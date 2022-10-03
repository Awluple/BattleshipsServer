using System;
using System.Collections.Generic;

using BattleshipsServer.Board;

using BattleshipsShared.Models;
using BattleshipsShared.Communication;

using Newtonsoft.Json.Linq;

namespace BattleshipsServer
{

    partial class WebSocketHandlers {
        partial void Rematch(object sender, WebSocketContextEventArgs e) {

            if(e.message.requestType != RequestType.RematchProposition) {
                return;
            }

            Game game = GamesManager.GetGame(Int32.Parse(e.WSocketContext.Headers["game"]));
            Dictionary<string ,object> toSend = new Dictionary<string, object> {
            };

            if(game.rematchProposed) {
                game.reset();
                Console.WriteLine("Rematch accepted");
                Send(RequestType.RematchAccepted, toSend, game.GetSocket(PlayerNumber.PlayerOne));
                Send(RequestType.RematchAccepted, toSend, game.GetSocket(PlayerNumber.PlayerTwo));
            } else {
                Console.WriteLine("Rematch proposition sent");
                Send(RequestType.RematchProposition, toSend, game.GetOpponent(e.WSocketContext.Headers["player"]).Value.WSocket.WebSocket);
                game.rematchProposed = true;
            }
            
            }
    }
}
