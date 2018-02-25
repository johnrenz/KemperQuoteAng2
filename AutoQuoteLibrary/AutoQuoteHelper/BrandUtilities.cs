using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoQuoteLibrary.AutoQuoteHelper;
using UDILibrary.UDIExtensions.Enumerator;
using System.Web;

namespace AutoQuoteLibrary.AutoQuoteHelper
{
    public static class BrandUtilities
    {
        public static String CompanyName(Int32 brand)
        {
            return ((Brand)brand).GetEnumDescription();
        }

        public static String LegalName(Int32 brand)
        {
            return Brand.KD.GetEnumDescription();
        }

        public static HtmlString NetworkName(Int32 brand)
        {
            return new HtmlString(CompanyName(brand) + " Network");
        }

        public static String DefaultTheme(String host)
        {
            host = host.ToLower();

            if (host.Contains("udpreferred") || host.Contains("select"))
            {
                return "KS";
            }
            else if (host.Contains("unitrin") || host.Contains("direct"))
            {
                return "KD";
            }
            else
            {
                return "KD";
            }
        }

        public static String Theme(Int32 brand)
        {
            switch ((Brand)brand)
            {
                case Brand.KS:
                    return "KS";
                case Brand.KD:
                    return "KD";
                default:
                    throw new Exception("KdQuoteLibrary.BrandUtilities.Theme: Invalid Market Brand - " + brand);
            }
        }
    }
}


