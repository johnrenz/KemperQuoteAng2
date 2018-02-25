using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KdQuoteLibrary.QuoteFlowHelper
{
    public class Discount
    {
        public string ID { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string ExpandedDesc { get; set; }
        public string Name { get; set; }
        public bool Applied { get; set; }
        public bool Purchased { get; set; }
        public bool CanBeDeleted { get; set; }
        public string Amount { get; set; }
        public decimal AmountNumeric { get; set; }
    }
}
