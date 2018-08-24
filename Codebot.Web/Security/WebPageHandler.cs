using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codebot.Runtime;

namespace Codebot.Web
{
	public class WebPageHandler<TUser> : PageHandler where TUser : WebUser
	{
		public TUser User { get { return Context.User as TUser; } }

		public override bool IsAdmin
		{
			get
			{
				return User.IsAdmin;
			}
		}

		public override bool IsAuthenticated
		{
			get
			{
				return !User.IsAnonymous;
			}
		}

		protected string UserReadFile(string user, string fileName, string empty = "")
		{
			fileName = $"/private/data/{user}/{fileName}";
			return FileExists(fileName)
				? IncludeRead(fileName).Trim()
				: empty;
		}

		protected void UserWriteFile(string user, string fileName, string content)
		{
			fileName = $"/private/data/{user}/{fileName}";
			Directory.CreateDirectory(Path.GetDirectoryName(fileName));
			File.WriteAllText(fileName, content);
		}

		[MethodPage("login")]
		public void LoginMethod()
		{
			var security = Context.ApplicationInstance as IUserSecurity;
			var name = ReadAny("name", "username", "login");
			var password = Read("password");
			if (String.IsNullOrWhiteSpace(name) || String.IsNullOrWhiteSpace(password))
			{
				Write("FAIL");
				return;
			}
			var success = User.Login(security, name, password, Request.UserAgent);
			var redirect = Read("redirect");
			if (redirect == "true")
				Redirect("/");
			else
				Write(success ? "OK" : "FAIL");
		}

		[MethodPage("logout")]
		public void LogoutMethod()
		{
			var security = Context.ApplicationInstance as IUserSecurity;
			User.Logout(security);
			var redirect = Read("redirect");
			if (redirect == "true")
                Redirect("/");
			else
				Write("OK");
		}

		[MethodPage("users")]
		public void UsersMethod()
		{
			var users = new List<string>() { User.Name };
			if (User.IsAdmin)
			{
				var security = Context.ApplicationInstance as IUserSecurity;
				lock (WebUser.Anonymous)
				{
					var names = security
						.Users
						.Select(user => user.Name)
						.Where(name => name != User.Name)
						.OrderBy(name => name);
					users.AddRange(names);
				}
			}
			var list = String.Join(", ", users.Select(name => $"\"{name}\""));
			Write($"[ {list} ]");
		}
	}
}
