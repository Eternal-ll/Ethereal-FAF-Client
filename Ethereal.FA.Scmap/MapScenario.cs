using NLua;
using System.Text.RegularExpressions;

namespace Ethereal.FA.Scmap
{
    public class MapScenario
    {
        public string Name { get; set; }
        public string Description { get; set; } 
        public string Preview { get; set; }
        public int MapVersion { get; set; }
        public bool IsAdaptiveMap { get; set; }
        public string Type { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public string PathToMap { get; set; }
        public string PathToSave { get; set; }
        public string PathToScript { get; set; }

        public double NoRushRadius { get; set; }

        public bool Starts { get; set; }

        public List<string> Armies { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scenario"></param>
        /// <returns></returns>
        public static MapScenario FromFile(string file)
        {
            if (!File.Exists(file)) return null;
            var scenario = new MapScenario();
            var test = File.ReadAllText(file);
            var regex = new Regex(@"STRING\( (.*) \)");
            var match = regex.Match(test);
            test = regex.Replace(test, match.Groups[^1].Value);
            using Lua lua = new Lua();
            lua.DoString(test);
            if (lua["ScenarioInfo.name"] is string name) scenario.Name = name;
            if (lua["ScenarioInfo.description"] is string description)
            {

                scenario.Description = description;
            }
            if (lua["ScenarioInfo.preview"] is string preview) scenario.Preview = preview;
            if (lua["ScenarioInfo.map_version"] is double version) scenario.MapVersion = (int)version;
            if (lua["ScenarioInfo.AdaptiveMap"] is bool adaptive) scenario.IsAdaptiveMap = adaptive;
            if (lua["ScenarioInfo.type"] is string type) scenario.Type = type;
            if (lua["ScenarioInfo.size"] is LuaTable sizes && sizes.Keys.Count == 2 && sizes[1] is long width && sizes[2] is long height)
            {
                scenario.Width = (double)width;
                scenario.Height = (double)height;
            }
            if (lua["ScenarioInfo.starts"] is bool starts) scenario.Starts = starts;
            if (lua["ScenarioInfo.map"] is string map) scenario.PathToMap = map;
            if (lua["ScenarioInfo.save"] is string save) scenario.PathToSave = save;
            if (lua["ScenarioInfo.script"] is string script) scenario.PathToScript = script;
            if (lua["ScenarioInfo.norushradius"] is double norush) scenario.NoRushRadius = norush;
            if (lua["ScenarioInfo.Configurations"] is LuaTable configurations)
            {
                foreach (KeyValuePair<object, object> item in configurations)
                {
                    if (item.Value is LuaTable config)
                    {
                        if (config["teams"] is LuaTable teams)
                        {
                            if (teams[1] is LuaTable data)
                            {
                                if (data["armies"] is LuaTable armiesTable)
                                {
                                    var armies = new List<string>();
                                    foreach (var army in armiesTable.Values)
                                    {
                                        armies.Add((string)army);
                                    }
                                    scenario.Armies = armies;
                                }
                            }
                        }
                    }
                }
            }
            lua.Dispose();
            return scenario;
        }
    }
}
