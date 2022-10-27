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
    /// <summary>Holds all the game data such as players information and boards</summary>
    public class Game
    {
        public (Player? playerOne, Player? playerTwo) players = (null, null);

        public GameBoard playerOneBoard {get; set;}
        public GameBoard playerTwoBoard {get; set;}
        public bool finished = false;
        public bool rematchProposed = false;

        /// <summary>Resets the game state for rematch</summary>
        public void reset() {
            rematchProposed = false;
            playerOneBoard = null;
            playerTwoBoard = null;
            rematchProposed = false;
        }

        /// <summary>Gets the opponent object</summary>
        /// <param name="player">Current player id</param>
        /// <returns>Opponent player object</returns>
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
        /// <summary>Gets a result of a shot</summary>
        /// <param name="row">Row cell number</param>
        /// <param name="column">Column cell number</param>
        /// <returns>Shot result</returns>

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

        /// <summary>Gets WebSocket object of a player</summary>
        /// <param name="player">Player from whom to get the WebSocket object</param>
        /// <returns>WebSocket object of the player</returns>
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
        /// <summary>Checks if the game is full</summary>
        /// <returns>True if the game is full</returns>
        public bool IsFull() {
            return players.playerOne != null && players.playerTwo != null;
        }
    }
}