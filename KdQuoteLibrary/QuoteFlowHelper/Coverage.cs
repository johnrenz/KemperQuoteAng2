using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using AutoQuote;

namespace KdQuoteLibrary.QuoteFlowHelper
{
    public class Coverage
    {
        public string CovID { get; set; }
        public string CovCode { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Caption { get; set; }
        public bool IncludeCaptionInLayout { get; set; }
        public string SelectedValue { get; set; }
        public string LongSelectedOption { get; set; }
        public string Premium { get; set; }
        public decimal PremiumNumeric { get; set; }
        public bool IncludePremiumInLayout { get; set; }
        public string HelpText { get; set; }
        public List<Limit> Limits { get; set; }
        public Limit SelectedLimit { get; set; }
        public bool SuppressRendering { get; set; }
        public string CovInputType { get; set; }
        public string WebQuestionID { get; set; }
        public bool Purchased { get; set; }

        public Dictionary<string, DropDownSelection> GetLimitSelections()
        {
            Dictionary<string, DropDownSelection> result = new Dictionary<string, DropDownSelection>();
            foreach (Limit lim in this.Limits)
            {
                if (lim.Value != null)
                    result.Add(lim.Caption, new DropDownSelection() { Value = lim.Value, Description = lim.Caption });
            }
            return result;
        }
        
    }
}
