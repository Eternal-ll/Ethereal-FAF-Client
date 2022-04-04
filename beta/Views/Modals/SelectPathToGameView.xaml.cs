using ModernWpf.Controls;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;

namespace beta.Views.Modals
{
    public partial class SelectPathToGameView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly ContentDialog Dialog;
        public SelectPathToGameView(ContentDialog dialog)
        {
            InitializeComponent();
            DataContext = this;
            Dialog = dialog;
        }

        #region Path
        private string _Path;
        public string Path
        {
            get => Path;
            set
            {
                _Path = value;
                PropertyChanged?.Invoke(this, new(nameof(Path)));

                if (Directory.Exists(Path))
                {
                    if (File.Exists(Path + "\\bin\\SupremeCommander.exe"))
                    {
                        ErrorTextBlock.Text = "Game found";
                        Dialog.Content = null;
                        Dialog.Hide();
                    }
                }
                ErrorTextBlock.Text = "Game not found";
            }
        }
        #endregion

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO

            //var result = dlg.ShowDialog();
            //var error = "Can`t find Bin\\ForgedAlliance.exe in selected folder";
            //if (result == System.Windows.Forms.DialogResult.OK)
            //{
            //    if (File.Exists(dlg.SelectedPath + "\\bin\\SupremeCommander.exe"))
            //    {
            //        Properties.Settings.Default.PathToGame = dlg.SelectedPath;

            //        ((Button)sender).Click -= Button_Click;
            //        dlg.Dispose();

            //        Dialog.Content = null;
            //        Dialog.Hide();
            //    }
            //    else ErrorTextBlock.Text = error;
            //}
            //else
            //{
            //    ErrorTextBlock.Text = error;
            //}
        }
    }
}
