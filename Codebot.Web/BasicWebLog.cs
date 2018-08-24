using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Codebot.Web
{
    public class BasicWebLog
    {
        public static BasicWebLog DefaultLog = new BasicWebLog();
		private static Object locker = new Object();

        private List<BasicWebLogInfo> data;

        public BasicWebLog()
        {
            data = new List<BasicWebLogInfo>();
            data.Capacity = 1001;
        }

        public void Reset()
        {
            lock (locker)
                data.Clear();
            data.Capacity = 1001;
        }

        public void Add(BasicHandler handler)
        {
            var info = new BasicWebLogInfo(handler);
            lock (locker)
            {
                if (data.Count > 1000)
                    data.RemoveRange(0, 500);
                data.Capacity = 1001;
                data.Add(info);
            }
        }

        public List<BasicWebLogInfo> Dump()
        {
            IEnumerable<BasicWebLogInfo> query;
            lock (locker)
                query = data.Select(item => item.Clone());
            return query.Reverse().Take(500).ToList();
        }
    }

    public class BasicWebLogInfo
    {
        public BasicWebLogInfo(BasicHandler handler)
        {
            Stamp = DateTime.Now;
            Url = handler.Request.Url.AbsoluteUri;
            Address = handler.Request.UserHostAddress;
            Agent = handler.Request.UserAgent;
            var referer = handler.Request.UrlReferrer;
            Referer = referer == null ? "" : referer.AbsoluteUri;
            Platform = handler.Request.Browser.Platform;
        }

        public DateTime Stamp { get; private set; }
        public string Url { get; private set; }
        public string Address { get; private set; }
        public string Agent { get; private set; }
        public string Referer { get; private set; }
        public string Platform { get; private set; }

        public BasicWebLogInfo Clone()
        {
            return (BasicWebLogInfo)MemberwiseClone();
        }
    }
}

