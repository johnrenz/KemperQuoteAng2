using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AutoQuoteLibrary.AutoQuoteHelper
{
    public class VehicleCoverage
    {
        public List<Coverage> Coverages { get; set; }
        public string VehicleNumber { get; set; }
        public string Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
    }
}
