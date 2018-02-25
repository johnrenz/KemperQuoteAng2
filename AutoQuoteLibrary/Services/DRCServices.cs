using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using UDILibrary.Enumerator;
using UDILibrary.UDIExtensions.XMLSerialization;

using SynchronousPluginHelper;
//using SynchronousPluginHelper.BindFlow.Parameters;
using AutoQuoteLibrary.AutoQuoteHelper;

namespace KdQuoteLibrary.Services
{
    public class DRCServices
    {
        private static readonly DRCServices _drcServices = new DRCServices();

        private DRCServices()
        {

        }

        public static DRCServices Instance
        {
            get
            {
                return _drcServices;
            }
        }

        public String GetState(AutoQuote.Autoquote quote)
        {
            String state = String.Empty;

            if (quote.getCustomer().getRiskState() > 0)
            {
                state = ((StatesEnum)quote.getCustomer().getRiskState()).ToString();
            }
            else if (quote.getCustomer().getAddressStateCode() != String.Empty)
            {
                state = quote.getCustomer().getAddressStateCode().ToUpper();
            }
            else
            {
                throw new Exception("KdQuoteLibrary.DRCServices.GetState: Could not load state!");
            }

            return state;
        }
    }
}
