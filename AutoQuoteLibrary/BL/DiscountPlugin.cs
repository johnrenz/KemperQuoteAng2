using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace AutoQuoteLibrary.BL
{
    internal class DiscountPlugin
    {
        /// <summary>
        /// Provides for Abstract XML that allows for State Specific Overrides
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public XElement Load(String state)
        {
            XDocument discounts = new XDocument();

            XsltArgumentList arguments = new XsltArgumentList();
            arguments.AddParam("state", String.Empty, state);

            using (XmlWriter writer = discounts.CreateWriter())
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(ConfigurationManager.AppSettings["PluginsPath"] + "\\QuoteFlowData\\Discounts\\Discounts.xslt");
                xslt.Transform(ConfigurationManager.AppSettings["PluginsPath"] + "\\xsl\\Discounts.xml", arguments, writer);
            }

            return discounts.Root;
        }
    }
}
