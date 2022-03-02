using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using System;
using System.Collections.Generic;

namespace beta.Models.Server
{
    public enum PreviewType : byte
    {
        Normal = 0,
        Coop = 1,
        Neroxis = 2
    }

    public struct InGameTeam
    {
        public string Name => Number switch
        {
            > 0 => Number - 1 == 0 ? "No team" : "Team " + (Number - 1),
            -1 => "Observers",
            _ => "Unknown",
        };

        public int Number { get; }
        public IPlayer[] Players { get; }
        public int SumRating
        {
            get
            {
                if (Players == null || Players.Length == 0) return 0;
                int sum = 0;
                for (int i = 0; i < Players.Length; i++)
                    if (Players[i] is PlayerInfoMessage player)
                        sum += player.ratings["global"].DisplayedRating;
                return sum;
            }
        }
        public int AverageRating
        {
            get
            {
                if (Players.Length == 0) return 0;

                var avg = SumRating / Players.Length;
                if (avg > 1000)
                    return (int)Math.Round(avg * .01) * 100;

                if (avg > 100)
                    return (int)Math.Round(avg * .01) * 100;

                return SumRating / Players.Length;
            }
        }

        public InGameTeam(int number, IPlayer[] players) : this()
        {
            Number = number;
            Players = players;
        }
    }

    //public static class GameInfoExtensions
    //{
    //    public static bool Update(this GameInfoMessage orig, GameInfoMessage newInfo)
    //    {
    //        if (orig.num_players > newInfo.num_players && newInfo.num_players == 0)
    //            return false;

    //        if (orig.num_players < newInfo.num_players)
    //            orig.PlayersCountChanged = 1;
    //        else if (orig.num_players > newInfo.num_players)
    //            orig.PlayersCountChanged = -1;
    //        else orig.PlayersCountChanged = null;

    //        orig.title = newInfo.title;

    //        orig.num_players = newInfo.num_players;

    //        // We have to change status of players that left or join
    //        // TODO FIX ME Rewrite this shit
    //        #region Updating status of left players
    //        IPlayer[] origPlayers = new IPlayer[30];
    //        int k = 0;
    //        for (int i = 0; i < orig.Teams.Length; i++)
    //        {
    //            var players = orig.Teams[i].Players;
    //            for (int j = 0; j < players.Length; j++)
    //            {
    //                origPlayers[k] = players[j];
    //                k++;
    //            }
    //        }

    //        IPlayer[] newPlayers = new IPlayer[30];
    //        k = 0;
    //        for (int i = 0; i < newInfo.Teams.Length; i++)
    //        {
    //            var players = newInfo.Teams[i].Players;
    //            for (int j = 0; j < players.Length; j++)
    //            {
    //                newPlayers[k] = players[j];
    //                k++;
    //            }
    //        }

    //        var enumerator = origPlayers.Except(newPlayers).GetEnumerator();
    //        while (enumerator.MoveNext())
    //            if(enumerator.Current is PlayerInfoMessage player)
    //                player.Game = null;
            
    //        #endregion

    //        orig.Teams = newInfo.Teams;
    //        orig.teams = newInfo.teams;
                
    //        orig.sim_mods = newInfo.sim_mods;


    //        //Map update
    //        if (orig.mapname != newInfo.mapname)
    //        {
    //            orig.map_file_path = newInfo.map_file_path;
    //            orig.max_players = newInfo.max_players;
    //            orig.mapname = newInfo.mapname;
    //        }

    //        orig.password_protected = newInfo.password_protected;

    //        return true;
    //    }
    //}

    public struct GameInfoMessage: IServerMessage
    {
        public ServerCommand Command { get; set; }
        public GameInfoMessage[] games { get; set; }

        public long uid { get; set; }
        public string title { get; set; }
        public string visibility { get; set; }
        public bool password_protected { get; set; }
        public string state { get; set; }
        public string game_type { get; set; }
        public string featured_mod { get; set; }
        public Dictionary<string, string> sim_mods { get; set; }
        public string mapname { get; set; }
        public string map_file_path { get; set; }
        public string host { get; set; }
        public int num_players { get; set; }
        public int max_players { get; set; }
        public double? launched_at { get; set; }
        public string rating_type { get; set; }
        public double? rating_min { get; set; }
        public double? rating_max { get; set; }
        public bool enforce_rating_range { get; set; }
        public Dictionary<int, string[]> teams { get; set; }
    }
}
