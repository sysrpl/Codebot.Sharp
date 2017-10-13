using System;
using System.Xml;

namespace Codebot.Storage
{
    /// <summary>
    /// Represents an Amazon Storage user.
    /// </summary>
    public sealed class Identity
    {
        public string ID { get; private set; }
        public string DisplayName { get; private set; }

        internal Identity(XmlReader reader)
        {
            if (reader.IsEmptyElement)
                throw new Exception("Expected a non-empty <Owner> element.");

            // Example:
            // <Owner>
            //     <ID>bcaf1ffd86f41caff1a493dc2ad8c2c281e37522a640e161ca5fb16fd081034f</ID>
            //     <DisplayName>webfile</DisplayName>
            // </Owner>
			reader.ReadStartElement("Owner");
			this.ID = reader.ReadElementContentAsString("ID", "");
			if (reader.Name == "DisplayName")
				this.DisplayName = reader.ReadElementContentAsString("DisplayName", "");
			reader.ReadEndElement();        }
    }
}
