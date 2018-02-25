using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KdQuoteLibrary.QuoteFlowHelper
{
    [Serializable]
    public class Vehicles
    {
        public Vehicle[] Vehicle { get; set; }
        public int IpVehicleCt { get; set; }
    }
}
