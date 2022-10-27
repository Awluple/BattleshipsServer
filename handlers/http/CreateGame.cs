using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

using BattleshipsShared.Models;

using BattleshipsServer.Board;

namespace BattleshipsServer
{
    /// <summary>Handles creation of game objects</summary>
    partial class HttpHandlers {
        partial void CreateGame(object sender, RequestProcessorEventArgs e) {
            var Response = e.context.Response;
            var Request = e.context.Request;

            if(Request.HttpMethod != "POST") {
                return;
            }
            if(Request.ContentType.ToLower() != "application/json") {
                Console.WriteLine(Request.ContentType);
                Response.StatusCode = 400;
                Response.Close();
                return;
            }

            
            Dictionary<string, int> result = new Dictionary<string, int>() {
                {"id", GamesManager.RegisterGame()}
            };

            byte[] bOutput = JsonSerializer.SerializeToUtf8Bytes(result);

            Send(e.context, bOutput);
        }
    }
}