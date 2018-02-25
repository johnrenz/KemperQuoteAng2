using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace KdQuoteLibrary.QuoteFlowHelper
{
    [Serializable]
    public class Vehicle
    {
        [XmlAttribute]
        public string SLICE { get; set; }
        public string AltGarZipCode1 { get; set; }
        public string CommuteZip { get; set; }
    }
}
