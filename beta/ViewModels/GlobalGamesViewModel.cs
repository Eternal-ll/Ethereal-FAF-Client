using beta.ViewModels.Base;

namespace beta.ViewModels
{
    internal class GlobalGamesViewModel : ViewModel
    {
        private readonly CustomGamesViewModel CustomGamesViewModel;
        private readonly CustomLiveGamesViewModel CustomLiveGamesViewModel;

        public GlobalGamesViewModel()
        {
            CustomGamesViewModel = new();
            CustomLiveGamesViewModel = new();
            IsIdleGamesOnView = true;
        }

        #region CurrentCustomGamesViewModel
        private ViewModel _CurrentCustomGamesViewModel = new PlugViewModel();
        public ViewModel CurrentCustomGamesViewModel
        {
            get => _CurrentCustomGamesViewModel;
            set => Set(ref _CurrentCustomGamesViewModel, value);
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
                        CurrentCustomGamesViewModel = CustomLiveGamesViewModel;
                        OnPropertyChanged(nameof(IsIdleGamesOnView));
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
                        CurrentCustomGamesViewModel = CustomGamesViewModel;
                        OnPropertyChanged(nameof(IsLiveGamesOnView));
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
                CustomLiveGamesViewModel.Dispose();
                CustomGamesViewModel.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
