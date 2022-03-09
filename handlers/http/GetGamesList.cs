using System.Text.Json;
using System.Linq;
using System.IO;
using System.Collections.Generic;

using BattleshipsShared.Models;

using BattleshipsServer.Board;

namespace BattleshipsServer
{

    partial class HttpHandlers {
        async partial void ReturnGamesList(object sender, RequestProcessorEventArgs e){
            if(e.context.Request.HttpMethod != "GET") {
                return;
            }

            var Response = e.context.Response;

            GameInfo[] avaliableGames = GamesList.gamesList.Select(e => {return new GameInfo(e.Key, e.Value.players.playerTwo == null ? 1 : 2);}).ToArray();

            Dictionary<string, GameInfo[]> keys = new Dictionary<string, GameInfo[]>() {
                {"games", avaliableGames}
            };

            byte[] bOutput = JsonSerializer.SerializeToUtf8Bytes(keys);


            Response.ContentType = "Application/json";
            Response.ContentLength64 = bOutput.Length;

            Stream OutputStream = Response.OutputStream;
            Response.StatusCode = 200;
            await OutputStream.WriteAsync(bOutput, 0, bOutput.Length);
            Response.Close();
        }
    }
}