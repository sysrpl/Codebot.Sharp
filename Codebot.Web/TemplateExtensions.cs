using System;
using System.Reflection;
using System.Linq;
using Codebot.Runtime;

namespace Codebot.Web
{
    public static class TemplateExtensions
    {
        public static string LoadTemplate(this MemberInfo info, string name)
        {
            var templates = info.GetCustomAttributes<TemplateAttribute>();
            var template = templates.Where(item => item.Name == name).FirstOrDefault();
            return template.Resource.Load();
        }
    }
}

