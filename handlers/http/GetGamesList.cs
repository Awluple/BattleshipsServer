using System.Linq;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

using BattleshipsBoard;

namespace BattleshipsServer
{
    partial class HttpHandlers {
        async partial void ReturnGamesList(object sender, RequestProcessorEventArgs e){
            if(e.context.Request.HttpMethod != "GET") {
                return;
            }

            var Response = e.context.Response;

            Dictionary<string, int[]> keys = new Dictionary<string, int[]>() {
                {"gamesID", GamesList.gamesList.Keys.ToArray()}
            };

            byte[] bOutput = JsonSerializer.SerializeToUtf8Bytes(keys);

            Response.ContentType = "Application/json";
            Response.ContentLength64 = bOutput.Length;

            Stream OutputStream = Response.OutputStream;
            Response.StatusCode = 200;
            await OutputStream.WriteAsync(bOutput, 0, bOutput.Length);
        }
    }
}