using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;

namespace beta.Infrastructure.Services
{
    public enum PlayerRelation
    {
        FOE = -1,
        NONE = 0,
        FRIEND = 1
    }
    public class SocialService : ISocialService
    {
        /*
        ## Social
        The social components of the lobby server are relatively limited, as the
        primary social element, chat, is handled by a separate server.The social
        features handled by the lobby server are therefore limited to:

        - Syncronizing online player state
        - Enforcing global bans
        - Modifying a list of friends and a list of foes
        - Modifying the currently selected avatar
        */

        #region Properties
        private readonly IPlayersService PlayersService;
        private readonly ISessionService SessionService;

        public SocialMessage SocialMessage { get; set; }

        public int[] FriendsIds { get; set;}

        public int[] FoesIds { get; set; }

        #endregion

        #region Ctor
        public SocialService(
            IPlayersService playersService,
            ISessionService sessionService)
        {
            PlayersService = playersService;
            SessionService = sessionService;

            sessionService.SocialInfo += OnNewSocialInfo;
        }
        #endregion

        #region Methods

        #region GetPlayer(int id, PlayerRelation relation)
        /// <summary>
        /// FAST | Get player with specified relation ship FOE / FRIEND / NONE by id.
        /// NONE returns Null.
        /// FRIEND by default. If Null then player is Offline
        /// </summary>
        /// <param name="login"></param>
        /// <param name="relation"></param>
        /// <returns>
        /// Player with associated relation
        /// </returns>
        public PlayerInfoMessage GetPlayer(int id, PlayerRelation relation = PlayerRelation.FRIEND)
        {
            if (relation == PlayerRelation.NONE) return null;

            /*
             * TODO: Loop cache list? Maybe offline JSON cache with all Friends / Foes, and notify if they are offline
             * for (...)
             *  if (... == ...) return ...
             */

            if (relation == PlayerRelation.FRIEND)
            {
                var friends = FriendsIds;
                if (friends != null & friends.Length > 0)
                {
                    for (int i = 0; i < friends.Length; i++)
                    {
                        if (friends[i] == id)
                        {
                            return PlayersService.GetPlayer(id);
                        }
                    }
                }
                return null;
            }

            // FOE
            var foes = FoesIds;

            if (foes != null & foes.Length > 0)
            {
                for (int i = 0; i < foes.Length; i++)
                {
                    if (foes[i] == id)
                    {
                        return PlayersService.GetPlayer(id);
                    }
                }
            }
            return null;
        }
        #endregion

        #region GetPlayer(int id, PlayerRelation relation)
        /// <summary>
        /// SLOW | Get player with specified relation ship FOE / FRIEND / NONE by login.
        /// NONE returns Null.
        /// FRIEND by default. If Null then player is Offline
        /// </summary>
        /// <param name="login"></param>
        /// <param name="relation"></param>
        /// <returns>
        /// Player with associated relation
        /// </returns>
        public PlayerInfoMessage GetPlayer(string login, PlayerRelation relation = PlayerRelation.FRIEND)
        {
            if (relation == PlayerRelation.NONE) return null;

            /*
             * TODO: Loop cache list? Maybe offline JSON cache with all Friends / Foes, and notify if they are offline
             * for (...)
             *  if (... == ...) return ...
             */

            var players = PlayersService.GetPlayers();
            var enumerator = players.GetEnumerator();

            if (relation == PlayerRelation.FRIEND)
            {
                var friends = FriendsIds;
                if (friends != null & friends.Length > 0)
                {
                    while (enumerator.MoveNext())
                    {
                        for (int i = 0; i < friends.Length; i++)
                        {
                            if (friends[i] == enumerator.Current.id)
                            {
                                return enumerator.Current;
                            }
                        }
                    }
                }                    
                return null;
            }

            // FOE
            var foes = FoesIds;
            if (foes != null & foes.Length > 0)
            {
                while (enumerator.MoveNext())
                {
                    for (int i = 0; i < foes.Length; i++)
                    {
                        if (foes[i] == enumerator.Current.id)
                        {
                            return enumerator.Current;
                        }
                    }
                }
            }
            return null;
        }
        #endregion
        
        #endregion

        #region LobbySessionService.SocialInfo event listener
        private void OnNewSocialInfo(object sender, EventArgs<SocialMessage> e)
        {
            SocialMessage = e.Arg;
            FriendsIds = e.Arg.friends;
            FoesIds = e.Arg.foes;
        } 
        #endregion
    }
}
