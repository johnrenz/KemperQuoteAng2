using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace AutoQuoteLibrary.AutoQuoteHelper
{
    public static class XmlNodeExtensions
    {
        public static XmlNode AppendNewChild(this XmlNode node, string name)
        {
            XmlNode child = node.OwnerDocument.CreateElement(name, node.NamespaceURI);
            return node.AppendChild(child);
        }
        public static bool SetItemValue(this XmlNode node, string itemName, string value)
        {
            bool success = false;
            if (node != null && node[itemName] != null)
            {
                node[itemName].InnerText = value;
                success = true;
            }
            return success;
        } 
    }
}