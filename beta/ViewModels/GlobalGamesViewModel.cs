using beta.ViewModels.Base;
using System.Threading.Tasks;

namespace beta.ViewModels
{
    internal class GlobalGamesViewModel : ViewModel
    {
        private CustomGamesViewModel CustomGamesViewModel;
        private CustomLiveGamesViewModel CustomLiveGamesViewModel;

        public GlobalGamesViewModel()
        {
            Task.Run(() => IsIdleGamesOnView = true);
        }

        #region CurrentCustomGamesViewModel
        private ViewModel _CurrentCustomGamesViewModel = new PlugViewModel();
        public ViewModel CurrentCustomGamesViewModel
        {
            get => _CurrentCustomGamesViewModel;
            set
            {
                if (Set(ref _CurrentCustomGamesViewModel, value))
                {

                }
            }
        }
        #endregion

        #region IsLiveGamesOnView
        private bool _IsLiveGamesOnView;
        public bool IsLiveGamesOnView
        {
            get => _IsLiveGamesOnView;
            set
            {
                if (!value && !_IsIdleGamesOnView) return;
                if (Set(ref _IsLiveGamesOnView, value))
                {
                    if (value)
                    {
                        _IsIdleGamesOnView = false;
                        OnPropertyChanged(nameof(IsIdleGamesOnView));
                        CurrentCustomGamesViewModel = CustomLiveGamesViewModel ??= new();
                    }
                }
            }
        }
        #endregion
        #region IsIdleGamesOnView
        private bool _IsIdleGamesOnView;
        public bool IsIdleGamesOnView
        {
            get => _IsIdleGamesOnView;
            set
            {
                if (!value && !_IsLiveGamesOnView) return;
                if (Set(ref _IsIdleGamesOnView, value))
                {
                    if (value)
                    {
                        _IsLiveGamesOnView = false;
                        OnPropertyChanged(nameof(IsLiveGamesOnView));
                        CurrentCustomGamesViewModel = CustomGamesViewModel ??= new();
                    }
                }
            }
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CustomLiveGamesViewModel?.Dispose();
                CustomGamesViewModel?.Dispose();
                CustomLiveGamesViewModel = null;
                CustomGamesViewModel = null;
            }
            base.Dispose(disposing);
        }
    }
}
