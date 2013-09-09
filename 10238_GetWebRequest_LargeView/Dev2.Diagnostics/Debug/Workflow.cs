﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Dev2.Diagnostics
{
    public class Workflow : IXmlSerializable
    {
        public IList<DebugState> DebugStates { get; set; } 

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            DebugStates = new List<DebugState>();
            reader.MoveToContent();
            reader.ReadStartElement();
            while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "DebugState")
            {
                var item = new DebugState();
                item.ReadXml(reader);
                DebugStates.Add(item);
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
