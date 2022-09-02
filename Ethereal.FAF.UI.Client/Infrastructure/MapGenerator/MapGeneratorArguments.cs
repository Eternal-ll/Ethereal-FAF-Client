namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal static class MapGeneratorArguments
    {
        /// <summary>
        /// set the seed for the generated map
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static string SetSeed(string seed) => $"--seed {seed} ";
        /// <summary>
        /// set the target folder for the generated map
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string SetFolderPath(string path) => $"--folder-path \"{path}\" ";
        public static string SetMapName(string map) => $"--map-name {map} ";
        /// <summary>
        /// path to dump previews to
        /// </summary>
        public static string SetPreviewPath(string path) => $"--preview-path {path}";
    }
}
