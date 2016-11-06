using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;
using System.IO;
using Codebot.Runtime;
using Codebot.Drawing;
using Codebot.Xml;

namespace Codebot.Net
{
	public static class Download
	{
		public static string Text(string url)
		{
			using (WebClient client = new WebClient())
			using (Stream stream = client.OpenRead(url))
			using (StreamReader reader = new StreamReader(stream))
				return reader.ReadToEnd();
		}

		public static string Text(string url, params object[] args)
		{
			return Text(String.Format(url, args));
		}

		public static Document Xml(string url)
		{
			return new Document(Text(url));
		}

		public static Document Xml(string url, params object[] args)
		{
			return new Document(Text(url, args));
		}

		public static Bitmap Image(string url)
		{
			using (WebClient client = new WebClient())
			using (Stream stream = client.OpenRead(url))
				return new Bitmap(stream);
		}

		public static Bitmap ImageCache(string url, string cacheName)
		{
			if (!File.Exists(cacheName))
			{
				using (WebClient client = new WebClient())
				using (Stream input = client.OpenRead(url))
				using (FileStream output = new FileStream(cacheName, FileMode.CreateNew, FileAccess.Write))
					output.Copy(input);
			}
			using (Stream stream = new FileStream(cacheName, FileMode.Open, FileAccess.Read))
				return new Bitmap(stream);
		}
	}
}
