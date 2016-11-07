using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using Codebot.Runtime;
using Codebot.Xml;
using System.Diagnostics;

namespace Codebot.Web
{
	public abstract class BasicHandler : IHttpHandler
	{
		public delegate string WriteConverter(object item);
		public delegate object QuerySectionsFunc(BasicHandler handler);

		private static Dictionary<string, DateTime> includeLog;
		private static Dictionary<string, string> includeData;

		static BasicHandler()
		{
			includeLog = new Dictionary<string, DateTime>();
			includeData = new Dictionary<string, string>();
		}

		public HttpContext Context { get; private set; }

		public HttpRequest Request { get { return Context.Request; } }

		public HttpResponse Response { get { return Context.Response; } }

		public HttpServerUtility Server { get { return Context.Server; } }

		/// <summary>
		/// Attaches a handler instance to an http context
		/// </summary>
		public void Attach(HttpContext context)
		{
			Context = context;
		}

		/// <summary>
		/// Returns true if the request is uses the POST method
		/// </summary>
		public bool IsPost
		{
			get
			{
				return Context.Request.RequestType.Equals("POST", StringComparison.CurrentCultureIgnoreCase);
			}
		}

		/// <summary>
		/// Returns true if the request is uses the GET method
		/// </summary>
		public bool IsGet
		{
			get
			{
				return Context.Request.RequestType.Equals("GET", StringComparison.CurrentCultureIgnoreCase);
			}
		}

		/// <summary>
		/// Returns true if there is request contains a QUERY
		/// </summary>
		public bool IsQuery
		{
			get
			{
				return Context.Request.QueryString.Count > 0;
			}
		}

		/// <summary>
		/// Returns true if a request contains a FORM
		/// </summary>
		public bool IsForm
		{
			get
			{
				return Context.Request.Form.Count > 0;
			}
		}

		/// <summary>
		/// Returns true if the request is plain, that is not a POST, QUERY, or FORM 
		/// </summary>
		public bool IsPlainRequest
		{
			get
			{
				return !IsPost && !IsQuery & !IsForm;
			}
		}

		/// <summary>
		/// Returns true if the request comes from a local network address
		/// </summary>
		public bool IsLocal
		{
			get
			{
				var address = Request.UserHostAddress;
				return address.Equals("127.0.0.1") || address.StartsWith("192.168.0.") || address.StartsWith("192.168.1.");
			}
		}

		/// <summary>
		/// Returns true if the request comes from a local network address
		/// </summary>
		public bool IsAdmin
		{
			get
			{
				var address = Request.UserHostAddress;
				return address.Equals("127.0.0.1") || address.StartsWith("192.168.0.") || address.StartsWith("192.168.1.");
			}
		}

		/// <summary>
		/// Returns true if the request comes from a small group of known platforms
		/// </summary>
		public bool IsHuman
		{
			get
			{
				return Platform.Length > 0;
			}
		}

		private static string[] platforms = { "windows phone", "windows", "macintosh", "linux", "iphone", "android" };

		/// <summary>
		/// Returns the platform name of a few known operating systems
		/// </summary>
		public string Platform
		{
			get
			{
				string agent = Request.UserAgent.ToLower();
				foreach (var platform in platforms)
					if (agent.Contains(platform))
						return platform;
				return String.Empty;
			}
		}

		/// <summary>
		/// Shortcut to setting or getting the Response ContentType
		/// </summary>
		public string ContentType
		{
			get
			{
				return Context.Response.ContentType;
			}
			set
			{
				Context.Response.ContentType = value;
			}
		}

		public static T Convert<T>(string value)
		{
			return Converter.Convert<string, T>(value);
		}

		public static bool TryConvert<T>(string value, out T result)
		{
			return Converter.TryConvert<string, T>(value, out result);
		}

		public void DeleteCookie(string key)
		{
			if (Context.Request.Cookies[key] != null)
			{
				HttpCookie cookie = new HttpCookie(key);
				cookie.Expires = DateTime.Now.AddDays(-1d);
				Context.Response.Cookies.Add(cookie);
			}
		}

