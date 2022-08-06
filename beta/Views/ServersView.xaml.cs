using beta.ViewModels;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for ServersView.xaml
    /// </summary>
    public partial class ServersView : UserControl
    {
        public ServersView(ServersViewModel model)
        {
            DataContext = model;
            InitializeComponent();
        }
    }
}
