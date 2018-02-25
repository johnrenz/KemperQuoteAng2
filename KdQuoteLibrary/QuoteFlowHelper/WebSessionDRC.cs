using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using AutoQuote;

namespace KdQuoteLibrary.QuoteFlowHelper
{
    [Serializable]
    public class WebSessionDRC : WebSession
    {
        public Autoquote Quote;

        public WebSessionDRC()
        {
        }
    }
}
