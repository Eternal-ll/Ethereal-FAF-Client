using beta.ViewModels;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for MatchMakerView.xaml
    /// </summary>
    public partial class MatchMakerView : UserControl
    {
        public MatchMakerView(MatchMakerViewModel model)
        {
            DataContext = model;
            InitializeComponent();
        }

        private void ListBox_Initialized(object sender, System.EventArgs e)
        {

        }
    }
}
