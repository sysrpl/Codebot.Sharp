using System;
using Codebot.Xml;

namespace Codebot.Dota.Entities
{
    public class Region : Lookup
    {
        public override bool Equals(int identity)
        {
            return identity / 10 == Id / 10;
        }
    }
}
