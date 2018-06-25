using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MilkRunApp_v3
{
    public class Demand
    {
        [XmlAttribute]
        public string orderNo { get; set; }      // The name of the servnig or served warehouse
        [XmlAttribute]
        public string from { get; set; }        // The name of the servnig or served warehouse
        [XmlAttribute]
        public string to { get; set; }          // The name of the served or serving station
        [XmlAttribute]
        public string item { get; set; }        // The name of the item, part or raw material   
        [XmlAttribute]
        public int amount { get; set; }         // The space requirement of the demand on the vehicle, expressed in KLT-sizes
        [XmlAttribute]
        public bool ifFinished { get; set; }    // TRUE if the item is a finished good; FALSE if it is raw material
        [XmlAttribute]
        public int cycleTime { get; set; }      // The Kanban-cycle-time of the line in minutes
    }
}
