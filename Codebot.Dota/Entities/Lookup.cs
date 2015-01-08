using System;
using Codebot.Xml;

namespace Codebot.Dota.Entities
{
    public class Lookup
    {
        public Lookup()
        {
            Id = -1;
            Handle = "invalid";
            Name = InvalidName;
        }

        protected virtual string InvalidName { get { return "Invalid"; }  }
        public bool Invalid { get { return Id < 0; } }

        public virtual bool Equals(int identity)
        {
            return identity == Id;
        }

        public virtual void Parse(Element element)
        {
            Filer filer = element.Filer;
            Id = filer.ReadInt("id");
            Handle = filer.ReadString("name");
            Name = filer.ReadString("localized_name");
        }

        public int Id { get; protected set; }
        public string Handle { get; protected set; }
        public string Name { get; set; }
        public object Tag { get; set; }
    }
}

