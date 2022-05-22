using beta.Infrastructure.Behaviors;
using beta.Models.API.MapsVault;
using beta.ViewModels;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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
        public MapDetailsView(ApiMapModel selected, ApiMapModel[] similar) : this() => DataContext = new MapViewModel(selected, similar);
        private void ListBox_Initialized(object sender, System.EventArgs e)
        {
            ((ListBox)sender).ItemsSource = Enumerable.Range(0, 20);
        }

        private void ScrollViewer_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var scroll = (ScrollViewer)sender;
            var property = scroll.GetType().GetProperty("ScrollInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            property.SetValue(scroll, new ScrollInfoAdapter((IScrollInfo)property.GetValue(scroll)));
        }

        private void ListBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //var scroll = Tools.FindChild<ScrollViewer>((ListBox)sender);
            //var property = scroll.GetType().GetProperty("ScrollInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            //property.SetValue(scroll, new ScrollInfoAdapter((IScrollInfo)property.GetValue(scroll)));
        }
    }
}
