using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace Codebot.Web
{
	public class BasicUser : IBasicUser, IPrincipal, IIdentity
	{
		private static IBasicUser anonymous;

		protected delegate IBasicUser AnonymousFactory();

		private static AnonymousFactory createAnonymous;

		protected static AnonymousFactory CreateAnonymous
		{
			get
			{
				return createAnonymous;
			}
			set
			{
				if (createAnonymous == null)
					createAnonymous = value;
			}
		}

		public static IBasicUser Anonymous 
		{ 
			get
			{
				if (anonymous == null)
					anonymous = CreateAnonymous();
				return anonymous;
			}
		}

		public static bool Init()
		{
			CreateAnonymous = () => new BasicUser(false, "Anonymous", string.Empty);
			return Anonymous is BasicUser;
		}

		private List<string> roleList;
		private string roles;

		public BasicUser(bool active, string name, string hash, string roles = "")
		{
			Active = active;
			Data = null;
			Name = name;
			Hash = hash;
			Roles = String.IsNullOrWhiteSpace(roles) ? "" : Regex.Replace(roles, @"\s+", "");
			roleList = Roles.Split(',').ToList();
		}

		public bool Active { get; set; }
		public object Data { get; set; }
		public string Name { get; private set; }
		public string Hash { get; private set; }

		public string Roles
		{ 
			get
			{
				return roles;
			}
			set
			{
				roles = String.IsNullOrWhiteSpace(value) ? "" : Regex.Replace(value, @"\s+", "").ToLower();
			}
		}

		public bool Login(IUserSecurity security, string name, string password, string salt)
		{
			IBasicUser user;
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

		public IBasicUser Restore(IUserSecurity security, string salt)
		{
			IBasicUser user = null;
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
			return roleList.IndexOf(role.ToLower()) > -1;
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
