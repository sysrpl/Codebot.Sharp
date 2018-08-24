using System.Collections.Generic;
using System.Web;

namespace Codebot.Web
{
	public interface IUserSecurity
	{
		HttpContext Context { get; }
		IWebUser User { get; }
		IEnumerable<IWebUser> Users { get; }
	}
}
