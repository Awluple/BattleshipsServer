using System;
using System.Collections.Generic;
using System.Net.WebSockets;

using BattleshipsShared.Models;

namespace BattleshipsServer.Board
{
    public static class GamesManager
    {
        static private Dictionary<int, Game> _gamesList = new Dictionary<int, Game>();

        static public Dictionary<int, Game> gamesList {get {
            return _gamesList;
        }}

        static private int MaxId = 0;

        static public int RegisterGame() {
            var game = new Game();
            _gamesList.Add(MaxId, game);
            MaxId++;
            
            return MaxId - 1;
        }
        static public JoinConfirmation AddPlayer(int gameId, string player, WebSocketContext WSocket) {
            Game game;
            if (!gamesList.TryGetValue(gameId, out game) && ((game.players.playerOne == null) || (game.players.playerTwo == null))) {
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

        static public void UnregisterGame(int gameId) {
            _gamesList.Remove(gameId);
        }

        static public Game GetGame(int gameId) {
            Game game;
            bool gameExist =  _gamesList.TryGetValue(gameId, out game);
            return game;
        }

        static public bool SetBoard(UserBoard userBoard, string player, out bool gameReady, out WebSocket playerSocket, out WebSocket opponentSocket){
            Game game = GetGame(userBoard.gameId);

            if(game.players.playerOne.Value.userId == player) {
                game.playerOneBoard = new Board(userBoard.board);
                playerSocket = game.players.playerOne.Value.WSocket.WebSocket;
                opponentSocket = game.players.playerTwo.Value.WSocket.WebSocket;
            } else if(game.players.playerTwo.Value.userId == player) {
                game.playerTwoBoard = new Board(userBoard.board);
                playerSocket = game.players.playerTwo.Value.WSocket.WebSocket;
                opponentSocket = game.players.playerOne.Value.WSocket.WebSocket;
            } else {
                gameReady = false;
                playerSocket = null;
                opponentSocket= null;
                return false;
            }
            if(game.playerOneBoard != null && game.playerTwoBoard != null){
                gameReady = true;
                opponentSocket = game.GetOpponent(player).Value.WSocket.WebSocket;
            } else {
                opponentSocket = null;
                gameReady = false;
            }
            return true;
        }

    }

}