		public void WriteCookie(string key, string value, DateTime? expires = null)
		{
			HttpCookie cookie = new HttpCookie(key);
			cookie.Value = value;
			cookie.Expires = expires.HasValue ? expires.Value : DateTime.Now.AddYears(5);
			Context.Response.Cookies.Add(cookie);
		}

		public void WriteCookie(string key, object value, DateTime? expires = null)
		{
			HttpCookie cookie = new HttpCookie(key);
			cookie.Value = value.ToString();
			cookie.Expires = expires.HasValue ? expires.Value : DateTime.Now.AddYears(5);
			Context.Response.Cookies.Add(cookie);
		}

		public string ReadCookie(string key, string defaultValue = "")
		{
			HttpCookie cookie = Context.Request.Cookies[key];
			return cookie != null ? cookie.Value : defaultValue;
		}

		public T ReadCookie<T>(string name, T defaultValue = default(T))
		{
			T result;
			TryReadCookie(name, out result, defaultValue);
			return result;
		}

		public bool TryReadCookie<T>(string key, out T result, T defaultValue = default(T))
		{
			HttpCookie cookie = Context.Request.Cookies[key];
			if (cookie == null)
			{
				result = defaultValue;
				return false;
			}
			if (TryConvert(key, out result))
			{
				return true;
			}
			else
			{
				result = defaultValue;
				return false;
			}
		}

		/// <summary>
		/// Returns true if a cookie with the key value exists
		/// </summary>
		public bool FindCookie(string key)
		{
			return Context.Request.Cookies[key] != null;
		}

		/// <summary>
		/// Reads max bytes as string from the request input stream
		/// </summary>
		public string ReadStream(int maxBytes = 4194304)
		{
			if (Request.TotalBytes > maxBytes)
				return String.Empty;
			var stream = Request.InputStream;
			byte[] buffer = new byte[stream.Length];
			stream.Read(buffer, 0, buffer.Length);
			return Encoding.UTF8.GetString(buffer);
		}

		/// <summary>
		/// Returns true if query request contains key with a value
		/// </summary>
		public bool QueryKeyExists(string key)
		{
			string s = Context.Request.QueryString[key];
			return !String.IsNullOrWhiteSpace(s);
		}


		/// <summary>
		/// Returns true if form request contains key with a value
		/// </summary>
		public bool FormKeyExists(string key)
		{
			string s = Context.Request.Form[key];
			return !String.IsNullOrWhiteSpace(s);
		}

		/// <summary>
		/// Trys to reads T from the request with a default value
		/// </summary>
		public bool TryRead<T>(string key, out T result, T defaultValue = default(T))
		{
			string s = Context.Request.QueryString[key];
			if (String.IsNullOrEmpty(s))
				s = Context.Request.Form[key];
			s = String.IsNullOrEmpty(s) ? String.Empty : s.Trim();
			if (s.Equals(String.Empty))
			{
				result = defaultValue;
				return false;
			}
			if (TryConvert(s, out result))
			{
				return true;
			}
			else
			{
				result = defaultValue;
				return false;
			}
		}

		/// <summary>
		/// Reads an environment variable
		/// </summary>
		public string ReadVar(string key)
		{
			return Context.Request.ServerVariables[key];
		}

		/// <summary>
		/// Reads T from the request with a default value
		/// </summary>
		public T Read<T>(string key, T defaultValue = default(T))
		{
			T result;
			TryRead(key, out result, defaultValue);
			return result;
		}

		/// <summary>
		/// Reads a string from the request with a default value
		/// </summary>
		public string Read(string key, string defaultValue = "")
		{
			string result;
			TryRead(key, out result, defaultValue);
			return result;
		}

		/// <summary>
		/// Obtains an posted file from the request
		/// </summary>
		public HttpPostedFile ReadUpload(string key)
		{
			return Context.Request.Files.Get(key);
		}

		/// <summary>
		/// Writes text to the response
		/// </summary>
		public void Write(string s)
		{
			if (!Response.IsRequestBeingRedirected)
				Context.Response.Write(s);
		}

		/// <summary>
		/// Writes an array of items to the response
		/// </summary>
		public void Write(string s, params object[] args)
		{
			if (!Response.IsRequestBeingRedirected)
				Context.Response.Write(String.Format(s, args));
		}

