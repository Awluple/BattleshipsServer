# BattleshipsServer
Server for my multiplayer Battleships game

I had planned to do a small project to learn C#, but it has grown to a size that I did not expect.

## File structure
```
├─.vscode
├─handlers
│  ├─http       -- handlers for http requests
│  └─websocket  -- handlers for WebSocket messages
└─models        -- classes to hold games data
```

## Requirements
 [BattleshipsShared](https://github.com/Awluple/BattleshipsShared)
 
 Newtonsoft.Json
 
 Newtonsoft.Json.Bson
 <br/><br/>
 
 And the client:  [Battleships](https://github.com/Awluple/Battleships)
 
 ## Building
 detnet build
 
**You must specify BattleshipsShared.dll location in BattleshipServer.csproj to build**

 ## Running
Server address can be specified in server_address.txt, the default is 127.0.0.0:7850
