using System;
using Codebot.Xml;
using System.Text;
using System.IO;

namespace Codebot.Web
{
	public abstract class DocumentHandler : BasicHandler
	{
		private Document requestDocument;
		public Document RequestDocument
		{
			get
			{
				return requestDocument;
			}
		}

		private Document responseDocument;
		public Document ResponseDocument
		{
			get
			{
				return responseDocument;
			}
		}

		protected void Error(int code, string message)
		{
			Response.StatusCode = code;
			Response.StatusDescription = message;
            responseDocument = new Document();
			Element root = responseDocument.Force("response");
			Filer filer = root.Attributes.Filer;
			filer.Write("code", Response.StatusCode);
			filer.Write("status", "error");
			root.Value = message;
		}

		protected void Error(int code, string message, params object[] args)
		{
			Error(code, String.Format(message, args));
		}

		protected virtual void ValidateRequest()
		{
			Element root = requestDocument.Root;
			if (root.Name != "request")
				RequestException.ThrowBadRequest();
			Filer filer = root.Attributes.Filer;
			if (String.IsNullOrEmpty(filer.ReadString("method")))
				RequestException.ThrowBadMethod();
		}

		private string method;
		protected string Method
		{
			get
			{
				if (method == null)
				{
					Filer filer = requestDocument.Root.Attributes.Filer;
					method = filer.ReadString("method");
				}
				return method;
			}
		}

		protected override void Render()
		{
			Response.ContentEncoding = Encoding.UTF8;
			Response.ContentType = "text/xml"; // ; charset=utf-8
			responseDocument = new Document();
			Element root = responseDocument.Force("response");
			bool documentRead = false;
			try
			{
				if (!IsPost)
					RequestException.ThrowBadPost();
				string s = String.Empty;
				using (StreamReader reader = new StreamReader(Request.InputStream))
					s = reader.ReadToEnd();
				requestDocument = new Document(s);
				documentRead = true;
				ValidateRequest();
				base.Render();
				root = responseDocument.Root;
				if (root.Name != "response")
					RequestException.ThrowInternalServer();
				Filer filer = root.Attributes.Filer;
				filer.Write("code", Response.StatusCode);
				filer.Write("status", Response.StatusDescription);
			}
			catch (RequestException requestException)
			{
				Error(requestException.Code, requestException.Message);
			}
			catch (Exception exception)
			{
				if (documentRead)
					Error(500, "An internal server error of type '{0}' was encountered'",
						exception.GetType().FullName);
				else
					Error(400, "Bad request");
			}
			Response.Write(responseDocument.ToString(Beautiful));
		}

		public bool Beautiful { get; set;  }
	}
}