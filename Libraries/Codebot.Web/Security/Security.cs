using System;
using System.Text;
using System.Security.Cryptography;
using System.Web;
using System.Linq;

namespace Codebot.Web
{
	public static class UserSecurity
	{
		private static HMACSHA256 hmac;

		private static Random random = new Random();

		public static string RandomSecretKey(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var secret = new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[random.Next(s.Length)]).ToArray());
			SecretKey(secret);
			return secret;
		}

		public static void SecretKey(string key)
		{
			var bytes = Encoding.UTF8.GetBytes(key);
			hmac = new HMACSHA256(bytes);
		}

		public static string ComputeHash(string value)
		{
			var bytes = Encoding.UTF8.GetBytes(value);
			bytes = hmac.ComputeHash(bytes);
			return Convert.ToBase64String(bytes);
		}

		private static readonly string key = "security-credentials";

		public static string ReadCredentials(HttpContext context)
		{
			HttpCookie cookie = context.Request.Cookies[key];
			return cookie != null ? cookie.Value : "";
		}

		public static void WriteCredentials(HttpContext context, string salt, BasicUser user)
		{
			HttpCookie cookie = new HttpCookie(key);
			string s = user.Credentials(salt);
			cookie.Value = s;
			cookie.Expires = DateTime.Now.AddYears(1);
			context.Response.Cookies.Add(cookie);
		}

		public static void DeleteCredentials(HttpContext context)
		{
			if (context.Request.Cookies[key] != null)
			{
				HttpCookie cookie = new HttpCookie(key);
				cookie.Expires = DateTime.Now.AddDays(-1d);
				context.Response.Cookies.Add(cookie);
			}
		}

		public static string Credentials(IUser user, string salt)
		{
			return user.Login + "-" + ComputeHash(salt + user.Login) + "-" + user.Hash;
		}

		public static bool Match(IUser user, string salt, string credentials)
		{
			if (!user.Active)
				return false;
			var items = credentials.Split('-');
			if (items.Length != 3)
				return false;
			if (items[0] != user.Login)
				return false;
			return (user.Hash == items[2] && ComputeHash(salt + user.Login) == items[1]);
		}
	}
}
