namespace Ethereal.FAF.Client.Updater
{
    public class Update
    {
        public string Version { get; set; }
        public bool ForceUpdate { get; set; }
        public string UpdateUrl { get; set; }
        public string DownloadUrl { get; set; }
    }
}
