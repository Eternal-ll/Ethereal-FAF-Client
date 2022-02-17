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

        //NEWS format
        //https://direct.faforever.com/wp-json/wp/v2/posts?
        //per_page=1&
        //page=1&
        //_embed=author,wp:featuredmedia&
        //_fields[]=title&
        //_fields[]=content&
        //_fields[]=newshub_externalLinkUrl&
        //_fields[]=_links&
        //_fields[]=_links

        public HomeView()
        {
            InitializeComponent();
        }
    }
}
