using beta.ViewModels;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for CustomGamesView.xaml
    /// </summary>
    public partial class CustomGamesView : UserControl
    {
        public CustomGamesView(GamesViewModel model)
        {
            DataContext = model;
            InitializeComponent();
        }

        private void TextBox_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                ((TextBox)sender).Clear();
            }
        }
    }
    public partial class CustomOpenGamesView : CustomGamesView
    {
        public CustomOpenGamesView(CustomGamesViewModel model) : base(model){}
    }
    public partial class CustomLiveGamesView : CustomGamesView
    {
        public CustomLiveGamesView(CustomLiveGamesViewModel model) : base(model){}
    }
}
