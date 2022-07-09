namespace beta.Models.Scmap
{
    /// <summary>
    /// 
    /// </summary>
    public class ScmapEnvCube
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// 
        /// </summary>
        public string File { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="file"></param>
        public ScmapEnvCube(string name, string file)
        {
            Name = name;
            File = file;
        }
    }
}
