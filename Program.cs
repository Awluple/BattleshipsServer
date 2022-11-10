using System;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using BattleshipsServer;
using BattleshipsServer.Board;

namespace BattleshipsServer
{

    static class Settings {
        public static readonly string serverUri = "http://" + System.IO.File.ReadAllText(new Uri(@".\server_address.txt", UriKind.Relative).ToString()) + "/";
        public static string sessionId = "";

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Settings.sessionId = Settings.RandomString(16);
            var server = new Server();
            server.Start(Settings.serverUri);

            var httpHandler = new HttpHandlers(server);
            var WSHandler = new WebSocketHandlers(server);


            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();
           
        }
    }

}