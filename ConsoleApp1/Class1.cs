using beta.Models.API;
using beta.Models.API.Base;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Player : ApiUniversalWithRelations2
    {

    }
    public class ApiUniversalResults
    {
        [JsonPropertyName("data")]
        public Player[] Data { get; set; }

        [JsonPropertyName("meta")]
        public ApiVaultMeta Meta { get; set; }
    }

    static public class test
    {
        private static StreamWriter sr;
        public static async Task Test()
        {
            var uri = "https://api.faforever.com/data/player?fields[player]=avatarAssignments&page[totals]=None&";

            WebRequest webRequest = WebRequest.Create(uri);
            var response = await webRequest.GetResponseAsync();

            sr = new(new FileStream("test.txt", FileMode.Create));

            var result = await JsonSerializer.DeserializeAsync<ApiUniversalResults>(response.GetResponseStream());
            await Parse(result.Data);

            var meta = result.Meta;
            Console.WriteLine($"Page: {1} / {meta.Page.AvaiablePagesCount}");
            for (int i = 2; i <= meta.Page.AvaiablePagesCount; i++)
            {
                var uri2 = "https://api.faforever.com/data/player?fields[player]=avatarAssignments&page[totals]=None&"
                    + $"&page[number]={i}";

                webRequest = WebRequest.Create(uri2);
                response = await webRequest.GetResponseAsync();
                result = await JsonSerializer.DeserializeAsync<ApiUniversalResults>(response.GetResponseStream());
                await Parse(result.Data);
                Console.WriteLine($"Page: {i} / {meta.Page.AvaiablePagesCount}");
            }
            sr.Close();

        }
        private static async Task Parse(Player[] players)
        {
            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                await WriteLine(sr, player);
            }
        }
        private static async Task WriteLine(StreamWriter sr, Player player)
        {
            var g = player.Relations is null ? 0 : player.Relations["avatarAssignments"].Data.Count;
            await sr.WriteLineAsync($"{player.Id},{g.ToString()}");
        }
    }
}
