# Ethereal-FAF-Client

Main logic services
-
- OAuthService - authorization / authentication service
  - 123

- SessionService - Main server interaction service.
  - event <bool> Authorized -> AuthView, open main lobby view
  - event <PlayerInfoMessage> NewPlayer -> PlayersService
  - event <GameInfoMessage> NewGame -> GamesService
  - event <SocialMessage> SocialInfo -> SocialService // Unused
  - GetSession() - ask and return session code
  - GenerateUID() - generates Unique PC Id
  - Authorize() - passing session and auth data for authorization

- GamesService
  - IdleGames - Open games
  - LiveGames - Games thats launched
  - SuspiciousGames - List of games (mainly open) that is bugged due to different reasons. Host didnt connected, etc. If after 120 seconds still no players, game being removed from Idle/Live games. 

- PlayersService
  - Players - ObservableCollection
  - PlayerUIDToId - Dictionary, uid to Players[id]
  - PlayerLoginToId - Dictionary, login to Players[id]

- SocialService
  - AddFriend(id) - adds specific user to friends
  - AddFoe() - adds specific user to foe (black list)
  - RemoveFriend()
  - AddFriend()
  - AddRelationShip() - creates friend/foe relationship
  - RemoveRelationShip() - removes friend/foe relationship

- AvatarService
  - GetAvatar(...) - Calls CacheService.GetImage()
  - SetAvatar() - Sets required avatar and notify FAF Lobby Server
  - UpdateAvaiableAvatas - Gets list of available avatars
  - Get..?

- MapService
  - GetMap() - returns map class GameMap/CoopMap/NeroxisMap
  - GetMapPreview(...) - Calls CacheService.GetImage()
  - IsLegacyMap() - Checks if map is original Supreme Commander Forged Alliance map
  - AttachMapScenario() - Parsing and loading data from local files about map. Name, description, size, mexs and hydros counts
  - Download() - not implemented

- CacheService
  - GetImage(Uri, Folder) - checking local cache for image, other way downloading from uri and caching locally

- GameLauncherService
  - JoinGame() - re-launch last game if update were required or something went wrong
  - JoinGame(...) - passing Game and Mod?
  - RestoreGame() - not implemented
  - ConfirmPatch(FeaturedMod) - API request and checks of MD5 of local patch
  - CopyOriginalBin() - copying original game BIN folder to local patch
  - OnPatchUpdateRequired(DownloaderModel) - invokes event for downloading patch

- IrcService
  - Methods-------------
  - Connect() - unused, connect to IRC FAF Server
  - Authorize - connects and authorize on IRC FAF Server
  - Join(channel) - joining to required channel
  - Leave(channel) - leaves from required channel
  - Leave(channels[]) - leaves from requires channels
  - LeaveAll() - leaves from all channels
  - Ping() - unused, can be used for monitoring connection stability
  - Quit() - disconnects from IRC FAF Server
  - SetTopic(channel, topic) - sets to required channel the topic
  - SendInvite(user, channel) - sends invite to specific user to join specific channel
  - Events-------------
  - UserConnected - to IRC server
  - UserDisconnected - from IRC server
  - UserJoined - to channel
  - UserLeft - from channel
  - UserChangedName
  - PrivateMessageReceived
  - ChannelMessageReceived
  - ChannelTopicUpdated
  - ChannelTopicChangedBy
  - ChannelUsersReceived

- ApiService

----
// Not implemented yet

- IceInteractionService??? This thing scares me >.< Service to control Ice-FAF-adapter for peer2peer connecions in game
- PatchService??
- ModsService
- VaultService?
