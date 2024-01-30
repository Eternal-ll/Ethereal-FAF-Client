using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;
using System;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal class FafUserService : IUserService
    {
        private readonly IFafAuthService _fafAuthService;
        private readonly IFafPlayersService _fafPlayersService;

        public FafUserService(IFafAuthService fafAuthService, IFafPlayersService fafPlayersService)
        {
            _fafAuthService = fafAuthService;
            _fafPlayersService = fafPlayersService;
        }
        private Player _Player;
        private bool TryGetSelf(out Player player)
        {
            if (_Player != null)
            {
                if (_Player.Id != _fafAuthService.GetUserId())
                {
                    _Player = null;
                }
                else
                {
                    player = _Player;
                    return true;
                }
            }
            if (!_fafPlayersService.TryGetPlayer(GetUserId(), out player))
            {
                return false;
            }
            _Player = player;
            return true;
        }

        public string GetClan()
        {
            if (TryGetSelf(out var player)) return player.Clan;
            return null;
        }

        public string GetCountry()
        {
            if (TryGetSelf(out var player)) return player.Country;
            return null;
        }

        public Rating GetRating(string rating)
        {
            if (TryGetSelf(out var player)) return rating switch
            {
                "global" => player.Ratings.Global,
                _ => throw new NotSupportedException("Unsupported requested rating name")
            };
            return null;
        }

        public int GetUserId()
        {
            return _fafAuthService.GetUserId();
        }

        public string GetUserName()
        {
            if (TryGetSelf(out var player)) return player.Login;
            return _fafAuthService.GetUserName();
        }
    }
}
