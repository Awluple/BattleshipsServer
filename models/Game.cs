using System.Net.WebSockets;

using BattleshipsShared.Models;

namespace BattleshipsServer.Board
{

    public class Game
    {
        public (Player? playerOne, Player? playerTwo) players = (null, null);

        public Board playerOneBoard {get; set;}
        public Board playerTwoBoard {get; set;}

        public Game() {
        }
        #nullable enable
        public Player? GetOpponent(string player) {
            if(players.playerOne == null || players.playerTwo == null) {
                return null;
            }
            if (player == players.playerOne.Value.userId) {
                return players.playerTwo;
            } else {
                return players.playerOne;
            }
        }
    }
}