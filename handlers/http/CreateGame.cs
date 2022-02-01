using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

using BattleshipsBoard;

namespace BattleshipsServer
{

    public struct User
    {
        public string username { get; set; }
    }

    partial class HttpHandlers {
        async partial void CreateGame(object sender, RequestProcessorEventArgs e) {
            var Response = e.context.Response;
            var Request = e.context.Request;


            if(Request.HttpMethod != "POST") {
                return;
            }
            if(Request.ContentType.ToLower() != "application/json") {
                Console.WriteLine(Request.ContentType);
                Response.StatusCode = 400;
                return;
            }

            User user = await JsonSerializer.DeserializeAsync<User>(Request.InputStream);
            
            Dictionary<string, byte> result = new Dictionary<string, byte>() {
                {"id", (byte)GamesList.RegisterGame(user.username)}
            };

            byte[] bOutput = JsonSerializer.SerializeToUtf8Bytes(result);

            Response.ContentType = "Application/json";
            Response.ContentLength64 = bOutput.Length;

            Stream OutputStream = Response.OutputStream;
            Response.StatusCode = 201;
            await OutputStream.WriteAsync(bOutput, 0, bOutput.Length);
        }
    }
}