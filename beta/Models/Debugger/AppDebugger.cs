﻿using beta.Views.Windows;
using System.Diagnostics;

namespace beta.Models.Debugger
{
    class AppDebugger
    {
        private static ServerDebugWindow DebugWindow;

        [Conditional("DEBUG")]
        public static void Init()
        {
            DebugWindow = new ServerDebugWindow();
            DebugWindow.Show();
        }

        [Conditional("DEBUG")]
        public static void LOGLobby(string data) => DebugWindow.LOGLobby(data);

        [Conditional("DEBUG")]
        public static void LOGIRC(string data) => DebugWindow.LOGIRC(data);

        [Conditional("DEBUG")]
        public static void LOGICE(string data) => DebugWindow.LOGICE(data);
        [Conditional("DEBUG")]
        public static void LOGJSONRPC(string data) => DebugWindow.LOGJSONRPC(data);
    }
}
