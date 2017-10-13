using System;
using System.Web;
using System.Web.UI;

namespace Codebot.Speech.Web
{

	public class home : System.Web.IHttpHandler
	{

		public void ProcessRequest(HttpContext context)
		{

		}

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}
	}
}
