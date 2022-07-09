using beta.ViewModels;
using System;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for NewsViewModel.xaml
    /// </summary>
    public partial class NewsView : UserControl
    {
        private readonly NewsViewModel ViewModel;
        public NewsView(NewsViewModel model)
        {
            ViewModel = model;
            DataContext = model;
            IsVisibleChanged += NewsView_IsVisibleChanged;
            InitializeComponent();
        }
            
        private void NewsView_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue) return;
            IsVisibleChanged -= NewsView_IsVisibleChanged;
            Dispose();
        }
        private void Dispose()
        {
            //ViewModel.Dispose();
            //ViewModel = null;
            Content = null;
            UpdateLayout();
            GC.Collect();
            GC.WaitForFullGCComplete();
        }
    }
}
