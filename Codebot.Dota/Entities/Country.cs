using System;

namespace Codebot.Dota.Entities
{
    public class Country : Lookup
    {
        private static readonly string blank = "http://cache.dotaplayer.info/images/blank.png";
        private static readonly string flag = "http://steamcommunity-a.akamaihd.net/public/images/countryflags/{0}.gif";

        protected override string InvalidName { get { return ""; }  }

        public string Flag
        { 
            get 
            { 
                return Id < 0 ? blank : String.Format(flag, Handle.ToLower()); 
            }
        }
    }
}
