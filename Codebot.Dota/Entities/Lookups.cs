using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codebot.Runtime;
using Codebot.Xml;

namespace Codebot.Dota.Entities
{
    public static class Lookups
    {
        private static Document Load(string resource)
        {
            return Document.Open(PathMapper.Map("Resources", resource));
        }

        private static LookupCollection<Country> countries;

        public static LookupCollection<Country> Countries
        {
            get
            {
                if (Object.ReferenceEquals(countries, null))
                {
                    countries = new LookupCollection<Country>();
                    countries.AddRange(Load("countries.xml").Root.Nodes);
                }
                return countries;
            }
        }

        private static LookupCollection<Hero> heroes;

        public static LookupCollection<Hero> Heroes
        {
            get
            {
                if (Object.ReferenceEquals(heroes, null))
                {
                    heroes = new LookupCollection<Hero>();
                    heroes.AddRange(Load("heroes.xml").Root.Nodes);
                }
                return heroes;
            }
        }

        private static LookupCollection<Item> items;

        private static Item LocateItem(string name)
        {
            name = name.Replace("'", "");
            if (name.StartsWith("recipe"))
                return null;
            return Items.Where(a => a.Name.ToLower().Replace("'", "") == name).FirstOrDefault();
        }

        public static LookupCollection<Item> Items
        {
            get
            {
                if (Object.ReferenceEquals(items, null))
                {
                    items = new LookupCollection<Item>(LocateItem);
                    items.AddRange(Load("items.xml").Root.Nodes);
                }
                return items;
            }
        }

        private static LookupCollection<Lobby> lobbies;

        public static LookupCollection<Lobby> Lobbies
        {
            get
            {
                if (Object.ReferenceEquals(lobbies, null))
                {
                    lobbies = new LookupCollection<Lobby>();
                    lobbies.AddRange(Load("lobbies.xml").Root.Nodes);
                }
                return lobbies;
            }
        }

        private static LookupCollection<Mode> modes;

        public static LookupCollection<Mode> Modes
        {
            get
            {
                if (Object.ReferenceEquals(modes, null))
                {
                    modes = new LookupCollection<Mode>();
                    modes.AddRange(Load("modes.xml").Root.Nodes);
                }
                return modes;
            }
        }

        private static LookupCollection<Region> regions;

        public static LookupCollection<Region> Regions
        {
            get
            {
                if (Object.ReferenceEquals(regions, null))
                {
                    regions = new LookupCollection<Region>();
                    regions.AddRange(Load("regions.xml").Root.Nodes);
                }
                return regions;
            }
        }
    }
}

