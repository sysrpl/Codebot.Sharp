using System;
using System.Linq;
using System.Security.Principal;

namespace Codebot.Web
{
	public class BasicUser : IBasicUser, IPrincipal, IIdentity
	{
		private static IBasicUser anonymous;

		protected delegate IBasicUser AnonymousFactory();

		protected static AnonymousFactory CreateAnonymous;

		public static IBasicUser Anonymous 
		{ 
			get
			{
				if (anonymous == null)
					anonymous = CreateAnonymous();
				return anonymous;
			}
		}

		static BasicUser()
		{
			CreateAnonymous = () => new BasicUser(false, "Anonymous", string.Empty);
		}

		public BasicUser(bool active, string name, string hash)
		{
			Active = active;
			Name = name;
			Hash = hash;
		}

		public bool Active { get; private set; }
		public string Name { get; private set; }
		public string Hash { get; private set; }

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
			return false;
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
