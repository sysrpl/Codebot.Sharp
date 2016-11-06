using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Codebot.Web
{
    public class CachedAttribute : Attribute { }

    public static class WebCache
    {
        internal class CacheKey
        {
            public CacheKey(Type handlerType, string queryString)
            {
                this.handlerType = handlerType;
                this.queryString = queryString;
            }

            public override bool Equals(Object obj)
            {
                if (obj == null)
                    return false;
                CacheKey c = obj as CacheKey;
                if ((System.Object)c == null)
                    return false;
                return (c.handlerType == handlerType) && (c.queryString == queryString);
            }

            public bool Equals(CacheKey c)
            {
                if ((System.Object)c == null)
                    return false;
                return (c.handlerType == handlerType) && (c.queryString == queryString);
            }

            public override int GetHashCode()
            {
                return handlerType.GetHashCode() + queryString.GetHashCode();
            }

            public Type handlerType;
            public  string queryString;
        }

        internal class CacheValue
        {
            public CacheValue(string value, BasicHandler handler)
            {
                Stamp = DateTime.Now;
                Value = value;
                Agent = handler.Request.UserAgent;
                Address = handler.Request.UserHostAddress;
                Rewrites = 0;
                Reuses = 0;
            }

            public DateTime Stamp { get; set; }
            public string Value { get; set; }
            public string Agent { get; set; }
            public string Address { get; set; }
            public long Rewrites { get; set; }
            public long Reuses { get; set; }
        }

        private enum CacheAction
        {
            Add,
            Rewrite,
            Reuse
        }

        private static bool enabled = true;
        private static int cacheLimit = 1000;
        private static Dictionary<CacheKey, CacheValue> cache = new Dictionary<CacheKey, CacheValue>();

        private static void Shrink()
        {
            if (cache.Count > cacheLimit)
            {
                var expired = DateTime.Now.AddHours(-1);
                var removed = new List<CacheKey>();
                foreach (var key in cache.Keys)
                    if (cache[key].Stamp < expired)
                        removed.Add(key);
                foreach (CacheKey key in removed)
                    cache.Remove(key);
                if (cache.Count > cacheLimit)
                    cacheLimit += 500;
                else
                    cacheLimit -= 250;
                if (cacheLimit < 1000)
                    cacheLimit = 1000;
                if (cache.Count > 20000)
                {
                    var flush = cache.Keys.OrderBy(item => cache[item].Stamp).Take(10000);
                    foreach (var key in flush)
                        cache.Remove(key);
                    cacheLimit = 10500;
                }
            }
        }

        public delegate string ProcessAction();

        public static void Process(BasicHandler handler, ProcessAction process, bool changed = false)
        {
            if (!Enabled)
            {
                process();
                return;
            }
            CacheAction action;
            CacheValue a = null;
            var key = new CacheKey(handler.GetType(), handler.Request.Url.Query);
            lock (cache)
            {
                Shrink();
                if (cache.ContainsKey(key))
                {
                    a = cache[key];
                    if (changed || (DateTime.Now - a.Stamp).TotalMinutes > 15 || String.IsNullOrWhiteSpace(a.Value))
                        action = CacheAction.Rewrite;
                    else
                        action = CacheAction.Reuse;
                }
                else
                    action = CacheAction.Add;
            }
            string result;
            switch (action)
            {
                case CacheAction.Add:
                    result = process();
                    key = new CacheKey(handler.GetType(), handler.Request.Url.Query);
                    lock (cache)
                        cache.Add(key, new CacheValue(result, handler));
                    break;
                case CacheAction.Rewrite:
                    result = process();
                    lock (cache)
                    {
                        a.Value = result;
                        a.Stamp = DateTime.Now;
                        a.Agent = handler.Request.UserAgent;
                        a.Address = handler.Request.UserHostAddress;
                        a.Rewrites++;
                    }
                    break;
                case CacheAction.Reuse:
                    lock (cache)
                    {
                        result = a.Value;
                        a.Agent = handler.Request.UserAgent;
                        a.Address = handler.Request.UserHostAddress;
                        a.Reuses++;
                    }
                    handler.Write(result);
                    break;
            }
        }

        public static void Reset()
        {
            lock (cache)
                cache.Clear();
        }

        public static bool Enabled
        {
            get
            {
                lock (cache)
                    return enabled;
            }
            set
            {
                lock (cache)
                {
                    enabled = value;
                    cache.Clear();
                }
            }
        }

        public class CacheInfo
        {
            internal CacheInfo(CacheKey key, CacheValue value)
            {
                HandlerType = key.handlerType;
                QueryString = key.queryString;
                Stamp = value.Stamp;
                Value = value.Value;
                Agent = value.Agent;
                Address = value.Address;
                Rewrites = value.Rewrites;
                Reuses = value.Reuses;
            }

            public Type HandlerType { get; private set; }
            public string QueryString { get; private set; }
            public DateTime Stamp { get; private set; }
            public string Value { get; private set; }
            public string Agent { get; set; }
            public string Address { get; set; }
            public long Rewrites { get; private set; }
            public long Reuses { get; private set; }
        }

        public static List<CacheInfo> Dump()
        {
            var list = new List<CacheInfo>();
            lock (cache)
            {
                foreach (var key in cache.Keys)
                    list.Add(new CacheInfo(key, cache[key]));
            }
            return list;
        }
    }
}

