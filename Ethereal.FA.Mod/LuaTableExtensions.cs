﻿using NLua;
using System.Collections.Generic;

namespace Ethereal.FA.Mod
{
    internal static class LuaTableExtensions
    {
        public static List<string> ToList(this LuaTable table)
        {
            var data = new List<string>();
            foreach (var item in table.Values)
            {
                data.Add(item.ToString());
            }
            return data;
        }
        public static string[] ToArray(this LuaTable table)
        {
            var data = new List<string>();
            foreach (var item in table.Values)
            {
                data.Add(item.ToString());
            }
            return data.ToArray();
        }
    }
}
