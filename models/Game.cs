
namespace BattleshipsBoard
{
    public class Game
    {
        private (string playerOne, string playerTwo) players = (null, null);


        public Game(string player) {
            players.playerOne = player;
        }

        public override string ToString(){
            return "1";
        }
    }
}