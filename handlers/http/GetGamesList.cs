using System.Text.Json;
using System.Linq;
using System.IO;
using System.Collections.Generic;

using BattleshipsShared.Models;

using BattleshipsServer.Board;

namespace BattleshipsServer
{
    /// <summary>Returns list of all games on http request</summary>
    partial class HttpHandlers {
        partial void ReturnGamesList(object sender, RequestProcessorEventArgs e){
            if(e.context.Request.HttpMethod != "GET" || e.context.Request.Url.ToString() != Settings.serverUri) {
                return;
            }

            var Response = e.context.Response;

            GameInfo[] avaliableGames = GamesManager.gamesList.Select(e => {return new GameInfo(e.Key, e.Value.players.playerTwo == null ? 1 : 2);}).ToArray();

            Dictionary<string, GameInfo[]> keys = new Dictionary<string, GameInfo[]>() {
                {"games", avaliableGames}
            };

            byte[] bOutput = JsonSerializer.SerializeToUtf8Bytes(keys);


            Send(e.context, bOutput);
        }
    }
}