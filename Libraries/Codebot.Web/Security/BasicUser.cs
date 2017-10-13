using System;
using System.Text;
using System.Security.Cryptography;
using System.Web;
using Codebot.Web;

namespace Codebot.Blob.Web
{
	public class BasicUser : IUser
	{
		public static string ComputeHash(string value)
		{
			var sha = SHA256.Create();
			var bytes = Encoding.UTF8.GetBytes(value);
			bytes = sha.ComputeHash(bytes);
			return Convert.ToBase64String(bytes);
		}

		private static readonly string key = "credentials";

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
			cookie.Expires = DateTime.Now.AddYears(5);
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

		public BasicUser(bool active, string login, string hash)
		{
			Active = active;
			Login = login;
			Hash = hash;
		}

		public bool Active { private set; get; }
		public string Login { private set; get; }
		public string Hash { private set; get; }

		public string Credentials(string salt)
		{
			return Login + "-" + ComputeHash(salt) + "-" + ComputeHash(Login) + "-" + Hash;
		}

		public bool Match(string salt, string credentials)
		{
			if (!Active)
				return false;
			var items = credentials.Split('-');
			if (items.Length != 4)
				return false;
			if (items[0] != Login)
				return false;
			return (Hash == items[3] && ComputeHash(Login) == items[2] && ComputeHash(salt) != items[1]);
		}
	}
}
