using System;
using Codebot.Xml;

namespace Codebot.Web
{
	public abstract class DocumentMethod
	{
		private DocumentHandler document;
		protected Element Request { get; private set; }
		protected Element Response { get; private set; }

		private Filer responseFiler;
		protected Filer ResponseFiler
		{
			get
			{
				if (responseFiler == null)
					responseFiler = Response.Filer;
				return responseFiler;
			}
		}

		private Filer requestFiler;
		protected Filer RequestFiler
		{
			get
			{
				if (requestFiler == null)
					requestFiler = Request.Force("params").Filer;
				return requestFiler;
			}
		}

		protected bool ReadBool(string name)
		{
			return RequestFiler.ReadBool(name);
		}

		protected DateTime ReadDate(string name)
		{
			return RequestFiler.ReadDate(name);
		}

		protected int ReadInt(string name)
		{
			return RequestFiler.ReadInt(name);
		}

		protected string ReadString(string name)
		{
			return RequestFiler.ReadString(name);
		}

		protected void Write(string name, object value)
		{
			ResponseFiler.Write(name, value);
		}

		public virtual void Dispatch(DocumentHandler handler)
		{
			document = handler;
			Request = handler.RequestDocument.Root;
			Response = handler.ResponseDocument.Root;
			if (!Execute())
			{
				document.Response.StatusCode = 200;
				Filer filer = Response.Attributes.Filer;
				filer.Write("code", document.Response.StatusCode);
			}
		}

		public static bool IsNull(params object[] args)
		{
			bool result = false;
			foreach (Object item in args)
			{
				if (item == null)
					return true;
				Type type = item.GetType();
				if (type == typeof(string))
					result = String.IsNullOrEmpty(item.ToString());
				else if (type == typeof(Guid))
					result = item.Equals(Guid.Empty);
				if (result)
					break;
			}
			return result;
		}

		public abstract bool Execute();
	}
}