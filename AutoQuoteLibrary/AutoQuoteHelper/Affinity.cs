using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoQuoteLibrary.AutoQuoteHelper
{
    [Serializable]
    public class Affinity
    {
        public string IsAffinity { get; set; }
        public string IsAgent { get; set; }
        public string Logo { get; set; }
        public string Description { get; set; }
    }
}
