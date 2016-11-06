using System;
using System.Web;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Codebot.Runtime;

namespace Codebot.Web
{
    public class PageHandler : BasicHandler
    {
        public delegate void WebMethod();

        private void InvokeDefaultPage()
        {
            var page = GetType().GetCustomAttribute<DefaultPageAttribute>(true);
            if (page != null)
            {
                ContentType = page.ContentType;
                Include(page.FileName, page.IsTemplate);
            }
        }

        private void InvokePageMethod(string methodName)
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
        }

        /// <summary>
        /// Sets the content type, adds optional header, footer, and error handling
        /// </summary>
        protected override void Run()
        {
            try
            {
                var method = Read("method", "").ToLower();
                if (method.Length == 0)
                    InvokeDefaultPage();
                else
                    InvokePageMethod(method);
            }
            catch (Exception e)
            {
                if (Settings.ReadBool("debug"))
                    Write("<code>{0}\n{1}\n{2}</code>", e.GetType().FullName, (HtmlString)e.Message, e.StackTrace);
                else
                    throw;
            }
        }
    }
}
