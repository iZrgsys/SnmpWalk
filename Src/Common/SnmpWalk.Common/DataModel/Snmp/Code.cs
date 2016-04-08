using System;
using System.Xml.Serialization;

namespace SnmpWalk.Common.DataModel.Snmp
{
    [Serializable]
    public class Code
    {
        [XmlElement("Decimal")]
        public int Decimal { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }
    }

    [XmlRoot("Additions")]
    public class Codes
    {
        [XmlArray("codes")]
        [XmlArrayItem("code",typeof(Code))]
        public Code[] Code { get; set; }
    }
}
