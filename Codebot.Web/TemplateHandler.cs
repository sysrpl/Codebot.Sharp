using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Web;
using Codebot.Runtime;

namespace Codebot.Web
{
    public class Templates : Dictionary<string, string> { }

    public class TemplateHandler : BasicHandler
    {
        private StringBuilder output;

        public void ProcessTemplate(HttpContext context)
        {
            ProcessTemplate(context, new StringBuilder());
            Write(output.ToString());
        }

        public void ProcessTemplate(HttpContext context, StringBuilder buffer)
        {
            output = buffer;
            (this as IHttpHandler).ProcessRequest(context);
        }

        protected virtual void Run(StringBuilder output, Templates templates)
        {
        }

        protected override void Run()
        {
            Templates templates = new Templates();
            var attributes = GetType().GetCustomAttributes<TemplateAttribute>();
            var changed = false;
            foreach (var attribute in attributes)
            {
                var c = false;
                var template = IncludeRead(attribute.Resource, out c);
                templates.Add(attribute.Name, template);
                changed = changed || c;
            }
            var cached = GetType().GetCustomAttribute<CachedAttribute>() != null;
            if (cached)
            {
                WebCache.ProcessAction process = () =>
                {
                    Run(output, templates);
                    return output.ToString();
                };
                WebCache.Process(this, process, changed);
            }
            else
                Run(output, templates);
        }

        public override string ToString()
        {
            if (output == null)
                ProcessTemplate(Context, new StringBuilder());
            return output.ToString();
        }
    }
}

