using Ethereal.FA.Scmap;
using System.Windows;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            new Scmap(@"C:\Users\Eternal\Documents\My Games\Gas Powered Games\Supreme Commander Forged Alliance\Maps\dualgap_adaptive.v0012");
        }
    }
}
