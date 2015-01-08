using System;
using System.Text;
using System.Collections.Generic;
using Codebot.Runtime;

namespace Codebot.Web
{
    public static class Template
    {
        public static string Load(string templateName)
        {
            string fileName = TemplateAttribute.TemplateFolder + templateName + TemplateAttribute.TemplateExtension;
            return fileName.Load();
        }

        public static StringBuilder Format(string templateName, object data, StringBuilder buffer = null)
        {
            return Load(templateName).FormatObject(data, buffer);
        }
    }
}
