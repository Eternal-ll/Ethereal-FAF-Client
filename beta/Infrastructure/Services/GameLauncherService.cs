using beta.Infrastructure.Services.Interfaces;
using beta.Infrastructure.Utils;
using beta.Models;
using beta.Models.API;
using beta.Models.Server;
using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using beta.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    public enum GameLauncherState : byte
    {
        Idle,
        SentCommandToJoin,
        DownloadingMods,
        DownloadingMap,
        Updating,
        SendingCommand,
        WaitingServer,
        LaunchingGame,
        PendingConnection,
        InGame,
    }
    public class GameLauncherService : IGameLauncherService
    {
        public event EventHandler<GameLauncherState> GameLauncherStateChanged;

        private readonly ISessionService SessionService;
        private readonly IGamesService GamesService;
        private readonly IDownloadService DownloadService;
        private readonly IMapsService MapsService;
        private readonly ILogger Logger;

        #region State
        private GameLauncherState _State;
        public GameLauncherState State
        {
            get => _State;
            set
            {
                if (!Equals(_State, value))
                {
                    _State = value;
                    OnStateChanged(value);
                }
            }
        } 
        #endregion

        private GameInfoMessage LastGame;

        public GameLauncherService(
            IDownloadService downloadService,
            IMapsService mapsService,
            ISessionService sessionService,
            IGamesService gamesService,
            ILogger<GameLauncherService> logger)
        {
            DownloadService = downloadService;
            MapsService = mapsService;
            Logger = logger;
            SessionService = sessionService;

            SessionService.GameLaunchDataReceived += OnGameLaunchDataReceived;
            GamesService = gamesService;
        }

        private void OnGameLaunchDataReceived(object sender, EventArgs<GameLaunchData> e)
        {
            var data = e.Arg;
        }

        public async Task JoinGame(GameInfoMessage game)
        {
            Logger.LogInformation($"Joining to the game '{game.title}' hosted by '{game.host}' on '{game.mapname}'");

            LastGame = game;

            // Check mods?
            // ...

            // Check map
            if (!ConfirmMap(game.Map.OriginalName))
            {
                Logger.LogWarning("Map {1} required to download", game.Map.OriginalName);
                // Run task for downloading
                await Task.Run(() => MapsService.Download(new($"https://content.faforever.com/maps/{game.Map.OriginalName}.zip")));

                // because it is .zip file, we cant just wait till model itself complete to download.
                // we have to wait the service to complete unzip process;
                MapsService.DownloadCompleted += OnRequiredMapDownloadCompleted;
                return;
            }
            Logger.LogInformation("Map confirmed!");

            // Check current patch
            var dataToDownload = await ConfirmPatch(game.FeaturedMod);
            if (dataToDownload.Length != 0)
            {
                // we have patch files to download

                //TODO We are downloading all files again, not optimized

                // Clear patch files with legacy
                //CopyOriginalBin();

                // Check for required patch files again
                //var data = await ConfirmPatch(game.FeaturedMod);

                // download required files
                var model = await DownloadPatchFiles(dataToDownload);

                model.Completed += OnPatchDownloadCompleted;

                return;
            }
            Logger.LogInformation("Patch confirmed");
        }

        private void OnPatchDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            var model = (DownloadViewModel)sender;
            model.Completed -= OnPatchDownloadCompleted;
            if (e.Cancelled) return;

            if (LastGame is not null)
            {
                JoinGame(LastGame);
            }
        }

        private void OnRequiredMapDownloadCompleted(object sender, string e)
        {
            // Check if this is required map
            if (LastGame is not null && LastGame.Map.OriginalName == e)
            {
                Logger.LogInformation($"Required map {e} is downloaded");
                ((IMapsService)sender).DownloadCompleted -= OnRequiredMapDownloadCompleted;

                // launch cycle again
                JoinGame(LastGame);
            }
        }

        private bool ConfirmMap(string name) => MapsService.CheckLocalMap(name) switch
        {
            Models.Enums.LocalMapState.NotExist => false,
            Models.Enums.LocalMapState.Older => false,
            Models.Enums.LocalMapState.Newest => false,
            Models.Enums.LocalMapState.Same => true,
            Models.Enums.LocalMapState.Unknown => false,
            _ => throw new NotImplementedException(),
        };

        private async Task<ApiFeaturedModFileData[]> ConfirmPatch(FeaturedMod featuredMod)
        {
            Logger.LogInformation($"Confirming patch for {featuredMod} game mod");
            WebRequest webRequest = WebRequest.Create($"https://api.faforever.com/featuredMods/{(int)featuredMod}/files/latest");

            using WebResponse response = await webRequest.GetResponseAsync();
            
            var result = await JsonSerializer.DeserializeAsync<ApiFeaturedModFileResults>(response.GetResponseStream());

            var localPath = App.GetPathToFolder(Folder.ProgramData);
            Logger.LogInformation($"Using {Folder.ProgramData}");

            Logger.LogInformation($"Local path: {localPath}");

            // Check MD5 of existed files. Clearing list of required files to download

            List<ApiFeaturedModFileData> data = new();

            Logger.LogInformation($"Files to check: {result.Data.Length}\nChecking files...");
            for (int i = 0; i < result.Data.Length; i++)
            {
                var item = result.Data[i];
                var file = localPath + item.Group + "\\";

                // TODO Move this checks of Bin / Gamedata
                if (!Directory.Exists(file))
                    Directory.CreateDirectory(file);

                file += item.Name;

                if (!ConfirmFile(file, item.MD5))
                {
                    data.Add(item);
                    Logger.LogWarning($"{i + 1}/{result.Data.Length} MD5 not confirmed {item.Name} ");
                }
                else
                {
                    Logger.LogInformation($"{i + 1}/{result.Data.Length} MD5 confirmed {item.Name}");
                }
            }

            if (data.Count > 0)
            {
                Logger.LogWarning($"Files to download {data.Count}/{result.Data.Length}");
            }

            return data.ToArray();
        }

        private async Task<DownloadViewModel> DownloadPatchFiles(ApiFeaturedModFileData[] data)
        {
            //if (!CopyOriginalBin())
            //{
            //    throw new Exception();
            //}

            var localPath = App.GetPathToFolder(Folder.ProgramData);
            var models = new DownloadItem[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                var item = data[i];

                models[i] = new(localPath + item.Group + "\\", item.Name, item.Url.AbsoluteUri);
            }

            return await DownloadService.DownloadAsync(models);
        }

        /// <summary>
        /// Returns true if exists and has same MD5
        /// </summary>
        /// <param name="file"></param>
        /// <param name="md5"></param>
        /// <returns></returns>
        private bool ConfirmFile(string file, string md5)
        {
            if (!File.Exists(file))
            {
                Logger.LogWarning($"File not exist {file}");
                return false;
            }
            var fileMD5 = Tools.CalculateMD5FromFile(file);
            if (fileMD5 != md5)
            {
                Logger.LogWarning($"MD5 didn`t match\n{file}\n{fileMD5} != {md5}");
                return false;
            }
            return true;
        }

        private async void OnDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            ((DownloadViewModel)sender).Completed -= OnDownloadCompleted;

            if (!e.Cancelled) await JoinGame(LastGame);
        }

        private bool CopyOriginalBin()
        {
            Logger.LogInformation("Copying original game files from Bin folder...");
            var pathToGame = Properties.Settings.Default.PathToGame;

            if (pathToGame is null)
            {
                Logger.LogError("No path to game folder");
                throw new ArgumentNullException(pathToGame);
            }

            if (!Directory.Exists(pathToGame))
            {
                Logger.LogError($"No game folder on {pathToGame}");
                throw new Exception("No game folder");
            }

            var pathToBin = pathToGame + "\\bin";

            if (!Directory.Exists(pathToBin))
            {
                throw new Exception("No bin folder");
            }

            var localPath = App.GetPathToFolder(Folder.ProgramData) + "bin";

            if (!Directory.Exists(localPath))
                Directory.CreateDirectory(localPath);

            Logger.LogInformation($"Target folder to copy: {localPath}");

            var files = Directory.GetFiles(pathToBin);
            Logger.LogInformation($"Files to copy: {files.Length}\nCopying files from Bin folder");
            for (int i = 0; i < files.Length; i++)
            {
                Logger.LogInformation($"Copying {files[i]}");
                File.Copy(files[i], files[i].Replace(pathToBin, localPath), true);
            }
            Logger.LogInformation("Copying completed");

            return true;
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


        public Task Join(GameInfoMessage game)
        {
            throw new NotImplementedException();
        }

        public Task HostAsync(string title, FeaturedMod mod, string visibility, string mapName, string password = null, bool isRehost = false)
        {
            throw new NotImplementedException();
        }
        private void OnStateChanged(GameLauncherState arg) => GameLauncherStateChanged?.Invoke(this, arg);
    }
}
