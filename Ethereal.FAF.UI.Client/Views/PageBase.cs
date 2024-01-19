using Ethereal.FAF.UI.Client.ViewModels.Base;
using System.Windows;
using System.Windows.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    public class PageBase : Page
    {
        public PageBase()
        {
            DataContext = this;
            Loaded += (cls, args) =>
            OnLoadedEvent(args);
            Unloaded += (cls, args) => OnUnloadedEvent(args);
        }
        public virtual ViewModel GetViewModel() { return null; }
        protected virtual void OnLoadedEvent(RoutedEventArgs? e)
        {
            var viewModel = GetViewModel();
            if (viewModel == null)
                return;

            // Run synchronous load then async load
            viewModel.OnLoaded();

            // Can't block here so we'll run as async on UI thread
            Application.Current.Dispatcher.InvokeAsync(viewModel.OnLoadedAsync);
        }
        protected virtual void OnUnloadedEvent(RoutedEventArgs? e)
        {
            var viewModel = GetViewModel();
            if (viewModel == null)
                return;

            // Run synchronous load then async load
            viewModel.OnUnloaded();
            // Can't block here so we'll run as async on UI thread
            Application.Current.Dispatcher.InvokeAsync(viewModel.OnUnloadedAsync);
        }
    }
}
