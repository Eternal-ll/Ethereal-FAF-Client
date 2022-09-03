using Ethereal.FAF.UI.Client.ViewModels;
using System;
using System.Windows.Controls;

namespace Ethereal.FAF.UI.Client.Views.Hosting
{
    /// <summary>
    /// Interaction logic for GenerateMapView.xaml
    /// </summary>
    public partial class GenerateMapView : UserControl, IGameHosting, IDisposable
    {
        GenerateMapsVM Model;
        public GenerateMapView(GenerateMapsVM model)
        {
            Model = model;
            DataContext = model;
            InitializeComponent();
        }
        bool disposed;
        public void Dispose()
        {
            if (disposed) return;
            Model.Dispose();
            InvalidateVisual();
        }

        public void SetHostingModel(GameHostingModel model)
        {
            Model.Game = model;
        }
    }
}
