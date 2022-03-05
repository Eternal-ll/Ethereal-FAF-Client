using beta.Infrastructure.Services.Interfaces;
using beta.Infrastructure.Utils;
using beta.Models.API;
using beta.Models.Server;
using beta.Models.Server.Enums;
using beta.ViewModels;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    public enum GameLauncherState : byte
    {
        Idle = 0,
        Updating = 1,

        GameIsRunning = 10,
    }
    public class GameLauncherService : IGameLauncherService
    {
        #region Events
        public event EventHandler<EventArgs<TestDownloaderModel>> PatchUpdateRequired;
        #endregion

        private readonly IApiService ApiService;

        public GameLauncherState State { get; set; }
        public bool GameIsRunning { get; set; } = false;
        private GameInfoMessage LastGame;

        public GameLauncherService(IApiService apiService)
        {
            ApiService = apiService;
        }

        public void JoinGame()
        {
            if (LastGame != null)
            {

            }
        }
        public async Task JoinGame(GameInfoMessage game)
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
            LastGame = game;
            if (!await ConfirmPatch(game.FeaturedMod))
            {

            }
            else
            {

            }
        }
        public void JoinGame(GameInfoMessage game, string password)
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

        private async Task<bool> ConfirmPatch(FeaturedMod featuredMod)
        {
            var json = await ApiService.GET($"featuredMods/{(int)featuredMod}/files/latest");
            if (json == null) return false;

            var response = JsonSerializer.Deserialize<Record>(json);

            if (response.FeaturedModFiles == null) return false;

            var localPath = App.GetPathToFolder(Models.Folder.ProgramData);

            #region Check MD5 of existed files. Clearing list of required files to download
            Record record = new Record();
            record.FeaturedModFiles = new();
            for (int i = 0; i < response.FeaturedModFiles.Count; i++)
            {
                var item = response.FeaturedModFiles[i];

                var path = localPath + item.attributes["group"].ToString();

                // TODO Move this checks of Bin / Gamedata
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = path + "\\" + item.attributes["name"].ToString();

                if (File.Exists(path))
                {
                    var md5 = Tools.CalculateMD5(path);
                    if (md5 != item.attributes["md5"].ToString())
                    {
                        record.FeaturedModFiles.Add(item);
                        //response.FeaturedModFiles.RemoveAt(i);
                    }
                }
                else record.FeaturedModFiles.Add(item);
            }
            #endregion

            // if not 0, then we have files to update
            if (record.FeaturedModFiles.Count != 0)
            {
                // TODO FIX ME. Currently no optimized update
                // we just updating the whole Bin folder again

                // false if no bin folder in path to the game
                if (!CopyOriginalBin()) return false;

                TestDownloaderModel model = new(record);

                OnPatchUpdateRequired(model);

                model.DownloadFinished += Model_DownloadFinished;

                return false;
            }
            return true;
        }

        private async void Model_DownloadFinished(object sender, EventArgs<bool> e)
        {
            if (e) await JoinGame(LastGame).ConfigureAwait(false);

            ((TestDownloaderModel)sender).DownloadFinished -= Model_DownloadFinished;
        }

        private bool CopyOriginalBin()
        {
            var pathToBin = Properties.Settings.Default.PathToGame + "\\bin";

            if (pathToBin == null || pathToBin.Length == 0) return false;
            if (!Directory.Exists(pathToBin)) return false;

            var localPath = App.GetPathToFolder(Models.Folder.ProgramData) + "bin";

            if (!Directory.Exists(localPath))
                Directory.CreateDirectory(localPath);

            var files = Directory.GetFiles(pathToBin);
            for (int i = 0; i < files.Length; i++)
            {
                File.Copy(files[i], files[i].Replace(pathToBin, localPath), true);
            }

            return true;
        }

        private void OnPatchUpdateRequired(TestDownloaderModel model) => PatchUpdateRequired?.Invoke(this, model);
    }
}
