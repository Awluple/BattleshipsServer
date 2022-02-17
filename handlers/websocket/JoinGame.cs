using BattleshipsBoard;

namespace BattleshipsServer
{

    public struct GameJoinInfo
    {
        public GameJoinInfo(int id, string player)
        {
            this.id = id;
            this.player = player;
        }
        public int id { get; }
        public string player { get; }
    };

    public struct Confirmation
    {
        #nullable enable
        public Confirmation(bool succeed, string? player = null)
        {
            this.succeed = succeed;
            this.player = player;
        }
        public bool succeed { get; }
        public string? player { get; }
    }

    partial class WebSocketHandlers {
        partial void JoinGame(object sender, WebSocketContextEventArgs e) {

            var ws = e.WSocketResult;

            GameJoinInfo requestedGame = new GameJoinInfo();

            GetDeserialized<GameJoinInfo>(ref requestedGame, e.receiveBuffer);

            Confirmation confirmation;

            // check if player2 is empty, then add it
            if(GamesList.gamesList.ContainsKey(requestedGame.id) && GamesList.gamesList[requestedGame.id].players.playerTwo == null){
                GamesList.AddPlayer(GamesList.gamesList[requestedGame.id], requestedGame.player);
                confirmation = new Confirmation(true, GamesList.gamesList[requestedGame.id].players.playerOne);
            } else {
                confirmation = new Confirmation(false);
            }
            
            Send(confirmation, e.WSocketContext.WebSocket);
        }
    }
}