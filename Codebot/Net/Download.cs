using System;
using System.Drawing;
using System.IO;
using System.Net;
using Codebot.Runtime;
using Codebot.Xml;

namespace Codebot.Net
{
	/// <summary>
	/// The Download static class provides simple methods to retreive text, xml, and images over http.
	/// </summary>
	public static class Download
	{
		/// <summary>
		/// Download content from a url as text and include custom headers in the request
		/// </summary>
		/// <returns>The content as text.</returns>
		/// <param name="url">Location of web resource.</param>
		/// <param name="headers">Custom headers to attach to the request.</param>
        public static string TextWithHeaders(string url, params string[] headers)
        {
            using (WebClient client = new WebClient())
            {
                foreach (var s in headers)
					if (!s.IsBlank())
                    	client.Headers.Add(s);
                using (Stream stream = client.OpenRead(url))
                using (StreamReader reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }

		/// <summary>
		/// Download content from a url as text.
		/// </summary>
		/// <returns>The content as text.</returns>
		/// <param name="url">Location of web resource.</param>
		public static string Text(string url)
		{
			return TextWithHeaders(url);
		}

		/// <summary>
		/// Download content from a formated url as text.
		/// </summary>
		/// <returns>The content as text.</returns>
		/// <param name="url">Location of web resource.</param>
		/// <param name="args">Arguments to format url.</param>
		public static string Text(string url, params string[] args)
		{
			return Text(String.Format(url, args));
		}

		/// <summary>
		/// Download content from a url as an xml document.
		/// </summary>
		/// <returns>The content as an xml document.</returns>
		/// <param name="url">Location of web resource.</param>
		public static Document Xml(string url)
		{
			return new Document(Text(url));
		}

		/// <summary>
		/// Download content from a formatted url as an xml document.
		/// </summary>
		/// <returns>The content as an xml document.</returns>
		/// <param name="url">Location of web resource.</param>
		/// <param name="args">Arguments to format url.</param>
		public static Document Xml(string url, params string[] args)
		{
			return new Document(Text(url, args));
		}

		/// <summary>
		/// Download content from a url as an image.
		/// </summary>
		/// <returns>The content as an image.</returns>
		/// <param name="url">Location of web resource.</param>
		public static Bitmap Image(string url)
		{
			using (WebClient client = new WebClient())
			using (Stream stream = client.OpenRead(url))
				return new Bitmap(stream);
		}

		/// <summary>
		/// Download content from a url as an image using a caching scheme.
		/// </summary>
		/// <returns>The content as an image.</returns>
		/// <param name="url">Location of web resource.</param>
		/// <param name="cacheName">File location and name to cache the image.</param>
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
