using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for CustomGamesView.xaml
    /// </summary>
    public partial class CustomGamesView : UserControl
    {
        public CustomGamesView() => InitializeComponent();

        private void TextBox_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                ((TextBox)sender).Clear();
            }
        }
    }
}
