using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Codebot.Runtime
{
	public static class StringExtensions
	{
		public static string[] Split(this string value, string s)
		{
			return value.Split(new string[] { s }, StringSplitOptions.None);
		}

		public static string[] SplitLines(this string value)
		{
			return value.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
		}

		public static string PascalCase(this string value)
		{
			if (value == null)
				return null;
			if (value.Length > 1)
				return char.ToUpper(value[0]) + value.Substring(1);
			return value.ToUpper();
		}

		public static string Between(this string value, string start, string end)
		{
			return value.RestOf(start).FirstOf(end);
		}

		public static string RestOf(this string value, string s)
		{
			var i = value.IndexOf(s);
			return i < 0 ? String.Empty : value.Substring(i + s.Length);
		}

		public static string FirstOf(this string value, string s)
		{
			var i = value.IndexOf(s);
			return i < 0 ? String.Empty : value.Substring(0, i);
		}

		public static string SecondOf(this string value, string s)
		{
			return value.FirstOf(s).FirstOf(s);
		}

		public static string LastOf(this string value, string s)
		{
			var a = value.Split(new string[] { s }, StringSplitOptions.None);
			return a.Length > 0 ? a[a.Length - 1] : String.Empty;
		}

		public static string ElementText(this string value, string element)
		{
			return value
				.SecondOf("<" + element + ">")
				.FirstOf("</" + element + ">");
		}

		public static string LoadResourceText(this string s, Assembly assembly)
		{
			var names = assembly.GetManifestResourceNames();
			string resource = names.First(item => item.EndsWith(s));
			using (Stream stream = assembly.GetManifestResourceStream(resource))
			using (StreamReader reader = new StreamReader(stream))
				return reader.ReadToEnd();
		}

		public static string LoadResourceText(this string s)
		{
			return LoadResourceText(s, Assembly.GetCallingAssembly());
		}

		public static string Quote(this string arg)
		{
			return "\"" + arg.Replace("\"", "\\\"") + "\"";
		}

		public static string Quote(this string[] args)
		{
			StringBuilder s = new StringBuilder();
			int i = 0;
			foreach (string arg in args)
			{
				if (i > 0)
					s.Append(" ");
				s.Append(arg.Quote());
				i++;
			}
			return s.ToString();
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

		public static bool IsBlank(this string s)
		{
			return String.IsNullOrWhiteSpace(s);
		}

		public static bool IsEmail(this string s)
		{
			string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
					  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
					  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
			Regex re = new Regex(strRegex);
			return re.IsMatch(s);
		}

		public static bool IsIdentifier(this string s)
		{
			const string first = @"(\p{Lu}|\p{Ll}|\p{Lt}|\p{Lm}|\p{Lo}|\p{Nl})";
			const string body = @"(\p{Mn}|\p{Mc}|\p{Nd}|\p{Pc}|\p{Cf})";
			Regex re = new Regex(string.Format("{0}({0}|{1})*", first, body));
			var nromalized = s.Normalize();
			return re.IsMatch(nromalized);
		}


		public static string Save(this string s, string fileName)
		{
			using (var stream = new StreamWriter(fileName, false))
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

		public static string EscapeHtml(this string s)
		{
			return s
				.Replace("&lt;", "&amp;lt;")
				.Replace("&gt;", "&amp;gt;")
				.Replace("<", "&lt;")
				.Replace(">", "&gt;");
		}

		private static string platformUnknown = "unknown";
		private static string[] platformNames = new string[] { "windows", "linux", "macintosh", "android", "ios" };

		public static string ToPlatform(this string s)
		{
			string platform = s.ToLower();
			foreach (var name in platformNames)
				if (platform.IndexOf(name) > -1)
					return name;
			return platformUnknown;
		}
	}
}
