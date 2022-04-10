namespace beta.Models
{
    public class DownloadItem
    {
        public DownloadItem(string folderPath, string fileName, string url)
        {
            FolderPath = folderPath;
            FileName = fileName;
            Url = url;
        }

        public string FolderPath { get; }
        public string FileName { get; }
        public string Url { get; }
    }
}
