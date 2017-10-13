using System.Web;

namespace Codebot.Web
{
	public interface IUser
	{
		bool Active { get; }
		string Name { get; }
		string Hash { get; }
		bool IsAnonymous { get; }
		bool Login(IUserSecurity security, string name, string password, string salt);
		void Logout(IUserSecurity security);
		IUser Restore(IUserSecurity security, string salt);
	}
}
