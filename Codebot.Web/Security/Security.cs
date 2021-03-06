using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Codebot.Web
{
	public static class Security
	{
		private static string secretKey;

		private static Random random;

		static Security()
		{
			random = new Random();
			RandomSecretKey(32);
		}

		public static string RandomSecretKey(int length)
		{
			const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
			var secret = new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[random.Next(s.Length)]).ToArray());
			SecretKey(secret);
			return secret;
		}

		public static void SecretKey(string key)
		{
			secretKey = key;
		}

		public static string ComputeHash(string value)
		{
			var bytes = Encoding.UTF8.GetBytes(value);
			var key = Encoding.UTF8.GetBytes(secretKey);
			using (var hmac = new HMACSHA256(key))
				bytes = hmac.ComputeHash(bytes);
			return Convert.ToBase64String(bytes);
		}

		private const string cookieName = "security-credentials";

		public static string ReadUserName(HttpContext context)
		{
			var s = ReadCredentials(context);
			return s.Split(':').FirstOrDefault();
		}

		public static string ReadCredentials(HttpContext context)
		{
			HttpCookie cookie = context.Request.Cookies[cookieName];
			return cookie != null ? cookie.Value : "";
		}

		public static void WriteCredentials(HttpContext context, IWebUser user, string salt)
		{
			HttpCookie cookie = new HttpCookie(cookieName);
			string s = Credentials(user, salt);
			cookie.Value = s;
			cookie.Expires = DateTime.Now.AddYears(1);
			context.Response.Cookies.Add(cookie);
		}

		public static void DeleteCredentials(HttpContext context)
		{
			if (context.Request.Cookies[cookieName] != null)
			{
				HttpCookie cookie = new HttpCookie(cookieName);
				cookie.Expires = DateTime.Now.AddDays(-1d);
				context.Response.Cookies.Add(cookie);
			}
		}

		public static string Credentials(IWebUser user, string salt)
		{
			return user.Name + ":" + ComputeHash(salt + user.Name + user.Hash);
		}

		public static bool Match(IWebUser user, string salt, string credentials)
		{
			if (!user.Active)
				return false;
			var items = credentials.Split(':');
			if (items.Length != 2)
				return false;
			if (items[0] != user.Name)
				return false;
			return (items[1] == ComputeHash(salt + user.Name + user.Hash));
		}
	}
}
