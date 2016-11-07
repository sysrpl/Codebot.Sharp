using System;

namespace Codebot.Web
{
    [AttributeUsage(AttributeTargets.All,  AllowMultiple = true)]
    public class TemplateAttribute : Attribute
    {
        public static string TemplateFolder = "/Templates/";
        public static string TemplateExtension = ".template";

        public TemplateAttribute(string name)
        {
            Name = name;
            Resource = TemplateFolder + name + TemplateExtension;
        }

        public string Name { get; private set; }
        public string Resource { get; private set; }
    }
}
