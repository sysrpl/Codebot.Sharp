﻿using System;

namespace Codebot.Web
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultPageAttribute : Attribute
    {
        public DefaultPageAttribute(string fileName)
        {
            ContentType = "text/html; charset=utf-8";
            FileName = fileName;
            IsTemplate = false;
        }

        public string ContentType { get; set; }
        public string FileName { get; set; }
        public bool IsTemplate { get; set; }
    }
}
    