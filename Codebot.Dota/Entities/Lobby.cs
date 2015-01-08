using System;
using Codebot.Xml;

namespace Codebot.Dota.Entities
{
    public class Lobby : Lookup
    {
        public string ShortName { get { return Name.StartsWith("Public") ? "Public" : Name; } }
    }
}

