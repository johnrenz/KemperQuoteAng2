using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AutoQuoteLibrary.AutoQuoteHelper
{
    public static class XELementExtensions
    {
        public static string GetValue(this XElement element, string name)
        {
            if (element.Element(name) == null)
            {
                return "";
            }
            else
            {
                return element.Element(name).Value;
            }
        }
    }
}
