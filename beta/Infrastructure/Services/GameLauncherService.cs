using beta.Infrastructure.Services.Interfaces;

namespace beta.Infrastructure.Services
{
    public class GameLauncherService : IGameLauncherService
    {
        public bool GameIsRunning { get; set; } = false;
        public void JoinGame(int uid)
        {
            /* SEND
            "command": "game_join",
            "uid": _
            */

            /* REPLY
            "command": "game_launch",
            "args": ["/numgames", players.hosting.game_count[RatingType.GLOBAL]],
            "mod": "faf",
            "uid": 42,
            "name": "Test Game Name",
            "init_mode": InitMode.NORMAL_LOBBY.value,
            "game_type": "custom",
            "rating_type": "global",
             */
        }
        public void JoinGame(int uid, string password)
        {
            /* SEND
            "command": "game_join",
            "uid": _,
            "password": _,
            */

            /* REPLY / WRONG
            "command": "notice",
            "style": "info",
            "text": "Bad password (it's case sensitive)."
             */
        }

        public void RestoreGame(int uid)
        {
            /* SEND
            "command": "restore_game_session",
            "game_id": 123
            */

            /* REPLY / WRONG
            "command": "notice",
            "style": "info",
            "text": "The game you were connected to does no longer exist"
            */
        }
    }
}
