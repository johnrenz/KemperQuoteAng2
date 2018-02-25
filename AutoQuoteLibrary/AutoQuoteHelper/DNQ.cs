using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoQuoteLibrary.AutoQuoteHelper
{
    [Serializable]
    public class DNQ
    {
        public string Knockout { get; set; }
        public string Template { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public string EmailSent { get; set; }
        public string Filter { get; set; }
    }
}
