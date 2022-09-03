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


        /// <summary>
        /// produce help message
        /// </summary>
        /// <returns></returns>
        public const string Help = "--help";
        /// <summary>
        /// list styles
        /// </summary>
        /// <returns></returns>
        public const string Styles = "--styles";
        /// <summary>
        /// list styles and weights based on the given parameters
        /// </summary>
        public const string Weights = "--weights";
        /// <summary>
        /// list biomes
        /// </summary>
        /// <param name="biomes"></param>
        /// <returns></returns>
        public const string Biomes = "--biomes";

        /// <summary>
        /// optional, set the map style for the generated map
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        public static string SetStyle(string style) => $"--style {style} ";
        /// <summary>
        /// optional, set the spawn count for the generated map
        /// </summary>
        /// <param name="spaw"></param>
        /// <returns></returns>
        public static string SetSpawnCount(int count) => $"--spawn-count {count} ";
        /// <summary>
        /// optional, set the number of teams for the generated map(0 is no teams asymmetric)
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string SetTeamsCount(int count) => $"--num-teams {count} ";
        /// <summary>
        /// optional, set the land density for the generated map
        /// </summary>
        /// <param name="density"></param>
        /// <returns></returns>
        public static string SetLandDensity(double density) => $"--land-density {density} ";
        /// <summary>
        /// optional, set the plateau density for the generated map
        /// </summary>
        /// <param name="platea"></param>
        /// <returns></returns>
        public static string SetPlateauDensity(double density) => $"--plateau-density {density} ";
        /// <summary>
        /// optional, set the mountain density for the generated map
        /// </summary>
        /// <param name="mountai"></param>
        /// <returns></returns>
        public static string SetMountainDensity(double density) => $"--mountain-density {density} ";
        /// <summary>
        /// optional, set the ramp density for the generated map
        /// </summary>
        /// <param name="ram"></param>
        /// <returns></returns>
        public static string SetRampDensity(double density) => $"--ramp-density {density} ";
        /// <summary>
        /// optional, set the reclaim density for the generated map
        /// </summary>
        /// <param name="density"></param>
        /// <returns></returns>
        public static string SetReclaimDensity(double density) => $"--reclaim-density {density} ";
        /// <summary>
        /// optional, set the mex density for the generated map
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static string SetMexsDensity(double density) => $"--mex-density {density} ";
        /// <summary>
        /// optional, set the mex count per player for the generated map
        /// </summary>
        /// <param name="perPlayer"></param>
        /// <returns></returns>
        public static string SetMexsCountPerPlayer(int perPlayer) => $"--mex-count {perPlayer} ";
        /// <summary>
        /// optional, set the map size(5km = 256, 10km = 512, 20km = 1024)
        /// </summary>
        /// <param name="ma"></param>
        /// <returns></returns>
        public static string SetMapSie(int size) => $"--map-size {size} ";
        /// <summary>
        /// optional, set the biome
        /// </summary>
        /// <param name="biome"></param>
        /// <returns></returns>
        public static string SetBiome(string biome) => $"--biome {biome} ";
        /// <summary>
        /// optional, set map to tournament style which will remove the preview.png and add time of original generation to map
        /// </summary>
        /// <param name="tournamen"></param>
        /// <returns></returns>
        public const string TournamentStyle = "--tournament-style ";
        /// <summary>
        /// optional, set map to blind style which will apply tournament style and remove in game lobby preview
        /// </summary>
        /// <param name="blind"></param>
        /// <returns></returns>
        public const string Blind = "--blind ";
        /// <summary>
        /// optional, set map to unexplored style which will apply tournament and blind style and add unexplored fog of war
        /// </summary>
        /// <param name="unexplored"></param>
        /// <returns></returns>
        public const string Unexplored = "--unexplored ";
        /// <summary>
        /// optional, turn on debugging options
        /// </summary>
        /// <param name="debug"></param>
        /// <returns></returns>
        public const string Debug = $"--debug ";
        /// <summary>
        /// optional, turn on visualization for all masks
        /// </summary>
        /// <param name="visualize"></param>
        /// <returns></returns>
        public const string Visualize = "--visualize ";
        /// <summary>
        /// optional, number of maps to generate
        /// </summary>
        /// <param name="nu"></param>
        /// <returns></returns>
        public static string SetCountToGenerate(int count) => $"--num-to-gen {count} ";
        /// <summary>
        /// optional, path to dump previews to
        /// </summary>
        /// <param name="previe"></param>
        /// <returns></returns>
        public static string SetDumpPreviewTo(string path) => $"--preview-path {path} ";
    }
}
