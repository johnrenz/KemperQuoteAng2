using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoQuoteLibrary.AutoQuoteHelper
{
    public class VehicleModel
    {
        public int ID { get; set; }
        public string YearVersionNumber { get; set; }
        public string MakeVersionNumber { get; set; }
        public string ModelVersionNumber { get; set; }
        public string BodyVersionNumber { get; set; }
        public string Description { get; set; }
        public string Year { get; set; }
    }
}