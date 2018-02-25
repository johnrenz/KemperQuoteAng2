using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KdQuoteLibrary.QuoteFlowHelper
{
    public class BundledCoverage
    {
        public string header { get; set; }
        public string description { get; set; }
    }
    public class EnhancedCoverage : Coverage
    {
        public List<BundledCoverage> Bundle { get; set; }
    }
}
