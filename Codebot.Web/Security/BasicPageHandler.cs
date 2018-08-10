using Codebot.Web;

namespace Codebot.Web
{
	public class BasicPageHandler : PageHandler
	{
		public BasicUser User { get { return Context.User as BasicUser; } }

		[MethodPage("login")]
		public void LoginMethod()
		{
			var s = Context.ApplicationInstance as IUserSecurity;
			User.Login(s, Read("name"), Read("password"), Request.UserAgent);
			Redirect("/");
		}

		[MethodPage("logout")]
		public void LogoutMethod()
		{
			var s = Context.ApplicationInstance as IUserSecurity;
			User.Logout(s);
			Redirect("/");
		}
	}
}
