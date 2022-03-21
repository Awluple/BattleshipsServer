using System;
using System.Collections.Generic;
using System.Net.WebSockets;

using BattleshipsShared.Models;

namespace BattleshipsServer.Board
{
    public static class GamesList
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

    }

}