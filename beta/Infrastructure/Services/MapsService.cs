﻿using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using beta.Properties;
using beta.Models.Server;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    public class MapsService : IMapsService
    {
        private readonly ICacheService CacheService;
        private readonly IApiService ApiService;

        private readonly List<string> LocalMaps = new();
        private readonly Dictionary<string, GameMap> CachedMaps = new();

        private readonly FileSystemWatcher LocalWatcher;

        private string PathToLegacyMaps => Settings.Default.PathToGame is not null ? Settings.Default.PathToGame + @"\maps\" : null;
        public MapsService(ICacheService cacheService, IApiService apiService)
        {
            CacheService = cacheService;
            ApiService = apiService;

            var path = App.GetPathToFolder(Folder.Maps);
            var maps = Directory.GetDirectories(path);
            for (int j = 0; j < maps.Length; j++)
            {
                var folder = maps[j].Split('\\')[^1];
                LocalMaps.Add(folder);
            }

            LocalWatcher = new()
            {
                Path = path,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.DirectoryName,
            };

            LocalWatcher.Created += OnNewLocalMap;
            LocalWatcher.Deleted += OnDeletingLocalMap;
        }   

        public LocalMapState CheckLocalMap(string name)
        {
            if (name is null) return LocalMapState.Unknown;

            int? version = null;
            var data = name.Split('.');
            if (data.Length > 1)
            {
                if (int.TryParse(data[1], out var v))
                {
                    version = v;
                }
                name = data[0];
            }

            var localMaps = LocalMaps;
            for (int i = 0; i < localMaps.Count; i++)
            {
                var local = localMaps[i];
                data = local.Split('.');

                // TODO fix... Maps with upper letter
                if (!name.Equals(data[0], StringComparison.OrdinalIgnoreCase)) continue;

                if (data.Length > 1)
                {
                    if (int.TryParse(data[1], out var v))
                    {
                        if (version is null)
                        {
                            return LocalMapState.Older;
                        }

                        if (v > version)
                        {
                            return LocalMapState.Older;
                        }
                        else
                        {
                            return LocalMapState.Newest;
                        }
                    }
                }
                return LocalMapState.Same;

            }
            return LocalMapState.NotExist;
        }
        public bool IsLegacyMap(string name) => Enum.IsDefined(typeof(LegacyMap), name.ToUpper());

        private void OnDeletingLocalMap(object sender, FileSystemEventArgs e)
        {
            LocalMaps.Remove(e.Name);
        }

        private void OnNewLocalMap(object sender, FileSystemEventArgs e)
        {
            LocalMaps.Add(e.Name);
        }

        public void AttachMapScenario(GameMap map)
        {
            //throw new NotImplementedException();
        }
        public string[] GetLocalMaps() => LocalMaps.ToArray();
        public Dictionary<string, string> GetMapScenario(string mapName, bool isLegacy = false)
        {
            var localMaps = LocalMaps;
            string pathToMaps;

            string formattedMapName = string.Empty;

            // Legacy maps stored in vaults, so this is mostly useless code
            // we need to copy original maps to local maps folder as example
            if (isLegacy && PathToLegacyMaps is not null)
            {
                pathToMaps = PathToLegacyMaps;
            }
            else
            {
                pathToMaps = App.GetPathToFolder(Folder.Maps);
                for (int i = 0; i < localMaps.Count; i++)
                {
                    if (i + 1 == localMaps.Count)
                    {
                        return null;
                    }
                    if (localMaps[i].Equals(mapName, StringComparison.OrdinalIgnoreCase))
                    {
                        formattedMapName = mapName.Split('.')[0];
                        break;
                    }
                }
            }

            string scenarioName;
            string saveName;
            if (formattedMapName.Length > 0)
            {
                scenarioName = formattedMapName + "_scenario.lua";
                saveName = formattedMapName + "_save.lua";
            }
            else
            {
                scenarioName = mapName + "_scenario.lua";
                saveName = mapName + "_save.lua";
            }

            var pathToScenario = mapName + "\\" + scenarioName;
            var pathToSave = mapName + "\\" + saveName;

            if (File.Exists(pathToMaps + pathToScenario))
            {
                Dictionary<string, string> scenario = new()
                {
                    ["name"] = "",
                    ["type"] = "",
                    ["size"] = "",
                    ["map_version"] = "", // comes with game_info from server
                    ["AdaptiveMap"] = "False",
                    ["Mass"] = "",
                    ["Hydrocarbon"] = "",
                    ["description"] = "",
                };

                int counter = scenario.Keys.Count - 2;

                using (var reader = new StreamReader(pathToMaps + pathToScenario))
                {
                    string line;
                    while ((line = reader.ReadLine()) is not null )
                    {
                        if (counter == 0) break;

                        var data = line.Split('=');
                        if (data.Length == 2 && data[1].Length > 2)
                        {
                            data[0] = data[0].Trim();
                            data[1] = data[1].Trim();
                            data[1] = data[1].Substring(0, data[1].Length - 1);
                            if (data[1].Length > 1 && (data[1][0] == '{' || data[1][0] == '\'' || data[1][0] == '\"'))
                            {
                                data[1] = data[1].Substring(1, data[1].Length - 2);
                            }
                            if (scenario.ContainsKey(data[0]))
                            {
                                scenario[data[0]] = data[1];
                                counter--;
                            }
                        }
                    }
                }

                if (File.Exists(pathToMaps + pathToScenario))
                    using (var reader = new StreamReader(pathToMaps + pathToSave))
                    {
                        string line;
                        int massCounter = 0;
                        int hydroCounter = 0;
                        while ((line = reader.ReadLine()) is not null)
                        {
                            if (line.Contains("\'Mass\'"))
                                massCounter++;
                            if (line.Contains("\'Hydrocarbon\'"))
                                hydroCounter++;
                        }
                        scenario["Mass"] = massCounter.ToString();
                        scenario["Hydrocarbon"] = hydroCounter.ToString();
                    }

                return scenario;
            }

            return null;
        }

        public void Download(Uri url)
        {
            throw new NotImplementedException();
        }

        public GameMap GetMap(Uri uri, bool attachScenario = true)
        {
            var mapName = uri.Segments[^1][0..^4];

            var isLegacyMap = IsLegacyMap(mapName);

            var cachedMaps = CachedMaps;
            if (cachedMaps.TryGetValue(mapName, out var cachedMap))
            {
                return cachedMap;
            }

            //neroxis_map_generator_1.8.5_c6gjzaqfmuove_aida.png
            //https://content.faforever.com/maps/previews/small/neroxis_map_generator_1.8.5_c6gjzaqfmuove_aida.png

            GameMap gameMap = new()
            {
                IsLegacy = isLegacyMap,
                OriginalName = mapName
            };

            if (uri.Segments[^1].StartsWith("neroxis"))
            {
                gameMap.ImageSource = new byte[1];
                return gameMap;
            }

            Task.Run(async () =>
            {
                gameMap.ImageSource = await CacheService.GetBitmapSource(uri, Folder.MapsSmallPreviews);
                gameMap.Scenario = attachScenario ? GetMapScenario(mapName, isLegacyMap) : null;
            });

            CachedMaps.Add(mapName, gameMap);

            return gameMap;
        }

        // UNUSED
        public Map GetMap(Uri uri, PreviewType previewType, bool attachScenario = true) => previewType switch
        {
            PreviewType.Coop => new CoopMap(uri.Segments[^1]),
            PreviewType.Neroxis => new NeroxisMap(uri.Segments[^1]),
            _ => GetMap(uri, attachScenario),
        };

        public BitmapImage GetMapPreview(Uri uri, Folder folder = Folder.MapsSmallPreviews) => CacheService.GetImage(uri, folder);

        //https://api.faforever.com/data/map
        //include=latestVersion,reviewsSummary&page[size]=50&page[number]=1&page[totals]=None
        //https://api.faforever.com/data/map?include=latestVersion,reviewsSummary&page%5Bsize%5D=50&page%5Bnumber%5D=1&page%5Btotals%5D=None
    }
}