		/// <summary>
		/// Writes object to the response
		/// </summary>
		public void Write(object obj)
		{
			if (!Response.IsRequestBeingRedirected)
				Context.Response.Write(obj.ToString());
		}

		/// <summary>
		/// Writes an array of items to the response using a converter
		/// </summary>
		public void Write(WriteConverter converter, params object[] items)
		{
			if (Response.IsRequestBeingRedirected)
				return;
			foreach (object item in items)
				Write(converter(item));
		}

		/// <summary>
		/// Map a physical file path to a server url
		/// </summary>
		public string MapPath(string path)
		{
			//path = Path.DirectorySeparatorChar == '/' ? path.Replace('\\', '/') : path.Replace('/', '\\');
			return Context.Server.MapPath(path);
		}

		/// <summary>
		/// Map a server url to a physical file path
		/// </summary>
		/*protected string ReverseMapPath(string path)
		{
			if (!path.EndsWith("\\"))
				path += "\\";
			path = path.Replace(Request.ServerVariables["APPL_PHYSICAL_PATH"], String.Empty);
			path = path.Replace("\\", "/");
			if (!path.StartsWith("/"))
				path = "/" + path;
			return path;
		}*/

		public string ReverseMapPath(string path)
		{
			path = path.Replace("\\", "/");
			if (!path.EndsWith("/"))
				path += "/";
			string root = Server.MapPath("/");
			root = root.Replace("\\", "/");
			if (!root.EndsWith("/"))
				root += "/";
			string res = "/" + path.Replace(root, "");
			return res;
		}
		/// <summary>
		/// Returns the content type for a file
		/// </summary>
		private static string MapContentType(string fileName)
		{
			string ext = fileName.Split('.').Last().ToLower();
			switch (ext)
			{
				case "avi":
					return "video/avi";
				case "bmp":
					return "image/bmp";
				case "csv":
					return "text/csv";
				case "doc":
					return "application/msword";
				case "docx":
					return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
				case "gif":
					return "image/gif";
				case "htm":
					return "text/html";
				case "html":
					return "text/html";
				case "jpeg":
					return "image/jpeg";
				case "jpg":
					return "image/jpeg";
				case "js":
					return "application/javascript";
				case "json":
					return "application/json";
				case "mp3":
					return "audio/mpeg";
				case "mp4":
					return "video/mp4";
				case "mpeg":
					return "video/mpeg";
				case "mpg":
					return "video/mpeg";
				case "ogg":
					return "application/ogg";
				case "pdf":
					return "application/pdf";
				case "png":
					return "image/png";
				case "ppt":
					return "application/vnd.ms-powerpoint";
				case "pptx":
					return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
				case "qt":
					return "video/quicktime";
				case "swf":
					return "application/x-shockwave-flash";
				case "tif":
					return "image/tiff";
				case "tiff":
					return "image/tiff";
				case "txt":
					return "text/plain";
				case "wav":
					return "audio/x-wav";
				case "wma":
					return "audio/x-ms-wma";
				case "wmv":
					return "audio/x-ms-wmv";
				case "xls":
					return "application/vnd.ms-excel";
				case "xlsx":
					return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				case "xml":
					return "text/xml";
				case "zip":
					return "application/zip";
				default:
					return "application/octet-stream";
			}
		}

		private static Document settings;
		private static Filer filer;

		/// <summary>
		/// Read access to the global settings document
		/// </summary>
		protected Filer Settings
		{
			get
			{
				string data = String.Empty;
				string path = MapPath("/resources");
				string fileName = Path.Combine(path, "settings.xml");
				lock (includeData)
				{
					if (!Directory.Exists(path))
						Directory.CreateDirectory(path);
					if (!File.Exists(fileName))
					{
						var document = new Document();
						document.Root = document.CreateElement("settings");
						document.Save(fileName);
					}
					DateTime lastWrite = File.GetLastWriteTime(fileName);
					bool changed = settings == null;
					if (includeLog.ContainsKey(fileName))
					{
						if (includeLog[fileName].Equals(lastWrite))
							data = includeData[fileName];
						else
						{
							changed = true;
							data = File.ReadAllText(fileName);
							includeLog[fileName] = lastWrite;
							includeData[fileName] = data;
						}
					}
					else
					{
						changed = true;
						data = File.ReadAllText(fileName);
						includeLog.Add(fileName, lastWrite);
						includeData.Add(fileName, data);
					}
					if (filer == null || changed)
					{
						settings = new Document(data);
						filer = settings.Root.Filer;
					}
				}
				return filer;
			}
		}

