using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Codebot.Runtime;
using Codebot.Xml;

namespace Codebot.Web
{
	public class BasicApplication : HttpApplication, IUserSecurity 
	{
		protected delegate IWriter DefaultUser();

		protected static string Hasher(string value)
		{
			return Security.ComputeHash(value);
		}

		protected virtual void GenerateDefaultUsers(DefaultUser defaultUser)
		{
			var writer = defaultUser();
			writer.WriteBool("active", true);
			writer.Write("name", "admin");
			writer.Write("hash", Hasher("admin"));
			writer.Write("roles", "admin,user");
		}

		protected virtual BasicUser CreateUser(string name, string hash, string roles)
		{
			return new BasicUser(true, name, hash, roles);
		}

		protected virtual void ReadUser(IReader reader, out BasicUser user)
		{
			user = new BasicUser(
				reader.ReadBool("active"),
				reader.ReadString("name"),
				reader.ReadString("hash"),
				reader.ReadString("roles"));
		}

		protected virtual void WriteUser(IWriter writer, BasicUser user)
		{
			writer.WriteBool("active", user.Active);
			writer.Write("name", user.Name);
			writer.Write("hash", user.Hash);
			writer.Write("roles", user.Roles);
		}

		private static readonly string securityFile = "/private/users.xml";

		public List<BasicUser> Users { get; private set; }

		public BasicApplication()
		{
			Users = new List<BasicUser>();
		}

		HttpContext IUserSecurity.Context
		{
			get
			{
				return Context;
			}
		}

		IBasicUser IUserSecurity.User
		{
			get
			{
				return Context.User as IBasicUser;
			}
		}

		IEnumerable<IBasicUser> IUserSecurity.Users
		{
			get
			{
				return Users;
			}
		}

		public bool AddUser(string name, string password, string roles = "user")
		{
			if (!Context.User.IsInRole("admin"))
				return false;
			if (String.IsNullOrWhiteSpace(name) || String.IsNullOrWhiteSpace(password))
				return false;
			name = name.Trim();
			if (!NameCheck.IsValidUserName(name))
				return false;
			var lowerName = name.ToLower();
			password = password.Trim();
			if (!NameCheck.IsValidPassword(password))
				return false;
			lock (Users)
			{
				var user = Users.FirstOrDefault(u => u.Name.ToLower() == lowerName);
				if (user != null)
					return false;
				var doc = new Document();
				var fileName = Context.Server.MapPath(securityFile);
				doc.Load(fileName);
				var filer = doc.Force("security/users").Nodes.Add("user").Filer;
				var hash = Hasher(password);
				user = CreateUser(name, hash, roles);
				WriteUser(filer, user);
				doc.Save(fileName, true);
				Users.Add(user);
			}
			return true;
		}

		public bool DeleteUser(string name)
		{
			if (!Context.User.IsInRole("admin"))
				return false;
			if (String.IsNullOrWhiteSpace(name))
				return false;
			name = name.Trim();
			if (!NameCheck.IsValidUserName(name))
				return false;
			var lowerName = name.ToLower();
			lock (Users)
			{
				var user = Users.FirstOrDefault(u => u.Name.ToLower() == lowerName);
				if (user == null)
					return false;
				if (user.IsInRole("admin"))
					return false;
				Users.Remove(user);
				var doc = new Document();
				var fileName = Context.Server.MapPath(securityFile);
				doc.Load(fileName);
				var node = doc.Root.FindNode("users/user[name='" + name + "']");
				if (node != null)
				{
					node.Parent.Nodes.Remove(node);
					doc.Save(fileName);
				}
			}
			return true;
		}

		private void CreateConfig(Document doc)
		{
			var filer = doc.Force("security").Filer;
			var secret = filer.ReadString("secret");
			if (String.IsNullOrWhiteSpace(secret))
			{
				secret = Security.RandomSecretKey(32);
				filer.WriteString("secret", secret);
			}
			Security.SecretKey(secret);
		}

		private void CreateUsers(Document doc)
		{
			var nodes = doc.Force("security/users").Nodes;
			if (nodes.Count == 0)
				GenerateDefaultUsers(() => nodes.Add("user").Filer);
			foreach (var node in nodes)
			{
				if (node.Name != "user")
					continue;
				var filer = node.Filer;
				BasicUser user;
				ReadUser(filer, out user);
				if (!user.Active)
					continue;
				Users.Add(user);
			}
		}

		protected void OnApplicationStart()
		{
			BasicUser.Init();
			string fileName = Context.Server.MapPath(securityFile);
			Document doc = new Document();
			FileInfo fileInfo = new FileInfo(fileName);
			if (fileInfo.Exists)
				doc.Load(fileName);
			else
	            Directory.CreateDirectory(fileInfo.Directory.FullName);				
			CreateConfig(doc);
			CreateUsers(doc);
			doc.Save(fileName);
		}

		protected void OnApplicationBeginRequest()
		{
			Context.User = BasicUser.Anonymous.Restore(this, Request.UserAgent);
		}

		public void Application_Start()
		{
			OnApplicationStart();
		}

		public void Application_BeginRequest()
		{
			OnApplicationBeginRequest();
		}
	}
}
