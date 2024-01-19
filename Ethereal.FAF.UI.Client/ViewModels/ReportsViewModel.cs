using Ethereal.FAF.API.Client;
using Ethereal.FAF.API.Client.Models;
using Ethereal.FAF.API.Client.Models.Base;
using Meziantou.Framework.WPF.Collections;
using System.Linq;
using System.Timers;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class ReportViewModel : Base.ViewModel
    {
        private IFafApiClient ApiClient;
            
        public ReportViewModel()
        {
            LastGames = new();

            LastGamesTimer = new(1500);
            LastGamesTimer.AutoReset = false;
            LastGamesTimer.Elapsed += (s, e) =>
            {
                if (ApiClient is null) return;
                if (Reporter is null) return;
                var response = ApiClient.GetPlayerGamesAsync(Reporter.Id, new() { Parameter = "id", SortDirection = System.ComponentModel.ListSortDirection.Descending },
                    new() { PageNumber = 1, PageSize = 15 }).Result;
                if (!response.IsSuccessStatusCode) return;
                var games = response.Content.Data;
                LastGames.Clear();
                LastGames.AddRange(games);
                GamesIds = games.Select(g => g.Id).ToArray();
            };
        }

        public void Initialize(IFafApiClient apiClient, Player reporter)
        {
            ApiClient = apiClient;
            Reporter = reporter;
        }

        #region Reporter
        private Player _Reporter;
        public Player Reporter
        {
            get => _Reporter;
            set
            {
                if (Set(ref _Reporter, value))
                {
                    LastGamesTimer.Start();
                }
            }
        }
        #endregion

        #region GameId
        private int _GameId;
        public int GameId
        {
            get => _GameId;
            set
            {
                if (Set(ref _GameId, value))
                {
                    if (SelectedGame?.Id != value)
                    {
                        SelectedGame = LastGames.FirstOrDefault(g => g.Id == value);
                    }
                }
            }
        }
        #endregion

        public ConcurrentObservableCollection<Model<Game>> LastGames { get;  }

        #region GamesIds
        private int[] _GamesIds;
        public int[] GamesIds { get => _GamesIds; set => Set(ref _GamesIds, value); }
        #endregion

        #region SelectedGame
        private Model<Game> _SelectedGame;
        public Model<Game> SelectedGame
        {
            get => _SelectedGame;
            set
            {
                if (Set(ref _SelectedGame, value))
                {
                    if (value is not null)
                    {
                        GameId = value.Id;
                    }
                }
            }
        }
        #endregion

        private Timer LastGamesTimer = new();
    }
    /// <summary>
    /// Reports view-model. View and create reports
    /// </summary>
    public class ReportsViewModel : Base.ViewModel
    {
        public ReportsViewModel(ReportViewModel reportViewModel)
        {
            ReportViewModel = reportViewModel;
        }

        #region ReportViewModel
        private ReportViewModel _ReportViewModel;
        public ReportViewModel ReportViewModel { get => _ReportViewModel; set => Set(ref _ReportViewModel, value); }
        #endregion
    }
}
