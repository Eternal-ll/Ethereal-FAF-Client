# Ethereal-FAF-Client

Main logic services
-
- OAuthService - authorization / authentication service. Using HttpClient and doing GET / POST requests
  - 123

- SessionService - Main server interaction service.
  - event <bool> Authorized -> AuthView, open main lobby view
  - event <PlayerInfoMessage> NewPlayer -> PlayersService
  - event <GameInfoMessage> NewGame -> GamesService
  - event <SocialMessage> SocialInfo -> SocialService // Unused

- GamesService
  - IdleGames - Open games
  - LiveGames - Games thats launched
  - SuspiciousGames - List of games (mainly open) that is bugged due to different reasons. Host didnt connected, etc. If after 120 seconds still no players, game being removed from Idle/Live games. 

- PlayersService
  - Players - ObservableCollection
  - PlayerUIDToId - Dictionary, uid to Players[id]
  - PlayerLoginToId - Dictionary, login to Players[id]

- SocialService
  - nothing yet

- AvatarService
  - Cache
  - GetAvatar(...) - returns avatar from cache or downloads from link, saves, caches and returns avatar

- MapService
  - Cache
  - GetMap(...) - returns map from cache or downloads from link, saves, caches and returns avatar

----
// Not implemented yet

- IceInteractionService???
- PatchService
- ModsService
- VaultService?
