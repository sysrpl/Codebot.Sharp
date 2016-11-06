using System;
using System.Xml;
using Codebot.Runtime;

namespace Codebot.Xml
{
	public abstract class Filer : Wrapper
	{
		internal Filer(XmlNode node)
			: base(node)
		{
		}

		internal XmlNode InternalNode
		{
			get
			{
				return (XmlNode)Controller;
			}
		}

		public static explicit operator Filer(XmlNode node)
		{
			if (node is XmlElement)
				return new ElementFiler(node);
			if (node is XmlAttribute)
				return new AttributeFiler(node.ParentNode);
			throw new ArgumentException();
		}

		public T Read<T>(string name, T value, bool stored)
		{
			T result;
			bool parsed = Converter.TryConvert(ReadValue(name, stored), out result);
			if (stored)
				if (parsed)
					WriteValue(name, result.ToString());
				else
					WriteValue(name,  value.ToString());
			return parsed ? result : value;
		}

		public T Read<T>(string name, T value)
		{
			return Read<T>(name, value, true);
		}

		public T Read<T>(string name)
		{
			return Read<T>(name, default(T), true);
		}

		public void Write(string name, object value)
		{
			WriteValue(name, value.ToString());
		}

		public string[] ReadValues(params string[] args)
		{
			string[] values = (string[])Array.CreateInstance(typeof(string), args.Length);
			for (int i = 0; i < (args.Length); i++)
			{
				values[i] = ReadString(args[i]);
			}
			return values;
		}

		public DateTime ReadDate(string name, DateTime value, bool stored)
		{
			DateTime result;
			if (!DateTime.TryParse(ReadValue(name, stored), out result))
			{
				result = value;
				if (stored) WriteValue(name, result.ToString());
			}
			return result;
		}

		public DateTime ReadDate(string name, DateTime value)
		{
			return ReadDate(name, value, true);
		}

		public DateTime ReadDate(string name)
		{
			return ReadDate(name, DateTime.Now, true);
		}

		public void WriteDate(string name, DateTime value)
		{
			WriteValue(name, value.ToString());
		}

		public int ReadInt(string name, int value, bool stored)
		{
			int result;
			bool parsed = int.TryParse(ReadValue(name, value.ToString(), stored), out result);
			if (!parsed)
			{
				result = value;
				if (stored) WriteValue(name, result.ToString());
			}
			return result;
		}

		public int ReadInt(string name, int value)
		{
			return ReadInt(name, value, true);
		}

		public int ReadInt(string name)
		{
			return ReadInt(name, 0, true);
		}

		public void WriteInt(string name, int value)
		{
			WriteValue(name, value.ToString());
		}

        public long ReadLong(string name, long value, bool stored)
        {
            long result;
            bool parsed = long.TryParse(ReadValue(name, value.ToString(), stored), out result);
            if (!parsed)
            {
                result = value;
                if (stored) WriteValue(name, result.ToString());
            }
            return result;
        }

        public long ReadLong(string name, long value)
        {
            return ReadLong(name, value, true);
        }

        public long ReadLong(string name)
        {
            return ReadLong(name, 0, true);
        }

        public void WriteLong(string name, long value)
        {
            WriteValue(name, value.ToString());
        }

		public string ReadString(string name, string value, bool stored)
		{
			string result = ReadValue(name, value, stored);
			if (result.Length == 0) result = value;
			return result;
		}

		public string ReadString(string name, string value)
		{
			return ReadString(name, value, true);
		}

		public string ReadString(string name)
		{
			return ReadString(name, string.Empty, true);
		}

		public void WriteString(string name, string value)
		{
			WriteValue(name, value);
		}

		public bool ReadBool(string name, bool value, bool stored)
		{
			string s = ReadValue(name, stored).ToUpper();
			bool result;
			switch (s)
			{
				case "Y":
				case "YES":
				case "T":
				case "TRUE":
				case "1":
					result = true;
					break;
				case "N":
				case "NO":
				case "F":
				case "FALSE":
				case "0":
					result = false;
					break;
				default:
					result = value;
					break;
			}
			if (stored) WriteValue(name, result ? "Y" : "N");
			return result;
		}

		public bool ReadBool(string name, bool value)
		{
			return ReadBool(name, value, true);
		}

		public bool ReadBool(string name)
		{
			return ReadBool(name, false, true);
		}

		public void WriteBool(string name, bool value)
		{
			WriteValue(name, value ? "Y" : "N");
		}

		public void WriteBool(string name, object value)
		{
			string s = value.ToString().ToUpper();
			switch (s)
			{
				case "Y":
				case "YES":
				case "T":
				case "TRUE":
				case "1":
					WriteValue(name, "Y");
					break;
				default:
					WriteValue(name, "N");
					break;
			}
		}

		protected string ReadValue(string name, bool stored)
		{
			return ReadValue(name, string.Empty, stored);
		}

		protected abstract string ReadValue(string name, string value, bool stored);

		protected abstract void WriteValue(string name, string value);
	}
}