		/// <summary>
		/// Escapes html characters
		/// </summary>
		/// <param name="s">The string to escape</param>
		/// <returns>Returns a string with html characters escaped</returns>
		public string HtmlEscape(string s)
		{
			s = Server.HtmlEncode(s);
			s = s.Replace("&#39;", "'");
			return s;
		}

		/// <summary>
		/// Returns true if a file exists
		/// </summary>
		/// <param name="fileName">The filename to be mapped</param>
		public bool FileExists(string fileName)
		{
			return File.Exists(MapPath(fileName));
		}

		/// <summary>
		/// Returns true if a folder exists
		/// </summary>
		/// <param name="folder">The folder to be mapped</param>
		public bool FolderExists(string folder)
		{
			return Directory.Exists(MapPath(folder));
		}

		/// <summary>
		/// Read the contents a cached file and indicate if a file has changed
		/// </summary>
		/// <param name="fileName">The file to read</param>
		/// <param name="fileName">A out bool indicating if the file has changed</param>
		/// <returns>Returns the contents of a file</returns>
		public string IncludeRead(string fileName, out bool changed)
		{
			string data = String.Empty;
			fileName = MapPath(fileName);
			lock (includeLog)
			{
				DateTime change = File.GetLastWriteTime(fileName);
				if (includeLog.ContainsKey(fileName))
				{
					if (includeLog[fileName].Equals(change))
					{
						changed = false;
						data = includeData[fileName];
					}
					else
					{
						changed = true;
						data = File.ReadAllText(fileName);
						includeLog[fileName] = change;
						includeData[fileName] = data;
					}
				}
				else
				{
					changed = true;
					data = File.ReadAllText(fileName);
					includeLog.Add(fileName, change);
					includeData.Add(fileName, data);
				}
			}
			return data;
		}

		public string IncludeReadDirect(string fileName)
		{
			bool changed;
			return IncludeRead(fileName, out changed);
		}

		/// <summary>
		/// Read the contents a cached file 
		/// </summary>
		/// <param name="fileName">The file to read</param>
		/// <param name="args">An optional list or items to format</param>
		/// <returns>Returns the contents of a file</returns>
		public string IncludeRead(string fileName, params object[] args)
		{
			bool changed;
			string include = IncludeRead(fileName, out changed);
			int start = include.IndexOf("<%include file=\"");
			int stop = 0;
			while (start > -1)
			{
				stop = include.IndexOf("\"%>", start);
				if (stop < start)
					break;
				string head = include.Substring(0, start);
				string tail = include.Substring(stop + 3);
				start = start + "<%include file=\"".Length;
				stop = stop - start;
				string insert = include.Substring(start, stop);
				include = head + IncludeRead(insert, out changed) + tail;
				start = include.IndexOf("<%include file=\"");
			}
			if (args.Length > 0)
				include = String.Format(include, args);
			return include;
		}

		/// <summary>
		/// Includes a cached file
		/// </summary>
		/// <param name="fileName">File a include</param>
		/// <param name="isTemplate">Option format the include as a template of the current handler</param>
		public void Include(string fileName, bool isTemplate = false)
		{
			string s = IncludeRead(fileName);
			if (isTemplate)
				s = s.FormatObject(this).ToString();
			Write(s);
		}

		/// <summary>
		/// Ends the response and redirects the client to a new page
		/// </summary>
		public void Redirect(string url = "")
		{
			if (url == "")
				url = Request.UrlReferrer.AbsoluteUri;
			Context.Response.Redirect(url, false);
			Context.ApplicationInstance.CompleteRequest();
		}

