using beta.ViewModels;
using System.Linq;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for MapDetailsView.xaml
    /// </summary>
    public partial class MapDetailsView : UserControl
    {
        public MapDetailsView() => InitializeComponent();
        public MapDetailsView(int id) : this() => DataContext = new MapViewModel(id);
        public MapDetailsView(string name) : this() => DataContext = new MapViewModel(name);

        private void ListBox_Initialized(object sender, System.EventArgs e)
        {
            ((ListBox)sender).ItemsSource = Enumerable.Range(0, 20);
        }
    }
}
