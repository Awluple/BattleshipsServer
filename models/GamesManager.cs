using System;
using System.Collections.Generic;
using System.Net.WebSockets;

using BattleshipsShared.Models;

namespace BattleshipsServer.Board
{
    /// <summary>Creates and sets games objects, keeps all games objects</summary>
    public static class GamesManager
    {
        static private Dictionary<int, Game> _gamesList = new Dictionary<int, Game>();

        static public Dictionary<int, Game> gamesList {get {
            return _gamesList;
        }}

        static private int MaxId = 0;
        
        /// <summary>Creates new game object</summary>
        /// <returns>Id of a newly created game object</returns>
        static public int RegisterGame() {
            var game = new Game();
            _gamesList.Add(MaxId, game);
            MaxId++;
            
            return MaxId - 1;
        }
        /// <summary>Checks if it is possible to join a game</summary>
        /// <param name="gameId">Game id to join</param>
        /// <param name="player">Player id trying to join</param>
        /// <param name="WSocket">WebSocketContext of the player</param>
        /// <returns>JoinConfirmation object with the result</returns>
        static public JoinConfirmation AddPlayer(int gameId, string player, WebSocketContext WSocket) {
            Game game;
            if (!gamesList.TryGetValue(gameId, out game)) {
                return new JoinConfirmation(false, null);
            }
            if(game.IsFull()) {
                return new JoinConfirmation(false, null);
            }
            
            if(game.players.playerOne == null) {
                game.players.playerOne = new Player(player, WSocket);
            } else {
                game.players.playerTwo = new Player(player, WSocket);
            }

            Player? opponent = game.GetOpponent(player);

            return new JoinConfirmation(true, opponent.HasValue ? opponent.Value.userId : null);
            
        }
        /// <summary>Removes a game</summary>
        static public void UnregisterGame(int gameId) {
            _gamesList.Remove(gameId);
        }
        
        static public Game GetGame(int gameId) {
            Game game;
            bool gameExist =  _gamesList.TryGetValue(gameId, out game);
            return game;
        }
        /// <summary>Seats a board for a player in a game object</summary>
        /// <param name="userBoard">Board to set</param>
        /// <param name="gameId">Game to which the board should be set</param>
        /// <param name="player">Player to whom the board should be set</param>
        /// <param name="gameReady">True if game is ready to start</param>
        /// <param name="requestedGame">Game object from the id</param>
        /// <returns>True if the board has been successfully set</returns>
        static public bool SetBoard(UserBoard userBoard, int gameId, string player, out bool gameReady, out Game requestedGame){
            Game game = GetGame(gameId);
            requestedGame = game;
            if(game.players.playerOne.Value.userId == player) {
                game.playerOneBoard = new GameBoard(userBoard.board);
            } else if(game.players.playerTwo.Value.userId == player) {
                game.playerTwoBoard = new GameBoard(userBoard.board);
            } else {
                gameReady = false;
                return false;
            }
            
            if(game.playerOneBoard != null && game.playerTwoBoard != null){
                gameReady = true;
            } else {
                gameReady = false;
            }
            return true;
        }

    }

}