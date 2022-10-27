using System.Text.Json;
using System;
using System.IO;
using System.Collections.Generic;

using BattleshipsShared.Models;

using BattleshipsServer.Board;

namespace BattleshipsServer
{

    partial class HttpHandlers {
        partial void GiveUserId(object sender, RequestProcessorEventArgs e){
            if(e.context.Request.HttpMethod != "GET" || e.context.Request.Url.ToString() != Settings.serverUri + "userid") {
                return;
            }

            Dictionary<string, string> user = new Dictionary<string, string>() {
                {"id", Users.getUserId().ToString()},
                {"sessionId", Settings.sessionId}
            };

            byte[] bOutput = JsonSerializer.SerializeToUtf8Bytes(user);

            Send(e.context, bOutput);
        }
    }
}