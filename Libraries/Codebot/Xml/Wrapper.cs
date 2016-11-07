using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Codebot.Xml
{
	public class Wrapper
	{
		internal Wrapper()
		{
		}

		internal Wrapper(object controller)
		{
			if (controller == null)
				throw new NullReferenceException();
			Controller = controller;
		}

		internal object Controller { get; set; }
	}
}
