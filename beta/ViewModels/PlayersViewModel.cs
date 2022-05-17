using beta.Infrastructure.Converters;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace beta.ViewModels
{
    public class PlayersViewModel : Base.ViewModel
    {
        private readonly IPlayersService PlayersService;
        private readonly IGamesService GamesService;
        //private readonly IAvatarService AvatarService;
        private object _lock = new();
        public PlayersViewModel()
        {
            PlayersService = App.Services.GetService<IPlayersService>();
            GamesService = App.Services.GetService<IGamesService>();

            var players = PlayersService.Players;
            Players = new(players);

            BindingOperations.EnableCollectionSynchronization(Players, _lock);

            PlayersCollectionViewSource = new()
            {
                Source = Players
            };

            PropertyGroupDescription groupDescription = new(null, new ChatUserGroupConverter());
            groupDescription.GroupNames.Add("Me");
            groupDescription.GroupNames.Add("Favourites");
            groupDescription.GroupNames.Add("Moderators");
            groupDescription.GroupNames.Add("Friends");
            groupDescription.GroupNames.Add("Clan");
            groupDescription.GroupNames.Add("Players");
            groupDescription.GroupNames.Add("IRC users");
            groupDescription.GroupNames.Add("Foes");
            PlayersCollectionViewSource.GroupDescriptions.Add(groupDescription);
            PlayersCollectionViewSource.SortDescriptions.Add(new(nameof(IPlayer.login), ListSortDirection.Ascending));

            PlayersCollectionViewSource.Filter += PlayersCollectionViewSource_Filter;

            PlayersService.PlayerLeft += PlayersService_PlayerLeft;
            //PlayersCollectionViewSource.SortDescriptions.Add(
            //    new SortDescription(nameof(PlayerInfoMessage.RelationShip), ListSortDirection.Ascending));
            PlayersService.PlayersReceived += PlayersService_PlayersReceived;
            PlayersService.PlayerReceived += PlayersService_PlayerReceived;
            PlayersService.PlayerUpdated += PlayersService_PlayerUpdated;
        }

        private void PlayersService_PlayerLeft(object sender, PlayerInfoMessage e)
        {
            Players.Remove(e);
        }

        public ObservableCollection<IPlayer> Players { get; private set; }
        private readonly CollectionViewSource PlayersCollectionViewSource;
        public ICollectionView PlayersView => PlayersCollectionViewSource.View;

        #region SelectedPlayer
        private IPlayer _SelectedPlayer;
        public IPlayer SelectedPlayer
        {
            get => _SelectedPlayer;
            set => Set(ref _SelectedPlayer, value);
        }
        #endregion

        private void PlayersService_PlayersReceived(object sender, PlayerInfoMessage[] e)
        {
            for (int i = 0; i < e.Length; i++)
            {
                var p = e[i];
                if (e[i].RelationShip == Models.Server.Enums.PlayerRelationShip.Me && p.login == "Eternal-")
                {

                }
                UpdatePlayer(e[i]);
            }
        }

        private void PlayersCollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            var filter = FilterText;
            if (string.IsNullOrWhiteSpace(filter)) return;
            var player = (IPlayer)e.Item;

            if (player.clan is not null)
            {
                e.Accepted = player.clan.Contains(filter, System.StringComparison.OrdinalIgnoreCase);
                return;
            }
            e.Accepted = player.login.Contains(filter, System.StringComparison.OrdinalIgnoreCase);
        }

        #region FilterText
        private string _FilterText = string.Empty;
        public string FilterText
        {
            get => _FilterText;
            set
            {
                if (Set(ref _FilterText, value))
                {
                    PlayersView.Refresh();
                }
            }
        }
        #endregion
            
        private void PlayersService_PlayerReceived(object sender, PlayerInfoMessage e) => UpdatePlayer(e);
        private void PlayersService_PlayerUpdated(object sender, PlayerInfoMessage e) => UpdatePlayer(e);
        private void UpdatePlayer(PlayerInfoMessage player)
        {
            if (player.RelationShip == Models.Server.Enums.PlayerRelationShip.Me && player.login =="Eternal-")
            {

            }
            var players = Players;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].id == player.id)
                {
                    players[i] = player;
                    return;
                }
            }
            Players.Add(player);
        }

    }
}
