using System;
using System.IO;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;

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

    partial class WebSocketHandlers {
        partial void JoinGame(object sender, WebSocketContextEventArgs e) {
            var ws = e.WSocketResult;
            
            MemoryStream ms = new MemoryStream(e.receiveBuffer);

            using (BsonDataReader reader = new BsonDataReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                GameJoinInfo game = serializer.Deserialize<GameJoinInfo>(reader);

                Console.WriteLine(game.id);
                Console.WriteLine("Player: " + game.player);
            }

        }
    }
}