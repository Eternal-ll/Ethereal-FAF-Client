﻿using Ethereal.FAF.UI.Client.ViewModels;
using System.Windows.Controls;

namespace Ethereal.FAF.UI.Client.Views.Hosting
{
    /// <summary>
    /// Interaction logic for HostGameView.xaml
    /// </summary>
    public sealed partial class HostGameView : UserControl
    {
        public HostGameView(HostGameViewModel model)
        {
            DataContext = model;
            InitializeComponent();
        }
    }
}
