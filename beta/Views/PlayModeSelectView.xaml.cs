using beta.ViewModels;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for PlayModeSelectView.xaml
    /// </summary>
    public partial class PlayModeSelectView : UserControl
    {
        public PlayModeSelectView(PlayModeSelectVM model)
        {
            DataContext = model;
            InitializeComponent();
        }
    }
}
