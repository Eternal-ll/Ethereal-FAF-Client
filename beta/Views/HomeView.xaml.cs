using System.Diagnostics;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView
    {
        public HomeView()
        {
            InitializeComponent();
        }

        private void TextBox_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                ((TextBox)sender).Text = string.Empty;
            }
        }
    }
}
