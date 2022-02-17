using System.Collections.Generic;
using System;
namespace BattleshipsBoard
{
    public static class GamesList
    {
        static private Dictionary<int, Game> _gamesList = new Dictionary<int, Game>();

        static public Dictionary<int, Game> gamesList {get {
            return _gamesList;
        }}

        static private int MaxId = 0;

        static public int RegisterGame(string player) {
            var game = new Game(player);
            _gamesList.Add(MaxId, game);
            MaxId++;
            
            return MaxId - 1;
        }
        static public void AddPlayer(Game game, string player) {
            game.players.playerTwo = player;
        }

        static public void UnregisterGame(int gameId) {
            _gamesList.Remove(gameId);
        }

    }

}