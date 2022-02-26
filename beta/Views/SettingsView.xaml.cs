using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new(propertyName));


        public SettingsView()
        {
            InitializeComponent();

            //Settings = Properties.Settings.Default;

            Loaded += SettingsView_Loaded;
        }


        private void SettingsView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
