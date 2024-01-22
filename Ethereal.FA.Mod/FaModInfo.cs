using NLua;

namespace Ethereal.FA.Mod
{
    /// <summary>
    /// 
    /// </summary>
    public class FaModInfo
    {
        /// <summary>
        /// Required name to use for this mod.This is what is displayed in the Lobby, after undergoing
        /// title-casing.Do NOT add version information here - keep this the exactly same when you
        /// update what you consider to be the same mod as the FAF client will uses this to auto-update
        /// mods.
        /// </summary>
        /// <example>Happy mod</example>
        public string Name { get; set; }
        /// <summary>
        /// Required integer version to use for this mod (please only increment) to indicate the most
        /// current version. Decimals are not supported. If updating the mod, make sure to change the
        /// <see cref="Uid"/> (as the FAF client requires that) and leave the name exactly the same.
        /// </summary>
        /// <example>4</example>
        public int Version { get; set; }
        /// <summary>
        /// Optional copyright info. Invisible in-game but shown in the FAF client.
        /// </summary>
        /// <example>Copyright © 2023, Someone</example>
        public string? Copyright { get; set; }
        /// <summary>
        /// Optional text displayed in the mods manager and FAF client
        /// </summary>
        /// <example>A long description of happy mod and why it will make you happy</example>
        public string Description { get; set; }
        /// <summary>
        /// Required name(s) of author(s). Shows up as `"UNKNOWN"` if left blank. Will begin to be
        /// truncated in-game if over 20 characters. Make sure this reflects all contributors whose work
        /// is present.
        /// </summary>
        /// <example>Joe</example>
        public string Author { get; set; }
        /// <summary>
        /// Optional URL to anywhere author likes. Will show up in the mods manager if valid.
        /// </summary>
        /// <example>https://www.faforever.com</example>
        public string Url { get; set; }
        /// <summary>
        /// Optional URL to anywhere author likes. Will show up in the mods manager if valid.
        /// </summary>
        /// <example>https://github.com/FAForever</example>
        public string GithubUrl { get; set; }
        /// <summary>
        /// Uniquely identifies this mod. This defaults to the mod's name, but that's not a very safe way
        /// to keep mods differentiated. It's recommended to use a GUID, or Guaranteed Unique ID.
        /// Every new version of a mod should get a new GUID so that mods which rely on the old version
        /// can select it appropriately, and so that the FAF client can keep track.<br/>
        /// The following webpages allow you to generate a GUID online (as of June 2023)<br/>
        /// <see href="http://www.somacon.com/p113.php"/><br/>
        /// <see href="http://www.famkruithof.net/uuid/uuidgen"/><br/>
        /// </summary>
        public string Uid { get; set; }
        /// <summary>
        /// Flag for whether to list this mod in the mods manager window, where the player can select it.
        /// An example where you would set it to false is library mod supplying functions, textures, and
        /// props for use by other mods. Those mods indicate that they require this one, and it is
        /// automatically loaded when they are.Note that unselectable mods can still be loaded.
        /// </summary>
        public bool Selectable { get; set; }
        /// <summary>
        /// Setting enabled to false causes a mod to never be loaded. Provides an easy way to disable a
        /// mod during development or for dummy mod packs that nest other mods inside for distributio
        /// The default is true.
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// A simple form of preventing conflicting mods. If a mod has `exclusive`, then no other mod can
        /// be active.The default is false (please leave it like that). Note that this precludes the use
        /// of `conflicts`.
        /// </summary>
        public bool Exclusive { get; set; }
        /// <summary>
        /// Setting to true means this mod will only affect the user interface's Lua state. If this flag
        /// is false (the default), all players in a multiplayer game must have it in order for anyone to
        /// use it.If true, one player can have it active independently of the other players.
        /// </summary>
        public bool UIOnly { get; set; }
        /// <summary>
        /// Path of the icon to use for this mod in the mods manager window.<br/>
        /// The game defaults to `"/textures/ui/common/dialogs/mod-manager/generic-icon_bmp.dds"`.
        /// The FAF client cannot read `.dds` so please use `.png` or `.jpg` as both the game and client understand those.
        /// Note that this is unrelated to the `mod_icons.lua` file explained below.
        /// </summary>
        public string IconPath { get; set; }
        /// <summary>
        /// Optionally indicates that this mod only works if another mod is also present. Does not affect
        /// mod loading order.
        /// It might be nice if you add a comment after each UID to denote the name of the mod so mere
        /// mortals can maintain your list :)
        /// </summary>
        public List<string> Requires { get; set; } = new();
        /// <summary>
        /// Optional table which will map the required UID's to friendly names so the user of the mod can
        /// determine what they are missing.
        /// </summary>
        public Dictionary<string, string> RequiresNames { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// Optionally indicates any other mods that this mod is known to conflict with; the game will
        /// refuse to enable both of them at the same time.You do not need to add old versions of your
        /// mod here (unless you'd like to be extra careful); the client determines mods with the same
        /// name to be the same mod and auto-updates them. Same format as `requires`.
        /// </summary>
        public List<string> Conflicts { get; set; } = new();
        /// <summary>
        /// Optional list of other mod UID's. If this mod happens to be active at the same time as any of
        /// the named other mods, it will be applied before them. Same format as `requires`.
        /// </summary>
        public List<string> Before { get; set; } = new();
        /// <summary>
        /// Optional list of other mod UID's. If this mod happens to be active at the same time as any of
        /// the named other mods, it will be applied after them. If you do not supply an `after` list,
        /// the `requires` list will be used in its place (if present). Same format as `requires`.
        /// </summary>
        public List<string> After { get; set; } = new();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static FaModInfo FromFile(string file)
        {
            if (!File.Exists(file)) return null;
            using var lua = new Lua();
            lua.DoFile(file);
            var mod = new FaModInfo();
            if (lua[FaModInfoDefaults.Name] is string name) mod.Name = name;
            if (lua[FaModInfoDefaults.Uid] is string uid) mod.Uid = uid;
            if (lua[FaModInfoDefaults.Description] is string description) mod.Description = description;
            if (lua[FaModInfoDefaults.Copyright] is string copyright) mod.Copyright = copyright;
            if (lua[FaModInfoDefaults.Author] is string author) mod.Author = author;
            if (lua[FaModInfoDefaults.Url] is string url) mod.Url = url;
            if (lua[FaModInfoDefaults.Github] is string githubUrl) mod.GithubUrl = githubUrl;
            if (lua[FaModInfoDefaults.Icon] is string icon) mod.IconPath = icon;
            if (lua[FaModInfoDefaults.Version] is int version)
                mod.Version = version;
            else if (lua[FaModInfoDefaults.Version] is string versionStr && int.TryParse(versionStr, out var parsedVersionNumber))
                mod.Version = parsedVersionNumber;
            if (lua[FaModInfoDefaults.Exclusive] is bool exclusive) mod.Exclusive = exclusive;
            if (lua[FaModInfoDefaults.UIOnly] is bool ui_only) mod.UIOnly = ui_only;
            if (lua[FaModInfoDefaults.Requires] is LuaTable requires) mod.Requires = requires.ToList();
            if (lua[FaModInfoDefaults.RequiresNames] is LuaTable requiresNames) mod.Requires = requiresNames.ToList();
            if (lua[FaModInfoDefaults.Conflicts] is LuaTable conflicts) mod.Conflicts = conflicts.ToList();
            if (lua[FaModInfoDefaults.Before] is LuaTable before) mod.Before = before.ToList();
            if (lua[FaModInfoDefaults.After] is LuaTable after) mod.After = after.ToList();
            return mod;
        }
    }
}
