using Ethereal.FAF.UI.Client.ViewModels;
using System;
using System.Windows.Controls;

namespace Ethereal.FAF.UI.Client.Views.Hosting
{
    /// <summary>
    /// Interaction logic for SelectLocalMapView.xaml
    /// </summary>
    public sealed partial class SelectLocalMapView : UserControl, IGameHosting, IDisposable
    {
        private LocalMapsVM Model;
        public SelectLocalMapView(LocalMapsVM model)
        {
            DataContext = model;
            Model = model;
            InitializeComponent();
        }
        bool disposed;
        public void Dispose()
        {
            if (disposed) return;
            Model.Dispose();
            Content = null;
            InvalidateVisual();
            GC.Collect();
        }

        public void SetHostingModel(GameHostingModel model)
        {
            Model.Game = model;
        }
    }
}
