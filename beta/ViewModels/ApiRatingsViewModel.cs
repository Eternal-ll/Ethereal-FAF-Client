using beta.Models.API;
using beta.Models.API.Base;
using beta.Models.Server.Enums;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace beta.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    internal class ApiRatingsViewModel : ApiPlayerViewModel
    {
        public string[] RatingTypes { get; private set; }

        private readonly Dictionary<string, ApiGamePlayerStats[]> Data = new();

        #region LiveCharts
        public ISeries[] Series { get; set; } =
        {
            new LineSeries<DateTimePoint>
            {
                TooltipLabelFormatter = (chartPoint) =>
                    $"{new DateTime((long)chartPoint.SecondaryValue):dd.MM.yyyy}: {chartPoint.PrimaryValue}",
                Values = new ObservableCollection<DateTimePoint>(),
                GeometrySize = 2,
                LineSmoothness = 0,
                Stroke = new SolidColorPaint(SKColors.SkyBlue, 2),
            }
        };

            public Axis[] XAxes { get; set; } =
            {
                new Axis
                {
                    Labeler = value => new DateTime((long) value).ToString("dd.MM.yyyy"),
                    LabelsRotation = 15,

                    // when using a date time type, let the library know your unit 
                    UnitWidth = TimeSpan.FromDays(1).Ticks, 

                    // if the difference between our points is in hours then we would:
                    // UnitWidth = TimeSpan.FromHours(1).Ticks,

                    // since all the months and years have a different number of days
                    // we can use the average, it would not cause any visible error in the user interface
                    // Months: TimeSpan.FromDays(30.4375).Ticks
                    // Years: TimeSpan.FromDays(365.25).Ticks

                    // The MinStep property forces the separator to be greater than 1 day.
                    MinStep = TimeSpan.FromDays(1).Ticks
                }
            };
        #endregion

        public ApiRatingsViewModel(int playerId, params string[] ratingTypes) : base(playerId)
        {
            if (ratingTypes.Length == 0) return;
            RatingTypes = ratingTypes;
            for (int i = 0; i < ratingTypes.Length; i++)
            {
                Data.Add(ratingTypes[i], null);
            }
            SelectedRatingType = ratingTypes[0];
        }

        #region SelectedRatingType
        private string _SelectedRatingType;
        public string SelectedRatingType
        {
            get => _SelectedRatingType;
            set
            {
                if (Set(ref _SelectedRatingType, value))
                {
                    RunRequest();
                }
            }
        }
        #endregion

        public ApiGamePlayerStats[] SelectedRatingData => Data[SelectedRatingType];

        protected override async Task RequestTask()
        {
            var lineSeries = (ObservableCollection<DateTimePoint>)Series[0].Values;
            lineSeries.Clear();

            var cached = Data[SelectedRatingType];
            if (IsRefreshing)
            {
                cached = null;
            }

            if (cached is not null)
            {
                foreach (var cachedData in cached)
                {
                    if (cachedData.ScoreDateTime.HasValue)
                    {
                        lineSeries.Add(new(cachedData.ScoreDateTime.Value, cachedData.RatingAfter));
                    }
                }
                OnPropertyChanged(nameof(SelectedRatingData));
                return;
            }
            

            var url = $"https://api.faforever.com/data/gamePlayerStats?filter=(player.id=={PlayerId};ratingChanges.leaderboard.technicalName=={SelectedRatingType})&fields[gamePlayerStats]=afterDeviation,afterMean,beforeDeviation,beforeMean,scoreTime&page[totals]=yes&page[size]=500";
            List<ApiGamePlayerStats> data = new();
            var pages = 1;

            ApiGamePlayerStats latestStats = null;
            for (int i = 1; i <= pages; i++)
            {
                var result = await ApiRequest<ApiUniversalResult<ApiGamePlayerStats[]>>.Request(url + $"&page[number]=" + i);
                if (i == 1)
                {
                    if (result.Data[0].ScoreDateTime.HasValue)
                        lineSeries.Add(new(result.Data[0].ScoreDateTime.Value, 0));
                }
                pages = result.Meta.Page.AvaiablePagesCount;
                for (int j = 0; j < result.Data.Length; j++)
                {
                    var rating = result.Data[j];

                    if (latestStats?.RatingAfter - rating.RatingAfter > 200)
                        continue;

                    data.Add(rating);

                    if (rating.ScoreDateTime.HasValue)
                    {
                        lineSeries.Add(new(rating.ScoreDateTime.Value, rating.RatingAfter));
                    }
                    latestStats = rating;
                }
            }
            Data[SelectedRatingType] = data.ToArray();
            OnPropertyChanged(nameof(SelectedRatingData));
        }
    }
}
