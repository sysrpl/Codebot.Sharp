﻿using System;
using System.Web;

namespace Codebot.Web
{
	public abstract class SimplePageHandler : BasicHandler
	{
		protected delegate string RenderEvent();
		protected delegate BasicHandler ErrorEvent(Exception e);

		protected RenderEvent Header;
		protected RenderEvent Footer;
		protected ErrorEvent Error;

		/// <summary>
		/// Sets the content type, adds optional header, footer, and error handling
		/// </summary>
		protected override void Render()
		{
			ContentType = "text/html";
			try
			{
				if (Header != null)
					Write(Header());
				base.Render();
				if (Footer != null)
					Write(Footer());
			}
			catch (Exception e)
			{
				if (Error == null) throw;
				IHttpHandler handler = Error(e) as IHttpHandler;
				if (handler == null) throw;
				Response.Clear();
				handler.ProcessRequest(Context);
			}
		}
	}
}
