# Ethereal FAF Client

Unofficial FAForever client by Eternal-ll, based on C# WPF .NET 8.0 for Windows OS only

[Legacy client v1.0 source code](/Ethereal-FAF-Client-legacy)

## Development

### Requirements

- .NET 8.0 SDK
- Visual Studio 2022 or newer

### Configuration

You must create JSON configuration file called "appsettings.Production.json". Configuration must contain OAuth2 secrets to work properly.
```json
  "Server": {
    "Name": "Server name",
    "Site": "https://example.com",
    "Lobby": {
      "IsWss": true, -- selector for TCP or WSS connection
      "Url": "wss://ws.faforever.com", -- WSS url
      "Host": null, -- TCP host
      "Port": 0 -- TCP port
    },
    "Replay": {
      "Host": "replay.com",
      "Port": "1000"
    },
    "Relay": {
      "Host": "lobby.com",
      "Port": "2000"
    },
    "IRC": {
      "Host": "chat.com",
      "Port": "3000"
    },
    "API": "https://api.com/",
    "UserApi": "https://user.com/",
    "Content": "https://content.com/",
    "OAuth": {
      "BaseAddress": "https://auth.com/",
      "ResponseSeconds": 30,
      "ClientId": "", -- OAuth2 client-id
      "Scope": "", -- OAuth2 scope
      "RedirectPorts": [ ] -- localhost ports for OAUth2 callbacks with code
    },
    "Cloudfare": {
      "HMAC": {
        "Secret": "",
        "Param": ""
      }
    }
  }
```

