using System;
using System.Net;

namespace Codebot
{
	public class CookieWebClient : WebClient
	{
		public CookieWebClient()
		{
			CookieContainer = new CookieContainer();
			ResponseCookies = new CookieCollection();
		}

		public CookieContainer CookieContainer { get; private set; }
		public CookieCollection ResponseCookies { get; private set; }

		protected override WebRequest GetWebRequest(Uri address)
		{
			var request = (HttpWebRequest)base.GetWebRequest(address);
			request.CookieContainer = CookieContainer;
			return request;
		}

		protected override WebResponse GetWebResponse(WebRequest request)
		{
			var response = (HttpWebResponse)base.GetWebResponse(request);
			ResponseCookies = response.Cookies;
			return response;
		}
	}}
