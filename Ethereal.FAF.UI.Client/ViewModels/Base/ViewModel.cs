using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace Ethereal.FAF.UI.Client.ViewModels.Base
{
    public class ViewModel : ObservableObject, INotifyPropertyChanged, IDisposable
    {
        protected bool Initialized { get; set; }
        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        }
        public void Dispose()
        {
            Dispose(true);
        }

        private bool _Disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _Disposed) return;
            _Disposed = true;
            // Clean resources
        }



        /// <summary>
        /// Called when the view's LoadedEvent is fired.
        /// </summary>
        public virtual void OnLoaded()
        {
            if (!Initialized)
            {
                Initialized = true;
                OnInitialLoaded();
                Application.Current.Dispatcher.InvokeAsync(OnInitialLoadedAsync);
            }
        }

        /// <summary>
        /// Called the first time the view's LoadedEvent is fired.
        /// Sets the <see cref="ViewModelState.InitialLoaded"/> flag.
        /// </summary>
        protected virtual void OnInitialLoaded() { }

        /// <summary>
        /// Called asynchronously when the view's LoadedEvent is fired.
        /// Runs on the UI thread via Dispatcher.UIThread.InvokeAsync.
        /// The view loading will not wait for this to complete.
        /// </summary>
        public virtual Task OnLoadedAsync() => Task.CompletedTask;

        /// <summary>
        /// Called the first time the view's LoadedEvent is fired.
        /// Sets the <see cref="ViewModelState.InitialLoaded"/> flag.
        /// </summary>
        protected virtual Task OnInitialLoadedAsync() => Task.CompletedTask;

        /// <summary>
        /// Called when the view's UnloadedEvent is fired.
        /// </summary>
        public virtual void OnUnloaded() { }

        /// <summary>
        /// Called asynchronously when the view's UnloadedEvent is fired.
        /// Runs on the UI thread via Dispatcher.UIThread.InvokeAsync.
        /// The view loading will not wait for this to complete.
        /// </summary>
        public virtual Task OnUnloadedAsync() => Task.CompletedTask;
    }
}
