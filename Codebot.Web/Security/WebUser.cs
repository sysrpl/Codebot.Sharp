using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace Codebot.Web
{
	public class WebUser : IWebUser, IPrincipal, IIdentity
	{
		private static IWebUser anonymous;

		public static IWebUser Anonymous 
		{ 
			get
			{
				return anonymous;
			}
			set
			{
				if (anonymous == null)
					anonymous = value;
			}
		}

		private List<string> roles;

		public WebUser()
		{
			Active = true;
			Data = null;
			Name = String.Empty;
			Hash = String.Empty;
			roles = new List<string>();
		}

		public bool Active { get; set; }
		public object Data { get; set; }
		public string Name { get; set; }
		public string Hash { get; set; }

		public string Roles
		{ 
			get
			{
				return String.Join(",", roles);
			}
			set
			{
				var values = String.IsNullOrWhiteSpace(value) ? "" : Regex.Replace(value, @"\s+", "").ToLower();
				roles.Clear();
				roles.AddRange(values.Split(','));
			}
		}

		public bool Login(IUserSecurity security, string name, string password, string salt)
		{
			IWebUser user;
			lock (Anonymous)
				user = security.Users.FirstOrDefault(u => u.Name == name);
			if (user == null)
			{
				Security.DeleteCredentials(security.Context);
				return false;
			}
			if (!user.Active || user.Hash != Security.ComputeHash(password))
			{
				Security.DeleteCredentials(security.Context);
				return false;
			}
			Security.WriteCredentials(security.Context, user, salt);
			return true;
		}

		public void Logout(IUserSecurity security)
		{
			Security.DeleteCredentials(security.Context);
		}

		public IWebUser Restore(IUserSecurity security, string salt)
		{
			IWebUser user = null;
			var name = Security.ReadUserName(security.Context);
			var credentials = Security.ReadCredentials(security.Context);
			lock (Anonymous)
				user = security.Users.FirstOrDefault(u => u.Name == name);
			if (user == null)
				return Anonymous;
			return Security.Match(user, salt, credentials) ? user : Anonymous;
		}

		public virtual bool IsInRole(string role)
		{
			return roles.IndexOf(role.ToLower()) > -1;
		}

		public bool IsAdmin { get { return IsInRole("admin"); } }

		public bool IsAnonymous { get { return this == Anonymous; } }

		public IIdentity Identity
		{
			get
			{
				return this;
			}
		}

		public string AuthenticationType
		{
			get
			{
				return "custom";
			}
		}

		public bool IsAuthenticated
		{
			get
			{
				return this != Anonymous;
			}
		}
	}
}
