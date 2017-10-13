using System;
using System.IO;

namespace Codebot
{
	public static class Buggit
	{
		public static string Location;

		public static void Logs(string s, params object[] args)
		{
			File.AppendAllText(Location, String.Format(s, args) + Environment.NewLine);
		}
	}
}
