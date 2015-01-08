using System;
using Codebot.Net;
using Codebot.Xml;

namespace Codebot.Dota.Remoting
{
    public class GetMatchHistoryBySequenceNumMethod : RemoteMethod
    {
        public GetMatchHistoryBySequenceNumMethod MatchesRequested(int value)
        {
            Add("matches_requested", value);
            return this;
        }
    }
}
