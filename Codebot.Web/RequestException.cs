using System;

namespace Codebot.Web
{
	public class RequestException : Exception
	{
		public RequestException(int code, string message)
            : base(message)
		{
			Code = code;
		}

		public RequestException(int code, string message, params object[] args)
			: this(code, String.Format(message, args))
		{
		}

		public int Code { get; private set; }

		public static void ThrowBadPost()
		{
			throw new RequestException(405, "POST method expected");
		}

		public static void ThrowBadRequest()
		{
			throw new RequestException(400, "Bad request");
		}

		public static void ThrowBadMethod()
		{
			throw new RequestException(400, "Invalid method");
		}

		public static void ThrowBadArguments()
		{
			throw new RequestException(400, "Invalid arguments");
		}

		public static void ThrowInternalServer()
		{
			throw new RequestException(500, "Internal server error");
		}
	}
}