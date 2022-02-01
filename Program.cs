﻿using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using BattleshipsServer;
using BattleshipsBoard;

namespace BattleshipsServer
{

    class Program
    {
        static void Main(string[] args)
        {

            var server = new Server();
            server.Start("http://127.0.0.1:7850/wsDemo/");

            GamesList.RegisterGame("test1");
            GamesList.RegisterGame("Test2");

            var httpHandler = new HttpHandlers(server);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        

        }
    }

}