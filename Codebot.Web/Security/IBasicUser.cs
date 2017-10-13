using System.Security.Principal;
using System.Web;

namespace Codebot.Web
{
	public interface IBasicUser : IPrincipal
	{
		bool Active { get; }
		string Name { get; }
		string Hash { get; }
		bool IsAdmin { get; }
		bool IsAnonymous { get; }
		bool Login(IUserSecurity security, string name, string password, string salt);
		void Logout(IUserSecurity security);
		IBasicUser Restore(IUserSecurity security, string salt);
	}
}
