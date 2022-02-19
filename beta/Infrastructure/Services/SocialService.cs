using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;

namespace beta.Infrastructure.Services
{
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

        private readonly ISessionService SessionService;

        public SocialMessage SocialMessage { get; set; }

        #endregion

        #region Ctor
        public SocialService(ISessionService sessionService)
        {
            SessionService = sessionService;

            //sessionService.SocialInfo += OnNewSocialInfo;
        }
        #endregion

        #region Methods

        public void AddRelationShip(int id, PlayerRelationShip relation = PlayerRelationShip.Friend)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveRelationShip(int id, PlayerRelationShip relation = PlayerRelationShip.Friend)
        {
            throw new System.NotImplementedException();
        }

        private void OnNewSocialInfo(object sender, EventArgs<SocialMessage> e)
        {

        }

        #endregion
    }
}
