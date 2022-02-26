using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services
{
    public struct Map
    {
        public Dictionary<string, string> Scenario { get; set; }
        public ImageSource MapPreviewSource { get; set; }
        public string Size
        {
            get
            {
                if (Scenario == null) return null;

                if (Scenario.TryGetValue("size", out var value))
                {
                    return value;
                }
                return null;
            }
        }
    }
    public class MapsService : IMapsService
    {
        private readonly ICacheService CacheService;

        private readonly List<string> LocalMaps = new();
        private readonly FileSystemWatcher LocalWatcher;

        private string path = @"C:\Users\Eternal\Documents\My Games\Gas Powered Games\Supreme Commander Forged Alliance\Maps\";
        public string[] LocalPaths;
        public MapsService(ICacheService cacheService)
        {
            CacheService = cacheService;


            LocalPaths = new string[]
            {
                @"C:\Supreme Commander Forged Alliance\maps\",
                @"C:\Users\Eternal\Documents\My Games\Gas Powered Games\Supreme Commander Forged Alliance\Maps\"
            };

            for (int i = 0; i < LocalPaths.Length; i++)
            {
                var path = LocalPaths[i];
                var folders = Directory.GetDirectories(path);
                for (int j = 0; j < folders.Length; j++)
                {
                    var folder = folders[j].Split('\\')[^1];
                    LocalMaps.Add(folder);
                }
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

        private void OnDeletingLocalMap(object sender, FileSystemEventArgs e)
        {
            LocalMaps.Remove(e.Name);
        }

        private void OnNewLocalMap(object sender, FileSystemEventArgs e)
        {
            LocalMaps.Add(e.Name); 
        }

        public void AttachMapScenario(ref Map map)
        {
            //throw new NotImplementedException();
        }
        public Dictionary<string, string> GetMapScenario(string mapName)
        {
            mapName = mapName.Substring(0, mapName.Length - 4);

            var localMaps = LocalMaps;
            var pathToMaps = LocalPaths[1];
            if (mapName.Contains("scmp", StringComparison.OrdinalIgnoreCase))
                pathToMaps = LocalPaths[0];
            for (int i = 0; i < localMaps.Count; i++)
            {
                if (localMaps[i].Equals(mapName, StringComparison.OrdinalIgnoreCase))
                {
                    if (mapName.Contains('.'))
                        mapName = mapName.Substring(0, mapName.IndexOf('.'));
                    var scenarioName = mapName + "_scenario.lua";
                    var saveName = mapName + "_save.lua";

                    var pathToScenario = localMaps[i] + "\\" + scenarioName;
                    var pathToSave = localMaps[i] + "\\" + saveName;
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
                    else return null;
                }
            }
            return null;
        }

        public void Download(Uri url)
        {
            throw new NotImplementedException();
        }

        public Map GetMap(Uri uri, bool attachScenario = true)
        {
            Map map = new();

            //neroxis_map_generator_1.8.5_c6gjzaqfmuove_aida.png
            //https://content.faforever.com/maps/previews/small/neroxis_map_generator_1.8.5_c6gjzaqfmuove_aida.png

            if (uri.Segments[^1].StartsWith("neroxis"))
            {
                App.Current.Dispatcher.Invoke(() =>
                map.MapPreviewSource = App.Current.Resources["MapGenIcon"] as ImageSource,
                System.Windows.Threading.DispatcherPriority.Background);
            }
            else
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    map.MapPreviewSource = GetMapPreview(uri);
                    map.Scenario = attachScenario ? GetMapScenario(uri.Segments[^1]) : null;
                },System.Windows.Threading.DispatcherPriority.Background);
            }
            return map;
        }

        public BitmapImage GetMapPreview(Uri uri, Folder folder = Folder.MapsSmallPreviews) => CacheService.GetImage(uri, folder);
    }
}
