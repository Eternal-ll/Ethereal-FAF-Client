using Ethereal.FAF.UI.Client.ViewModels;
using System;
using System.Windows.Controls;

namespace Ethereal.FAF.UI.Client.Views.Hosting
{
    /// <summary>
    /// Interaction logic for HostGameView.xaml
    /// </summary>
    public partial class HostGameView : UserControl
    {
        public HostGameView(HostGameViewModel model)
        {
            DataContext = model;
            InitializeComponent();
        }
    }
}
