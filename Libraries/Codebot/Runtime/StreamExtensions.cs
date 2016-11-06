using System;
using System.IO;

namespace Codebot.Runtime
{
	public static class StreamExtensions
	{

		public static void Copy(this Stream stream, Stream source)
		{
			const int size = 32768;
			byte[] buffer = new byte[size];
			while (true)
			{
				int read = source.Read(buffer, 0, buffer.Length);
				if (read <= 0)
					return;
				stream.Write(buffer, 0, read);
			}
		}
	}
}
