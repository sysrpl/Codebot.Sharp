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

		public static void DeleteCookie(this HttpContext context, string key)
		{
			if (context.Request.Cookies[key] != null)
			{
				HttpCookie cookie = new HttpCookie(key);
				cookie.Expires = DateTime.Now.AddDays(-1d);
				context.Response.Cookies.Add(cookie);
			}
		}

		public static string ReadCookie(this HttpContext context, string key, string defaultValue = "")
		{
			HttpCookie cookie = context.Request.Cookies[key];
			return cookie != null ? cookie.Value : defaultValue;
		}

		public static void WriteCookie(this HttpContext context, string key, string value, DateTime? expires = null)
		{
			HttpCookie cookie = new HttpCookie(key);
			cookie.Value = value;
			cookie.Expires = expires.HasValue ? expires.Value : DateTime.Now.AddYears(5);
			context.Response.Cookies.Add(cookie);
		}
	}
}
