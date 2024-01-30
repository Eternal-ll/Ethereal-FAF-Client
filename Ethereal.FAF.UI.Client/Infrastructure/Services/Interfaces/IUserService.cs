using FAF.Domain.LobbyServer;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IUserService
    {
        public int GetUserId();
        public string GetUserName();
        public string GetClan();
        public string GetCountry();
        public Rating GetRating(string rating);
    }
}
