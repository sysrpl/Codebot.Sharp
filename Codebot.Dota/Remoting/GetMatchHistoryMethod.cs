using System;
using Codebot.Net;
using Codebot.Xml;

namespace Codebot.Dota.Remoting
{
    public enum MatchSkill
    {
        Any = 0,
        Normal = 1,
        High = 2,
        VeryHigh = 3
    }

    public class GetMatchHistoryMethod : RemoteMethod
    {
        public GetMatchHistoryMethod AccountId(Int64 value)
        {
            Add("account_id", value);
            return this;
        }

        public GetMatchHistoryMethod MinPlayers(int value)
        {
            Add("min_players", value);
            return this;
        }

        public GetMatchHistoryMethod HeroId(int value)
        {
            Add("hero_id", value);
            return this;
        }

        public GetMatchHistoryMethod Skill(MatchSkill value)
        {
            Add("skill", (int)value);
            return this;
        }

        public GetMatchHistoryMethod StartAtMatchId(Int64 value)
        {
            Add("start_at_match_id", value);
            return this;
        }

        public GetMatchHistoryMethod MatchesRequested(int value)
        {
            Add("matches_requested", value);
            return this;
        }
    }
}
