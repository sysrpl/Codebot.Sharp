using System;
using System.Web;
using System.IO;

namespace Codebot.Web
{
	public static class WebObjectExtensions
	{
		public static string ReadBody(this HttpRequest request)
		{
			using (StreamReader reader = new StreamReader(request.InputStream))
				return reader.ReadToEnd();
		}
	}
}
