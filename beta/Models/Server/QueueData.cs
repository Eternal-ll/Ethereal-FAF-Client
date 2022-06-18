using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using beta.ViewModels.Base;
using System;
using System.Text.Json.Serialization;

namespace beta.Models.Server
{
    public class QueueData : ViewModel, IServerMessage
    {
        public ServerCommand Command { get; set; }
        [JsonPropertyName("queue_name")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MatchMakerType Type { get; set; }
        [JsonPropertyName("queue_pop_time")]
        private string _queue_pop_time;
        public string queue_pop_time
        {
            get => _queue_pop_time;
            set => Set(ref _queue_pop_time, value);
        }

        #region TimeSpanToMatch
        /// <summary>
        /// Seconds to auto-match
        /// </summary>
        private double _queue_pop_time_delta;
        public double queue_pop_time_delta
        {
            get => _queue_pop_time_delta;
            set
            {
                if (Set(ref _queue_pop_time_delta, value))
                {
                    OnPropertyChanged(nameof(TimeSpanToMatch));
                }
            }
        }
        #endregion

        public TimeSpan TimeSpanToMatch => TimeSpan.FromSeconds(queue_pop_time_delta);

        #region _CountInQueue
        private int _CountInQueue;
        [JsonPropertyName("num_players")]
        public int CountInQueue
        {
            get => _CountInQueue;
            set => Set(ref _CountInQueue, value);
        } 
        #endregion
        public int team_size { get; set; }

        #region boundary_80s
        private int[][] _boundary_80s;
        public int[][] boundary_80s
        {
            get => _boundary_80s;
            set => Set(ref _boundary_80s, value);
        } 
        #endregion
        public int[][] boundary_75s { get; set; }

        //Additional fields

        public string Mode => Type switch
        {
            MatchMakerType.ladder1v1 or
            MatchMakerType.tmm2v2 or
            MatchMakerType.tmm4v4_share_until_death => "Share until death",
            MatchMakerType.tmm4v4_full_share => "Full share",
            _ => "Unknown"
        };
        public string Name => Type switch
        {
            MatchMakerType.ladder1v1 => "1 vs 1",
            MatchMakerType.tmm2v2 => "2 vs 2",
            MatchMakerType.tmm4v4_full_share or
            MatchMakerType.tmm4v4_share_until_death => "4 vs 4",
            _ => Type.ToString(),
        };
    }
}
