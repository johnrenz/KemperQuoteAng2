using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using QuoteFlowPlugin.BL;

namespace QuoteFlowPlugin
{
    public class ZipServices : IZipServices
    {
        //tc #7590 03-01-2011 - Split Zip Codes
        public XElement SplitZip(XElement request)
        {
            Quote quote = new Quote();
            XElement response = new XElement("Response");
            LookupServices lookup = new LookupServices();

            quote.AiisQuoteMaster.getCustomer().setZipCode1(Int32.Parse(request.Element("ZipCode").Value));
            lookup.SetSplitZip(quote);
            response.Add(quote.AddInfo.Element("SplitZip"));

            return response;
        }
    }
}
