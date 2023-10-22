using beta.Infrastructure.Utils;
using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for ClansView.xaml
    /// </summary>
    public partial class ClansView : INavigableView<ClansViewModel>
    {
        public ClansView(ClansViewModel vm)
        {
            ViewModel = vm;
            Initialized += ClansView_Initialized;
            InitializeComponent();
        }

        private void ClansView_Initialized(object sender, System.EventArgs e)
        {
            ViewModel.LoadPageCommand.Execute(null);
        }

        public ClansViewModel ViewModel { get; }

        private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeight - e.ViewportHeight - e.VerticalOffset == 0)
            {
                if (ViewModel.CanLoadPage())
                {
                    Task.Run(ViewModel.AddPage);
                }
            }
        }
    }
}
