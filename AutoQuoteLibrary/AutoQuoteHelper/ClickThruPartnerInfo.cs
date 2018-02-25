using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoQuoteLibrary.AutoQuoteHelper
{
    [Serializable]
    public class ClickThruPartnerInfo
    {
        public string CTID { get; set; }
        public string Custom { get; set; }
        public string MarketKeyCode { get; set; }
        public string AMFAccountNumber { get; set; }
        public Affinity Affinity { get; set; }
        public string Keywords { get; set; }
        public string HTTPReferrer { get; set; }
        public string SalesPhone { get; set; }
        public string SalesHours { get; set; }
        public string LandingPage { get; set; }
    }
}
