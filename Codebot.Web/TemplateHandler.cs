using System.Text;
using System.Web;

namespace Codebot.Web
{
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

        protected virtual void Run(Templates templates, StringBuilder output)
        {
        }

        protected override void Run()
        {
            Run(new Templates(IncludeReadDirect), output);
        }

        public override string ToString()
        {
            if (output == null)
                ProcessTemplate(Context, new StringBuilder());
            return output.ToString();
        }
    }
}

