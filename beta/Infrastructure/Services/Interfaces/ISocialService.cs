using beta.Models.Server;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface ISocialService
    {
        public SocialMessage SocialMessage { get; set; }
        public int[] FriendsIds { get; set; }
        public int[] FoesIds { get; set; }
    }
}
