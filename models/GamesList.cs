using System.Collections.Generic;

namespace BattleshipsBoard
{
    public static class GamesList
    {
        static private Dictionary<int, (string, Game)> _gamesList = new Dictionary<int, (string, Game)>();

        static public Dictionary<int, (string, Game)> gamesList {get {
            return _gamesList;
        }}

        static private int MaxId = 0;

        static public int RegisterGame(string player) {
            var game = new Game(player);
            _gamesList.Add(MaxId, (game.ToString(), game));
            MaxId++;
            
            return MaxId - 1;
        }

        static public void UnregisterGame(int gameId) {
            _gamesList.Remove(gameId);
        }

    }

}