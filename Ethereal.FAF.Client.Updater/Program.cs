
using Ethereal.FAF.Client.Updater;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

Process[] localByName = Process.GetProcessesByName("Ethereal.FAF.UI.Client");
foreach (Process p in localByName)
{
    Console.WriteLine("Killing Ethereal FAF client");
    p.Kill();
}

var branch = "develop";
if (args.Contains("master")) branch = "master";
Console.WriteLine("Branch: {0}", branch);
var updateUrl = $"https://raw.githubusercontent.com/Eternal-ll/Ethereal-FAF-Client/{branch}/update.json";
Console.WriteLine("Retrieving update.json from {0}", updateUrl);
using var client = new HttpClient();
client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
{
    NoCache = true
};
var updateData = await client.GetFromJsonAsync<Update>(updateUrl);
Console.WriteLine(JsonSerializer.Serialize(updateUrl));
Console.WriteLine("Downloading update...");
var updateArchive = "update.rar";
using var stream = await client.GetStreamAsync("https://github.com/Eternal-ll/Ethereal-FAF-Client/releases/latest/download/update.zip");
using var file = new FileStream(updateArchive, FileMode.OpenOrCreate);
stream.CopyTo(file);
stream.Flush();
stream.Close();
file.Flush();
file.Close();
Console.WriteLine("Extracting update...");
ZipFile.ExtractToDirectory(updateArchive, Directory.GetCurrentDirectory(), true);
Console.WriteLine("Removing archive");
File.Delete(updateArchive);
UserSettings.Update("Client:Version", updateData.Version);
UserSettings.Update("Client:Updated", true, "appsettings.user.json");
Process.Start(new ProcessStartInfo()
{
    FileName = "Ethereal.FAF.UI.Client.exe"
});
Environment.Exit(0);