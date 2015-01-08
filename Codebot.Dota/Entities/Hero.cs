using System;
using System.Linq;
using Codebot.Xml;
using Codebot.Dota.Remoting;

namespace Codebot.Dota.Entities
{
    public class Hero : Lookup
    {
        public override void Parse(Element element)
        {
            base.Parse(element);
            Filer filer = element.Filer;
            Kind = filer.ReadString("kind");
            Attack = filer.ReadString("attack");
            Roles = filer.ReadString("roles");
        }

        public string Kind { get; set; }
        public string Attack { get; set; }
        public string Roles { get; set; }

        private readonly string path = "images/heroes/";

        private string Path(string size, string ext)
        {
            return Host.Bucket + path + size + Handle + ext;
        }

        public string ImageLarge { get { return Path("large/", ".png");  } }
        public string ImageMedium { get { return Path("medium/", ".48.png");  } }
        public string ImageSmall { get { return Path("small/", ".24.png");  } }
    }
 }

