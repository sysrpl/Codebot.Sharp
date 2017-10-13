using System.Collections.Generic;
using System.Web;

namespace Codebot.Web
{
	public interface IUserSecurity
	{
		HttpContext Context { get; }
		IBasicUser User { get; }
		IEnumerable<IBasicUser> Users { get; }
	}
}