		/// <summary>
		/// Clears the response and transmits a file using a content type and attachment
		/// </summary>
		private void SendFileData(string fileName, string contentType, bool attachment)
		{
			fileName = MapPath(fileName);
			Context.Response.Clear();
			Context.Response.Buffer = true;
			Context.Response.ContentType = contentType;
			if (attachment)
				Context.Response.AddHeader("Content-Disposition", String.Format("attachment; fileName=\"{0}\"",
								Path.GetFileName(fileName)));
			using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				const int bufferSize = 10240;
				byte[] buffer = new byte[bufferSize];
				int bytesRead = 0;
				do
				{
					bytesRead = stream.Read(buffer, 0, buffer.Length);
					if (bytesRead == bufferSize)
						Context.Response.BinaryWrite(buffer);
					else if (bytesRead > 0)
					{
						Array.Copy(buffer, buffer, bytesRead);
						Context.Response.BinaryWrite(buffer);
					}
				} while (bytesRead == bufferSize);
			}
			Context.Response.End();
		}

		/// <summary>
		/// Clears the response and transmits an attachment file
		/// </summary>
		public void SendAttachment(string fileName)
		{
			SendFileData(fileName, MapContentType(fileName), true);
		}

		/// <summary>
		/// Clears the response and transmits an attachment file using a content type
		/// </summary>
		public void SendAttachment(string fileName, string contentType)
		{
			SendFileData(fileName, contentType, true);
		}

		/// <summary>
		/// Clears the response and transmits a file as a page
		/// </summary>
		public void SendFile(string fileName)
		{
			SendFileData(fileName, MapContentType(fileName), false);
		}

		/// <summary>
		/// Clears the response and transmits a file as a page using a content type
		/// </summary>
		public void SendFile(string fileName, string contentType)
		{
			SendFileData(fileName, contentType, false);
		}

		/// <summary>
		/// Run is invoked by the Render() method
		/// </summary>
		protected abstract void Run();

		/// <summary>
		/// Render is invoked by IHttpHandler.ProcessRequest() and in turn invokes Run() 
		/// </summary>
		protected virtual void Render()
		{
			Run();
		}

		/// <summary>
		/// Process a template into a buffer
		/// </summary>
		protected void InsertTemplate<T>(StringBuilder buffer) where T : TemplateHandler, new()
		{
			T handler = new T();
			handler.ProcessTemplate(Context, buffer);
		}

		/// <summary>
		/// Process a template returning a string
		/// </summary>
		protected StringBuilder InsertTemplate<T>() where T : TemplateHandler, new()
		{
			var buffer = new StringBuilder();
			InsertTemplate<T>(buffer);
			return buffer;
		}

		/// <summary>
		/// Process a template writing it to the response
		/// </summary>
		protected void WriteTemplate<T>() where T : TemplateHandler, new()
		{
			var buffer = InsertTemplate<T>();
			Write(buffer);
		}

		/// <summary>
		/// A basic logging mechanism
		/// </summary>
		public BasicWebLog Log
		{
			get
			{
				return BasicWebLog.DefaultLog;
			}
		}


		/// <summary>
		/// Gets the name of the class
		/// </summary>
		public string PathName
		{
			get
			{
				var items = Context.Request.Url.AbsolutePath.Split('/');
				if (items.Length == 1)
					return "home";
				return "home" + String.Join("-", items, 0, items.Length - 1);
			}
		}

		/// <summary>
		/// Gets the name of the class
		/// </summary>
		public string ClassName
		{
			get
			{
				return GetType().ToString().Split('.').Last();
			}
		}

		/// <summary>
		/// Gets the url of the refering page
		/// </summary>
		public string Referer
		{
			get
			{
				return Request.UrlReferrer.ToString();
			}
		}

		/// <summary>
		/// Gets the title of the response
		/// </summary>
		public virtual string Title
		{
			get
			{
				return "Blank";
			}
		}

		/// <summary>
		/// Gets the content of the response
		/// </summary>
		public virtual string Content
		{
			get
			{
				return "Blank";
			}
		}

		#region IHttpHandler Members

		bool IHttpHandler.IsReusable
		{
			get
			{
				return true;
			}
		}

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			Attach(context);
			PathMapper.Mapper = Server.MapPath;
			Render();
		}

		#endregion
	}
}