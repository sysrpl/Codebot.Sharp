using System;
using Codebot.Xml;
using Codebot.Dota.Remoting;

namespace Codebot.Dota.Entities
{
    public class Item : Lookup
    {
        public Item()
            : base()
        {
            Cost = 0;
            Description = String.Empty;
        }

        public override void Parse(Element element)
        {
            base.Parse(element);
            Filer filer = element.Filer;
            Cost = filer.ReadInt("cost");
            Description = filer.ReadString("description");
        }


        public int BaseId 
        {
            get
            {
                if (Name.Contains("("))
                    return Lookups.Items.Locate(Name.Split('(')[0].Trim()).Id;
                else
                    return Id;
            }
        }
        public int Cost { get; set; }
        public string Description { get; set; }

        private readonly string path = "images/items/";

        private string Path(string size, string ext)
        {
            string handle = Handle.StartsWith("recipe") ? "recipe" : Handle;
            return Host.Bucket + path + size + handle + ext;
        }

        public string ImageLarge { get { return Path("large/", ".png");  } }
        public string ImageMedium { get { return Path("medium/", ".48.png");  } }
        public string ImageSmall { get { return Path("small/", ".24.png");  } }
    }
}
