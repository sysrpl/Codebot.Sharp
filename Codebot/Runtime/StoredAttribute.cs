using System;

namespace Codebot.Runtime
{
	public class StoredAttribute
	{
		public StoredAttribute(string name = "")
		{
			Name = name;
		}

		public string Name { private set; get; }
	}
}
