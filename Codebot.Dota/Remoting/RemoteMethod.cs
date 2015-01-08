using System;
using System.Text;
using System.Threading;
using Codebot.Net;
using Codebot.Xml;

namespace Codebot.Dota.Remoting
{
    public class RemoteMethod
    {
        private static int counter = 0;

        public static int Counter { get { return counter; } }

        private StringBuilder method;

        internal void Add(string name, object value)
        {
            method.AppendFormat("&{0}={1}", name, value.ToString());
        }

        public virtual void Prepare(string url, string key)
        {
            method = new StringBuilder(url);
            method.AppendFormat("?format=XML&key={0}", key);
        }

        private const double sigma = 0.001;

        public Document Invoke()
        {
            if (delay >= sigma)
            {
                var seconds = (DateTime.Now - lastInvoke).TotalSeconds;
                if (seconds < delay)
                {
                    var gap = delay - seconds;
                    var milliseconds = (int)(gap * 1000.0);
                    if (milliseconds > 0)
                        Thread.Sleep(milliseconds);
                }
            }
            lastInvoke = DateTime.Now;
            counter++;
            return Download.Xml(method.ToString());
        }

        private static DateTime lastInvoke = DateTime.Now;
        private static double delay = 0;

        public static double Delay
        {
            get
            {
                return delay;
            }
            set
            {
                delay = value < sigma ? 0 : value;
                lastInvoke = DateTime.Now;
            }
        }
    }
}
    