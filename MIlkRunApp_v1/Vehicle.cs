using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MilkRunApp_v3
{
    public class Vehicle
    {
        [XmlAttribute]
        public string id { get; set; }
        [XmlAttribute]
        public string type { get; set; }
        [XmlAttribute]
        public int capacity { get; set; }
        [XmlAttribute]
        public int speed { get; set; }
    }
}
