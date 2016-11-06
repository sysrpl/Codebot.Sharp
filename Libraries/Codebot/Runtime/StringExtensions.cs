using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Codebot.Runtime
{
    public static class StringExtensions
    {
        public static string LoadResource(this string s, Assembly assembly)
        {
            string resource = assembly.GetManifestResourceNames().First(item => item.EndsWith(s));
            using (Stream stream = assembly.GetManifestResourceStream(resource))
            using (StreamReader reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        public static string LoadResource(this string s)
        {
            return LoadResource(s, Assembly.GetCallingAssembly());
        }

        public static string Reverse(this string s)
        {
            char[] c = s.ToCharArray();
            Array.Reverse(c);
            return new string(c);
        }

        public static bool Contains(this string s, params string[] items)
        {
            foreach (string item in items)
                if (s.IndexOf(item) > -1)
                    return true;
            return false;
        }

        public static bool IsEmail(this string s)
        {
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                      @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                      @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex re = new Regex(strRegex);
            return re.IsMatch(s);
        }

        public static string Save(this string s, string fileName)
        {
            using (var stream = new StreamWriter(fileName))
                stream.Write(s);
            return s;
        }

        public static string Load(this string s)
        {
            using (var stream = new StreamReader(s))
                return stream.ReadToEnd();
        }

        public static string Ordinal(this int number)
        {
            string suffix = String.Empty;
            int ones = number % 10;
            int tens = (int)Math.Floor(number / 10M) % 10;
            if (tens == 1)
                suffix = "th";
            else
                switch (ones)
            {
                case 1:
                    suffix = "st";
                    break;

                case 2:
                    suffix = "nd";
                    break;

                case 3:
                    suffix = "rd";
                    break;

                default:
                    suffix = "th";
                    break;
            }
            return String.Format("{0}{1}", number, suffix);
        }

        public static string FirstCharToUpper(this string s)
        {
            if (String.IsNullOrEmpty(s))
                return string.Empty;
            return s.First().ToString().ToUpper() + s.Substring(1).ToLower();
        }

    }
}
