using ModernWpf.Controls;
using System.IO;
using System.Windows.Controls;

namespace beta.Views.Modals
{
    public partial class SelectPathToGameView : UserControl
    {
        private readonly ContentDialog Dialog;
        public SelectPathToGameView(ContentDialog dialog)
        {
            InitializeComponent();
            Dialog = dialog;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new();

            var result = dlg.ShowDialog();
            var error = "Can`t find Bin\\ForgedAlliance.exe in selected folder";
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (File.Exists(dlg.SelectedPath + "\\bin\\SupremeCommander.exe"))
                {
                    Properties.Settings.Default.PathToGame = dlg.SelectedPath;

                    ((Button)sender).Click -= Button_Click;
                    dlg.Dispose();

                    Dialog.Content = null;
                    Dialog.Hide();
                }
                else ErrorTextBlock.Text = error;
            }
            else
            {
                ErrorTextBlock.Text = error;
            }
        }
    }
}
