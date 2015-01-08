using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Codebot.Xml
{
    public class Elements : Nodes<Element>
	{
		protected override IEnumerable GetEnumerable()
		{
			return List;
		}

		protected override XmlNode GetItem(string name)
		{
			return InternalNode.SelectSingleNode(name);
		}

		protected override XmlNode GetItem(int index)
		{
			return List.Item(index);
		}

		internal XmlNodeList List { get; set; }

		internal Elements(XmlNodeList list, XmlNode node) : base(node)
		{
			List = list;
		}

		public override int Count
		{
			get
			{
				return List.Count;
			}
		}
	}
}