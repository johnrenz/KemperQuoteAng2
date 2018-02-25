using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace KdQuoteLibrary.QuoteFlowHelper
{
    [Serializable]
    public class HOIRenterInfo
    {
        public int HOIRenterDeductible { get; set; }
        [XmlIgnore]
        public DateTime HOIRenterEffDate { get; set; }
        public int HOIRenterInculded { get; set; }
        public int HOIRenterLiability { get; set; }
        public decimal HOIRenterPremium { get; set; }
        public int HOIRenterProperty { get; set; }
        public HOIRenterInfo.EnumRenterProvide HOIRenterProvide;
        public string HOMessage;
        public int HONoOfUnit;
        
        public enum EnumRenterProvide
        {
            [XmlEnum("YES")]
            Yes = 0,
            [XmlEnum("NO")]
            No = 1,
        }

    }
}
