using System;
using System.Configuration;
using Codebot.Net;
using Codebot.Data;
using Codebot.Xml;

// Based on the Dota 2 WebAPI: http://dev.dota2.com/showthread.php?t=47115
// More things to know: http://dev.dota2.com/showthread.php?t=58317
// Steam API Reference: https://developer.valvesoftware.com/wiki/Steam_Web_API

// https://api.steampowered.com/IDOTA2Match_570/GetMatchHistory/V001/?format=XML&skill=3&key=CA6F17829044A38170FAE2CF7ACB0CF3
// https://api.steampowered.com/IDOTA2Match_570/GetMatchHistoryBySequenceNum/v0001/?format=XML&key=CA6F17829044A38170FAE2CF7ACB0CF3&start_at_match_seq_num=759700131
// https://api.steampowered.com/IDOTA2Match_570/GetMatchDetails/V001/?match_id=847298208&format=XML&key=CA6F17829044A38170FAE2CF7ACB0CF3
// http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?format=XML&key=CA6F17829044A38170FAE2CF7ACB0CF3&steamids=76561198075439542
// http://api.steampowered.com/IEconDOTA2_205790/GetHeroes/v0001/?language=en&format=XML&key=CA6F17829044A38170FAE2CF7ACB0CF3
// To launch dota? ... dota2://matchid=852669330
// https://api.steampowered.com/IEconDOTA2_570/GetHeroes/v0001/?key=CA6F17829044A38170FAE2CF7ACB0CF3
namespace Codebot.Dota.Remoting
{
    public static class Api
    {
        private const long steamBits = 76561197960265728;

        public static bool PlayerIsPublic(long playerId)
        {
            if (playerId < 1)
                return false;
            const long privatePlater = 4294967295;
            return playerId != privatePlater;
        }

        public static long PlayerToSteam(long playerId)
        {
            if (playerId < 1 || playerId == steamBits)
                return 0;
            return playerId < steamBits ? playerId + steamBits : playerId;
        }

        public static long SteamToPlayer(long steamId)
        {
            if (steamId < 1 || steamId == steamBits)
                return 0;
            return steamId > steamBits ? steamId - steamBits : steamId;
        }

        public static string PlayerToUrl(long playerId)
        {
            return SteamToUrl(PlayerToSteam(playerId));
        }

        public static string SteamToUrl(long steamId)
        {
            const string url = "http://steamcommunity.com/profiles/{0}/";
            return String.Format(url, steamId);
        }

        private const int second = 1;
        private const int minute = 60 * second;
        private const int hour = 60 * minute;
        private const int day = 24 * hour;
        private const int month = 30 * day;

        public static string DurationToClock(int duration)
        {
            return (duration / minute).ToString("D2") + ":" + (duration % minute).ToString("D2");
        }

        public static DateTime UnixTimeToDateTime(long unixTime)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        public static string UnixTimeToAgo(long unixTime)
        {
            DateTime date = UnixTimeToDateTime(unixTime);
            return DateTimeToAgo(date);
        }

        public static long UnixTimeToSecondsAgo(long unixTime)
        {
            var dateTime = UnixTimeToDateTime(unixTime);
            return DateTimeToSecondsAgo(dateTime);
        }

        public static long DateTimeToUnixTime(DateTime dateTime)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var span = dateTime - epoch;
            return (long)span.TotalSeconds;
        }

        public static long DateTimeToSecondsAgo(DateTime date)
        {
            var span = new TimeSpan(DateTime.UtcNow.Ticks - date.Ticks);
            return (long)span.TotalSeconds;
        }

