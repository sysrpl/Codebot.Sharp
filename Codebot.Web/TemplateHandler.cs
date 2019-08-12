using System.Collections.Generic;
using System.Text;
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
            var changed = false;
            var attributes = GetType().GetCustomAttributes<TemplateAttribute>();
            foreach (var a in attributes)
                foreach(var i in a.Items)
                {
                    var template = IncludeReadDirect(i.Resource, out bool c);
                    templates.Add(i.Name, template);
                    changed = changed || c;
                }
            var cached = GetType().GetCustomAttribute<CachedAttribute>() != null;
            if (cached)
            {
                string process()
                {
                    Run(output, templates);
                    return output.ToString();
                }
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

