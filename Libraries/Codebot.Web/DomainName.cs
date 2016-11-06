using System;
using System.Web;

namespace Codebot.Web
{
	public class DomainName
	{
		private string domainName;
		private string[] items;

		public DomainName()
			: this(HttpContext.Current.Request.ServerVariables["SERVER_NAME"])
		{
		}

		public DomainName(string path)
		{
			path = path ?? String.Empty;
			if (path.IndexOf("://") > 0)
			{
				Uri uri = new Uri(path);
				domainName = uri.Host;
			}
			else
				domainName = path;
			items = domainName.Split('.');
		}

		public static string GetHost(string path)
		{
			DomainName domain = new DomainName(path);
			return domain.Host;
		}

		public static string GetSubDomain(string path)
		{
			DomainName domain = new DomainName(path);
			return domain.SubDomain;
		}

		public static string GetDomain(string path)
		{
			DomainName domain = new DomainName(path);
			return domain.Domain;
		}

		public static string GetTopLevel(string path)
		{
			DomainName domain = new DomainName(path);
			return domain.TopLevel;
		}

		public string Host
		{
			get
			{
				return domainName;
			}
		}

		public string SubDomain
		{
			get
			{
				if (items.Length < 3)
					return String.Empty;
				return items[items.Length - 3];
			}
		}

		public string Domain
		{
			get
			{
				if (items.Length < 2)
					return String.Empty;
				return items[items.Length - 2];
			}
		}

		public string TopLevel
		{
			get
			{
				if (items.Length < 1)
					return String.Empty;
				return items[items.Length - 1];
			}
		}
	}
}
