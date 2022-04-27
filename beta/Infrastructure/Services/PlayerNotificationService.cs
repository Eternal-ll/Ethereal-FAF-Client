using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.Models.Server.Enums;
using Notification.Wpf;

namespace beta.Infrastructure.Services
{
    public class PlayerConnectedEventArgs
    {
        public PlayerInfoMessage Player { get; }
        public PlayerConnectedEventArgs(PlayerInfoMessage player)
        {
            Player = player;
        }
    }
    public class PlayerDisconnectedEventArgs
    {
        public PlayerInfoMessage Player { get; }
        public PlayerDisconnectedEventArgs(PlayerInfoMessage player)
        {
            Player = player;
        }
    }
    internal class PlayerNotificationService : IPlayerNotificationService
    {
        private readonly IPlayersService PlayersService;
        private readonly IGamesService GamesService;
        private readonly NotificationManager NotificationManager;

        public PlayerNotificationService(IPlayersService playersService, IGamesService gamesService)
        {
            PlayersService = playersService;
            GamesService = gamesService;
            NotificationManager = new();

            playersService.PlayersReceived += PlayersService_PlayersReceived;
            playersService.PlayerReceived += PlayersService_PlayerReceived;
            playersService.PlayerLeft += PlayersService_PlayerLeft;

            gamesService.PlayersJoinedGame += GamesService_PlayersJoinedGame;
            gamesService.PlayersLeftGame += GamesService_PlayersLeftGame;
            gamesService.GameClosed += GamesService_GameClosed;
            gamesService.GameEnd += GamesService_GameEnd;
        }



        private void GamesService_GameClosed(object sender, GameInfoMessage e)
        {

        }

        private void GamesService_GameEnd(object sender, GameInfoMessage e)
        {

        }

        private void GamesService_PlayersJoinedGame(object sender, System.Collections.Generic.KeyValuePair<GameInfoMessage, PlayerInfoMessage[]> e)
        {

        }

        private void GamesService_PlayersLeftGame(object sender, System.Collections.Generic.KeyValuePair<GameInfoMessage, PlayerInfoMessage[]> e)
        {

        }



        #region PlayersService events handlers
        private void PlayersService_PlayersReceived(object sender, PlayerInfoMessage[] e)
        {
            foreach (var player in e)
            {
                HandleAndNotifyOnConnect(player);
            }
        }
        private void PlayersService_PlayerReceived(object sender, PlayerInfoMessage e) =>
            HandleAndNotifyOnConnect(e);

        private void PlayersService_PlayerLeft(object sender, PlayerInfoMessage e) =>
            HandleAndNotifyOnLeave(e);

        private void HandleAndNotifyOnConnect(PlayerInfoMessage player)
        {
            if (player.RelationShip == PlayerRelationShip.Me ||
                player.RelationShip == PlayerRelationShip.None)
                return;

            if (player.IsFavourite)
            {
                return;
            }

            if (player.IsClanmate)
            {

                return;
            }

            if (player.RelationShip == PlayerRelationShip.Friend)
            {

                return;
            }

            if (player.RelationShip == PlayerRelationShip.Foe)
            {

                return;
            }
            NotificationManager.Show(new PlayerConnectedEventArgs(player));
        }

        private void HandleAndNotifyOnLeave(PlayerInfoMessage player)
        {
            if (player.RelationShip == PlayerRelationShip.Me ||
                player.RelationShip == PlayerRelationShip.None)
                return;

            if (player.IsFavourite)
            {
                return;
            }

            if (player.IsClanmate)
            {
                return;
            }

            if (player.RelationShip == PlayerRelationShip.Friend)
            {
                return;
            }

            if (player.RelationShip == PlayerRelationShip.Foe)
            {
                return;
            }

        }
        #endregion
    }
}
