using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services
{
    public class MapsService : IMapsService
    {
        private readonly ICacheService CacheService;

        private readonly List<string> LocalMaps = new();
        private readonly List<GameMap> GameMaps = new();
        private readonly FileSystemWatcher LocalWatcher;

        private string path = @"C:\Users\Eternal\Documents\My Games\Gas Powered Games\Supreme Commander Forged Alliance\Maps\";

        public string[] LocalPaths;
        private string _PathToLegacyMaps;
        private string PathToLegacyMaps
        {
            get
            {
                var pathToGame = Properties.Settings.Default.PathToGame;
                if (pathToGame != null)
                {
                    return pathToGame + @"\maps\";
                }
                return null;
            }
        }
        public MapsService(ICacheService cacheService)
        {
            CacheService = cacheService;

            LocalPaths = new string[]
            {
                @"C:\Users\Eternal\Documents\My Games\Gas Powered Games\Supreme Commander Forged Alliance\Maps\"
            };

            var path = Environment.ExpandEnvironmentVariables(Properties.Settings.Default.PathToMaps);
            var folders = Directory.GetDirectories(path);
            for (int j = 0; j < folders.Length; j++)
            {
                var folder = folders[j].Split('\\')[^1];
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
        public Dictionary<string, string> GetMapScenario(string mapName, bool isLegacy = false)
        {
            var localMaps = LocalMaps;
            string pathToMaps;

            string formattedMapName = string.Empty;

            if (isLegacy && PathToLegacyMaps != null)
            {
                pathToMaps = PathToLegacyMaps;
            }
            else
            {
                pathToMaps = Properties.Settings.Default.PathToMaps;
                for (int i = 0; i < localMaps.Count; i++)
                {
                    if (i + 1 == localMaps.Count)
                    {
                        return null;
                    }

                    if (localMaps[i].Equals(mapName, StringComparison.OrdinalIgnoreCase))
                    {
                        formattedMapName = mapName.Substring(0, mapName.IndexOf('.'));
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
                    while ((line = reader.ReadLine()) != null)
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
                        while ((line = reader.ReadLine()) != null)
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
            var mapName = uri.Segments[^1].Substring(0, uri.Segments[^1].Length - 4);
            GameMap map = new()
            {
                IsLegacy = IsLegacyMap(mapName)
            };

            //neroxis_map_generator_1.8.5_c6gjzaqfmuove_aida.png
            //https://content.faforever.com/maps/previews/small/neroxis_map_generator_1.8.5_c6gjzaqfmuove_aida.png

            if (uri.Segments[^1].StartsWith("neroxis"))
            {
                App.Current.Dispatcher.Invoke(() =>
                map.SmallPreview = App.Current.Resources["MapGenIcon"] as ImageSource,
                System.Windows.Threading.DispatcherPriority.Background);
            }
            else
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    map.SmallPreview = GetMapPreview(uri);
                    map.Scenario = attachScenario ? GetMapScenario(mapName, map.IsLegacy) : null;
                },System.Windows.Threading.DispatcherPriority.Background);
            }
            return map;
        }

        public BitmapImage GetMapPreview(Uri uri, Folder folder = Folder.MapsSmallPreviews) => CacheService.GetImage(uri, folder);

        //https://api.faforever.com/data/map
        //include=latestVersion,reviewsSummary&page[size]=50&page[number]=1&page[totals]=None
        //https://api.faforever.com/data/map?include=latestVersion,reviewsSummary&page%5Bsize%5D=50&page%5Bnumber%5D=1&page%5Btotals%5D=None
    }
}
