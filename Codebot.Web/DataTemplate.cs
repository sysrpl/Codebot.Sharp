using System.Data.Common;
using System.Text;

namespace Codebot.Web
{
	public class DataTemplate : TemplateHandler 
    {
        protected DbDataReader Reader;

        protected abstract class BaseDataObject
        {

            protected abstract void Run();
            protected abstract void SetOwner(DataTemplate owner);

            public void Process(DataTemplate owner)
            {
                SetOwner(owner);
                Run();
            }
        }

        protected abstract class DataObject<T> : BaseDataObject where T : DataTemplate
        {
            protected T Owner { get; private set; }
            protected DbDataReader Reader { get { return Owner.Reader; } }

            protected override void SetOwner(DataTemplate owner)
            {
                Owner = (T)owner;
            }

            protected string HtmlEscape(string s)
            {
                return Owner.HtmlEscape(s);
            }
        }

        protected T Run<T>() where T : BaseDataObject, new()
        {
            var data = new T();
            data.Process(this);
            return data;
        }

        protected StringBuilder FormatRun<T>(string template) where T : BaseDataObject, new()
        {
            var data = new T();
            data.Process(this);
            return template.FormatObject(data);
        }

        protected void FormatRun<T>(string template, StringBuilder buffer) where T : BaseDataObject, new()
        {
            var data = new T();
            data.Process(this);
            template.FormatObject(data, buffer);
        }    
    }
}

