using BattleshipsServer.Board;
using System;

using BattleshipsShared.Models;
using BattleshipsShared.Communication;

namespace BattleshipsServer
{

    partial class WebSocketHandlers {
        partial void JoinGame(object sender, WebSocketContextEventArgs e) {

            var ws = e.WSocketResult;

            Console.WriteLine("Message: " + e.message.requestType);

            GameJoinInfo requestedGame = new GameJoinInfo();

            GetDeserialized<GameJoinInfo>(ref requestedGame, e.receiveBuffer);

            JoinConfirmation confirmation;

            // check if player2 is empty, then add it
            if(GamesList.gamesList.ContainsKey(requestedGame.id) && GamesList.gamesList[requestedGame.id].players.playerTwo == null){
                GamesList.AddPlayer(GamesList.gamesList[requestedGame.id], requestedGame.opponentPlayer);
                confirmation = new JoinConfirmation(true, GamesList.gamesList[requestedGame.id].players.playerOne);
            } else {
                confirmation = new JoinConfirmation(false, null);
            }
            
            Send(RequestType.JoinConfirmation, confirmation, e.WSocketContext.WebSocket);
        }
    }
}