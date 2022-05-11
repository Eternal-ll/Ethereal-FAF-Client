using beta.Infrastructure.Services.Interfaces;
using beta.Models.API;
using beta.Models.Server;
using beta.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Data;

namespace beta.ViewModels
{
    internal class UserProfileViewModel : ViewModel
    {
        public UserProfileViewModel()
        {
            RelationshipsVM = new();
            var playersService = App.Services.GetService<IPlayersService>();
            Self = playersService.Self;
            if (Self is not null)
            {
                ProfileViewModel = new(Self);
            }
        }
        public RelationshipsViewModel RelationshipsVM { get; set; }
        public ProfileViewModel ProfileViewModel { get; set; }
        public PlayerInfoMessage Self { get; set; }
    }

    internal class RelationshipsViewModel : ApiViewModel
    {
        private readonly ISocialService SocialService;
        private List<int> Friends;
        private List<int> Foes;

        public ObservableCollection<ApiPlayerData> FriendsData { get; set; } = new();
        public ObservableCollection<ApiPlayerData> FoesData { get; set; } = new();

        public RelationshipsViewModel()
        {
            var socialService = App.Services.GetService<ISocialService>();
            var sessionService = App.Services.GetService<ISessionService>();
            SocialService = socialService;
            socialService.AddedFriend += SocialService_AddedFriend;
            socialService.RemovedFriend += SocialService_RemovedFriend;
            socialService.RemovedFoe += SocialService_RemovedFoe;
            socialService.AddedFoe += SocialService_AddedFoe;
            sessionService.SocialDataReceived += SessionService_SocialDataReceived;

            BindingOperations.EnableCollectionSynchronization(FriendsData, new());
            BindingOperations.EnableCollectionSynchronization(FoesData, new());
        }

        private void SessionService_SocialDataReceived(object sender, SocialData e)
        {
            Friends = new(e.friends);
            Foes = new(e.foes);
            RunRequest();
        }

        private void SocialService_AddedFoe(object sender, int e)
        {
            Task.Run(() =>
            {
                var player = GetPlayer(e);
                Foes.Add(e);
                FoesData.Add(player.Result);
            });
        }

        private void SocialService_RemovedFoe(object sender, int e)
        {
            for (int i = 0; i < FoesData.Count; i++)
            {
                if (FoesData[i].Id == e)
                {
                    FoesData.RemoveAt(i);
                    break;
                }
            }
            Foes.Remove(e);
        }

        private void SocialService_RemovedFriend(object sender, int e)
        {
            for (int i = 0; i < FriendsData.Count; i++)
            {
                if (FriendsData[i].Id == e)
                {
                    FriendsData.RemoveAt(i);
                    break;
                }
            }
            Friends.Remove(e);
        }

        private void SocialService_AddedFriend(object sender, int e)
        {
            Task.Run(() =>
            {
                var player = GetPlayer(e);
                Friends.Add(e);
                FriendsData.Add(player.Result);
            });
        }

        private async Task<ApiPlayerData> GetPlayer(int id)
        {
            var res = await ApiRequest<ApiUniversalResult<ApiPlayerData>>.RequestWithId("https://api.faforever.com/data/player/", id);
            return res.Data;
        }

        protected override async Task RequestTask()
        {
            var friends = Friends;
            for (int i = 0; i < friends.Count; i++)
            {
                var player = await GetPlayer(friends[i]);
                FriendsData.Add(player);
            }
            var foes = Foes;
            for (int i = 0; i < foes.Count; i++)
            {
                var player = await GetPlayer(foes[i]);
                FoesData.Add(player);
            }
        }
    }
}