        public static string DateTimeToAgo(DateTime date)
        {
            var span = new TimeSpan(DateTime.UtcNow.Ticks - date.Ticks);
            var delta = DateTimeToSecondsAgo(date);
            if (delta < 0)
            {
                return "not yet";
            }
            if (delta < 1 * minute)
            {
                return span.Seconds == 1 ? "one second ago" : span.Seconds + " seconds ago";
            }
            if (delta < 2 * minute)
            {
                return "a minute ago";
            }
            if (delta < 45 * minute)
            {
                return span.Minutes + " minutes ago";
            }
            if (delta < 90 * minute)
            {
                return "an hour ago";
            }
            if (delta < 24 * hour)
            {
                return span.Hours + " hours ago";
            }
            if (delta < 48 * hour)
            {
                return "yesterday";
            }
            if (delta < 30 * day)
            {
                return span.Days + " days ago";
            }
            if (delta < 12 * month)
            {
                int months = System.Convert.ToInt32(Math.Floor((double)span.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            else
            {
                int years = System.Convert.ToInt32(Math.Floor((double)span.Days / 365));
                return years <= 1 ? "one year ago" : years + " years ago";
            }
        }

        private static string key = ConfigurationManager.AppSettings["dota.key"];

        //    GetMatchHistory available parameters
        //    ------------------------------------
        //    language=<code>                # localized hero language
        static public GetHeroesMethod GetHeroes()
        {
            var method = new GetHeroesMethod();
            method.Prepare("https://api.steampowered.com/IEconDOTA2_570/GetHeroes/v0001/", key);
            method.Add("language", "en_us");
            return method;
        }

        //    GetMatchHistory available parameters
        //    ------------------------------------
        //    account_id=<id>                # search for all matches for the given user (32-bit or 64-bit steam ID)
        //    date_min=<date>                # date in UTC seconds since Jan 1, 1970 (unix time format)
        //    date_max=<date>                # date in UTC seconds since Jan 1, 1970 (unix time format)
        //    hero_id=<id>                   # Search for matches with a specific hero being played (hero ID, not name, see HEROES below)
        //    min_players=<count>            # the minimum number of players required in the match
        //    player_name=<name>             # Search matches with a player name, exact match only
        //    hero_id=<id>                   # Search for matches with a specific hero being played
        //    skill=<skill>                  # 0 for any, 1 for normal, 2 for high, 3 for very high skill
        //    league_id=<id>                 # matches for a particular league
        //    skill=<skill>                  # 0 for any, 1 for normal, 2 for high, 3 for very high skill (default is 0)
        //    start_at_match_id=<id>         # start the search at the indicated match id, descending
        //    tournament_games_only=<string> # set to only show tournament games
        //    matches_requested=<n>          # defaults is 25 matches, this can limit to less
        static public GetMatchHistoryMethod GetMatchHistory()
        {
            var method = new GetMatchHistoryMethod();
            method.Prepare("https://api.steampowered.com/IDOTA2Match_570/GetMatchHistory/V001/", key);
            return method;
        }

        //    GetMatchHistoryBySequenceNum available parameters
        //    ------------------------------------
        //     start_at_match_seq_num=<id>    # start the search at the indicated match id, descending
        //     matches_requested=<n>          # maximum is 25 matches (default is 25)
        static public GetMatchHistoryBySequenceNumMethod GetMatchHistoryBySequenceNum(long startAtMatchSeqNum)
        {
            var method = new GetMatchHistoryBySequenceNumMethod();
            method.Prepare("https://api.steampowered.com/IDOTA2Match_570/GetMatchHistoryBySequenceNum/v0001/", key);
            method.Add("start_at_match_seq_num", startAtMatchSeqNum);
            return method;
        }

        //    GetMatchDetails available parameters
        //    ------------------------------------
        //    match_id=<id>                  # get detailed information about a specified match
        static public GetMatchDetailsMethod GetMatchDetails(long matchId)
        {
            var method = new GetMatchDetailsMethod();
            method.Prepare("https://api.steampowered.com/IDOTA2Match_570/GetMatchDetails/V001/", key);
            method.Add("match_id", matchId);
            return method;
        }

        //    GetPlayerSummaries available parameters
        //    ------------------------------------
        //    steamids=<list>                # comma-separated list of 64 bit steam IDs to retrieve
        static public GetPlayerSummariesMethod GetPlayerSummaries(params long[] steamIds)
        {
            var method = new GetPlayerSummariesMethod();
            method.Prepare("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/", key);
            method.Add("steamids", String.Join(",", steamIds));
            return method;
        }
    }
}

