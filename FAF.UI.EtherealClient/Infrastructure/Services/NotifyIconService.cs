// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Services;

namespace FAF.UI.EtherealClient.Infrastructure.Services
{
    public class NotifyIconService : NotifyIconServiceBase
    {
        public override bool Register()
        {
            if (IsRegistered)
                return false;

            InitializeContent();

            if (ParentWindow != null)
            {
                ParentHandle = new WindowInteropHelper(ParentWindow).Handle;

                base.Register();
            }

            if (ParentHandle == IntPtr.Zero)
                return false;
            return base.Register();
        }

        private void InitializeContent()
        {
            TooltipText = "Ethereal service";

            ContextMenu = new ContextMenu
            {
                Items =
                {
                    new Wpf.Ui.Controls.MenuItem
                    {
                        Header = "Home",
                        SymbolIcon = SymbolRegular.Library28
                    },
                    new Wpf.Ui.Controls.MenuItem
                    {
                        Header = "Save",
                        SymbolIcon = SymbolRegular.Save28
                    },
                    new Wpf.Ui.Controls.MenuItem
                    {
                        Header = "Open",
                        SymbolIcon = SymbolRegular.Folder28
                    },
                    new Separator(),
                    new Wpf.Ui.Controls.MenuItem
                    {
                        Header = "Reload",
                        SymbolIcon = SymbolRegular.ArrowClockwise28
                    },
                }
            };

            foreach (var singleContextMenuItem in ContextMenu.Items)
                if (singleContextMenuItem is MenuItem)
                    (singleContextMenuItem as MenuItem).Click += OnMenuItemClick;
        }
        private void OnMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;
        }
    }
}