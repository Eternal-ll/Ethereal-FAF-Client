using FAF.Domain.LobbyServer.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public abstract class INPC : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        public virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        }
    }
    public partial class GameInfoMessage : INPC
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServerCommand Command { get; set; }
        [JsonPropertyName("games")]
        public GameInfoMessage[] Games { get; set; }
        [JsonPropertyName("visibility")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameVisibility Visibility { get; set; }
        [JsonPropertyName("password_protected")]
        public bool PasswordProtected { get; set; }
        [JsonPropertyName("uid")]
        public long Uid { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        #region State
        private GameState _State;
        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameState State { get => _State; set => Set(ref _State, value); } 
        #endregion
        [JsonPropertyName("game_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }
        [JsonPropertyName("featured_mod")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FeaturedMod FeaturedMod { get; set; }
        [JsonPropertyName("sim_mods")]
        public Dictionary<string, string> SimMods { get; set; }
        #region Mapname
        private string _Mapname;
        [JsonPropertyName("mapname")]
        public string Mapname { get => _Mapname; set => Set(ref _Mapname, value); }
        #endregion
        #region MapFilePath
        private string _MapFilePath;
        [JsonPropertyName("map_file_path")]
        public string MapFilePath { get => _MapFilePath; set => Set(ref _MapFilePath, value); }
        #endregion
        #region Host
        private string _Host;
        [JsonPropertyName("host")]
        public string Host { get => _Host; set => Set(ref _Host, value); } 
        #endregion
        #region NumPlayers
        private int _NumPlayers;
        [JsonPropertyName("num_players")]
        public int NumPlayers { get => _NumPlayers; set => Set(ref _NumPlayers, value); }
        #endregion
        #region MaxPlayers
        private int _MaxPlayers;
        [JsonPropertyName("max_players")]
        public int MaxPlayers { get => _MaxPlayers; set => Set(ref _MaxPlayers, value); }
        #endregion
        #region LaunchedAt
        private double? _LaunchedAt;
        [JsonPropertyName("launched_at")]
        public double? LaunchedAt { get => _LaunchedAt; set => Set(ref _LaunchedAt, value); } 
        #endregion
        [JsonPropertyName("rating_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RatingType RatingType { get; set; }
        [JsonPropertyName("rating_min")]
        public double? RatingMin { get; set; }
        [JsonPropertyName("rating_max")]
        public double? RatingMax { get; set; }
        [JsonPropertyName("enforce_rating_range")]
        public bool EnforceRatingRange { get; set; }
        [JsonPropertyName("teams_ids")]
        public TeamIds[] TeamsIds { get; set; }
        [JsonPropertyName("teams")]
        public Dictionary<int, string[]> Teams { get; set; }
    }
}
