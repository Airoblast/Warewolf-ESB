using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Dev2.Diagnostics
{
    [Serializable]
    public class DebugItemResult : IDebugItemResult, IXmlSerializable
    {
        public DebugItemResultType Type { get; set; }
        public string Label { get; set; }
        public string Variable { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public string GroupName { get; set; }
        public int GroupIndex { get; set; }
        public string MoreLink { get; set; }

        #region IXmlSerializable

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();

            GroupName = reader.GetAttribute("GroupName");
            int idx;
            int.TryParse(reader.GetAttribute("GroupIndex"), out idx);
            GroupIndex = idx;

            while(reader.Read())
            {
                if(reader.IsStartElement("Type"))
                {
                    var result = reader.ReadElementString("Type");
                    DebugItemResultType type;
                    Enum.TryParse(result, out type);
                    Type = type;
                }

                if(reader.IsStartElement("Label"))
                {
                    Label = reader.ReadElementString("Label");
                }

                if(reader.IsStartElement("Variable"))
                {
                    Variable = reader.ReadElementString("Variable");
                }

                if(reader.IsStartElement("Operator"))
                {
                    Value = reader.ReadElementString("Operator");
                }

                if(reader.IsStartElement("Value"))
                {
                    Value = reader.ReadElementString("Value");
                }

                if(reader.IsStartElement("MoreLink"))
                {
                    MoreLink = reader.ReadElementString("MoreLink");
                }

                if(reader.NodeType == XmlNodeType.EndElement && reader.Name == "DebugItemResult")
                {
                    reader.ReadEndElement();
                    break;
                }
            }

        }

        public void WriteXml(XmlWriter writer)
        {
            if(!string.IsNullOrWhiteSpace(GroupName))
            {
                writer.WriteAttributeString("GroupName", GroupName);
            }

            if(GroupIndex != 0)
            {
                writer.WriteAttributeString("GroupIndex", GroupIndex.ToString(CultureInfo.InvariantCulture));
            }

            writer.WriteElementString("Type", Type.ToString());
            writer.WriteElementString("Label", Label);
            writer.WriteElementString("Variable", Variable);
            writer.WriteElementString("Operator", Operator);
            writer.WriteElementString("Value", Value);

            if(!string.IsNullOrWhiteSpace(MoreLink))
            {
                writer.WriteElementString("MoreLink", MoreLink);
            }
        }

        #endregion
    }
}