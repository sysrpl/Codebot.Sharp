using System;
using System.Linq;
using Codebot.Runtime;

namespace Codebot.Web
{
	public class PageHandler : BasicHandler
    {
        public delegate void WebMethod();

        /// <summary>
        /// Invoked when no default page is found
        /// </summary>
        protected virtual void EmptyPage()
        {
        }

        private void InvokeDefaultPage()
        {
            var page = GetType().GetCustomAttribute<DefaultPageAttribute>(true);
            if (page != null)
            {
                var logged = GetType().GetCustomAttribute<LoggedAttribute>(true);
                if (logged != null)
                    Log.Add(this);
                ContentType = page.ContentType;
                Include(page.FileName, page.IsTemplate);
                return;
            }
            EmptyPage();
        }

        /// <summary>
        /// Invoked when no method is found
        /// </summary>
        protected virtual void EmptyMethod(string methodName)
        {
			InvokeDefaultPage();
        }

        private void InvokeMethod(string methodName)
        {
            foreach (var method in GetType().GetMethods())
            {
                var attribute = method.GetCustomAttributes<MethodPageAttribute>(true)
                    .Where(a => a.MethodName.ToLower() == methodName)
                    .FirstOrDefault();
                if (attribute != null)
                {
                    ContentType = attribute.ContentType;
                    var pageMethod = (WebMethod)Delegate.CreateDelegate(typeof(WebMethod), this, method);
                    pageMethod();
                    return;
                }
            }
            EmptyMethod(methodName);
        }

        /// <summary>
        /// Sets the content type, adds optional header, footer, and error handling
        /// </summary>
        protected override void Run()
        {
            try
            {
                var methodName = Read("method", "").ToLower();
                if (methodName.Length == 0)
                    InvokeDefaultPage();
                else
                    InvokeMethod(methodName);
            }
            catch (Exception e)
            {
                if (Settings.ReadBool("debug"))
                    Write("<code>{0}\n{1}\n{2}</code>", e.GetType().FullName, (HtmlString)e.Message, e.StackTrace);
                else
                    throw e;
            }
        }
    }
}
