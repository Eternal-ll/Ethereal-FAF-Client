using beta.Models.Server;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IAvatarService
    {
        public Task UpdatePlayerAvatarAsync(PlayerInfoMessage player, PlayerAvatar avatar);

    }
}
