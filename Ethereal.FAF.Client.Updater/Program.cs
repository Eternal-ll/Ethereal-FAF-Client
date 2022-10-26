
using Ethereal.FAF.Client.Updater;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Text.Json;

var branch = "develop";
if (args.Contains("master")) branch = "master";
Console.WriteLine("Branch: {0}", branch);
var updateUrl = $"https://raw.githubusercontent.com/Eternal-ll/Ethereal-FAF-Client/{branch}/update.json";
Console.WriteLine("Retrieving update.json from {0}", updateUrl);
using var client = new HttpClient();
var updateData = await client.GetFromJsonAsync<Update>(updateUrl);
Console.WriteLine(JsonSerializer.Serialize(updateUrl));
Console.WriteLine("Downloading update...");
var updateArchive = "update.rar";
using var stream = await client.GetStreamAsync(updateData.UpdateUrl);
using var file = new FileStream(updateArchive, FileMode.OpenOrCreate);
stream.CopyTo(file);
stream.Flush();
Console.WriteLine("Extracting update...");
ZipFile.ExtractToDirectory(updateArchive, "/");
Console.WriteLine("Done");
Process.Start("Ethereal.FAF.UI.Client.exe");