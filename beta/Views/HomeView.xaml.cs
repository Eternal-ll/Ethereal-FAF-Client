using beta.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : INotifyPropertyChanged
    {
        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        public HomeView()
        {
            InitializeComponent();
        }
    }
}
