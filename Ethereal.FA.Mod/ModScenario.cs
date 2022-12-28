using NLua;

namespace Ethereal.FA.Mod
{
    /// <summary>
    /// 
    /// </summary>
    public class ModScenario
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scenarioFile"></param>
        /// <returns></returns>
        public static ModScenario FromFile(string scenarioFile)
        {
            if (!File.Exists(scenarioFile)) return null;
            var lua = new Lua();
            lua.DoString(scenarioFile);
            var scenario = new ModScenario();

            if (lua["name"] is string name) scenario.Name = name;
            if (lua["uid"] is string uid) scenario.Uid = uid;
            if (lua["description"] is string description) scenario.Description = description;
            if (lua["copyright"] is string copyright) scenario.Copyright = copyright;
            if (lua["author"] is string author) scenario.Author = author;
            if (lua["url"] is string url) scenario.Url = url;
            if (lua["icon"] is string icon) scenario.Icon = icon;
            if (lua["version"] is string version) scenario.Version = version;
            if (lua["exclusive"] is bool exclusive) scenario.IsExclusive = exclusive;
            if (lua["ui_only"] is bool ui_only) scenario.IsUI = ui_only;
            if (lua[""] is LuaTable table) scenario.Requires = GetList(table);
            if (lua[""] is LuaTable table2) scenario.Conflicts = GetList(table2);
            if (lua[""] is LuaTable table3) scenario.Before = GetList(table3);
            if (lua[""] is LuaTable table4) scenario.After = GetList(table4);

            return scenario;
        }

        private static string[] GetList(LuaTable table)
        {
            var data = new string[table.Keys.Count];
            for (int i = 1; i <= table.Keys.Count; i++)
            {
                data[i - 1] = (string)table[1];
            }
            return data;
        }

        /// <summary>
        /// mod name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// an ID every mod needs, you can generate a random one
        /// </summary>
        public string Uid { get; set; }
        /// <summary>
        /// use a versioning pattern of your choice, the current FAF mod vault will only display a single integer though
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// mention how you want copying to be treated
        /// </summary>
        public string Copyright { get; set; }
        /// <summary>
        /// mod description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Author name
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// if you have one, for example a forum thread that you could be making to show us your mod
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Preview logo
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// true|false : whether the mod should be selectable in the ingame lobby
        /// </summary>
        public bool IsSelectable { get; set; }
        /// <summary>
        /// true|false
        /// </summary>
        public bool IsEnabled { get; set; }
        /// <summary>
        /// true|false:
        /// </summary>
        public bool IsExclusive { get; set; }
        /// <summary>
        /// true|false: whether all players need to mod for the game to run.SIM mods are needed by everyone, UI mods not
        /// </summary>
        public bool IsUI { get; set; }
        /// <summary>
        /// table of requirements for your mod(for example "common mod tools"), enter the UIDs here
        /// </summary>
        public string[] Requires { get; set; }
        /// <summary>
        /// table of conflicts, this way the lobby will warn the player that they don't work together
        /// </summary>
        public string[] Conflicts { get; set; }
        /// <summary>
        /// mods that will have to be hooked in before your mod is hooked
        /// </summary>
        public string[] Before { get; set; }
        /// <summary>
        /// mods that have to be hooked in after your mod
        /// </summary>
        public string[] After { get; set; }
    }
}
