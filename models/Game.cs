using System;
using System.Net.WebSockets;

using BattleshipsShared.Models;

using BattleshipsShared.Communication;

namespace BattleshipsServer.Board
{

    public enum PlayerNumber
    {
        PlayerOne,
        PlayerTwo
    }

    public class Game
    {
        public (Player? playerOne, Player? playerTwo) players = (null, null);

        public GameBoard playerOneBoard {get; set;}
        public GameBoard playerTwoBoard {get; set;}
        public bool finished = false;
        public bool rematchProposed = false;

        public Game() {
        }

        public void reset() {
            rematchProposed = false;
            playerOneBoard = null;
            playerTwoBoard = null;
            rematchProposed = false;
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

        public ShotStatus CheckShot(int row, int column, string player) {
            if(players.playerOne == null || players.playerTwo == null) {
                throw new NullReferenceException("One of the players is not set");
            }
            if(player == players.playerOne.Value.userId) {
                return playerTwoBoard.CheckShot(row, column);
            } else {
                return playerOneBoard.CheckShot(row, column);
            }
        }

        public WebSocket? GetSocket(PlayerNumber player) {
            if(players.playerOne == null || players.playerTwo == null) {
                throw new NullReferenceException("One of the players is not set");
            }
            if(player == PlayerNumber.PlayerOne) {
                return players.playerOne.Value.WSocket.WebSocket;
            } else {
                return players.playerTwo.Value.WSocket.WebSocket;
            }
        }
    }
}