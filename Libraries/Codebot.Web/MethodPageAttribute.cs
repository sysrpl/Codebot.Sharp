using System;

namespace Codebot.Web
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodPageAttribute : Attribute
    {
        public MethodPageAttribute(string methodName)
        {
            ContentType = "text/html; charset=utf-8";
            MethodName = methodName;
        }

        public string ContentType { get; set; }
        public string MethodName { get; set; }
    }
}
