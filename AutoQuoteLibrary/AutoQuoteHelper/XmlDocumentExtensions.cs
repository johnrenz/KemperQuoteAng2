using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace AutoQuoteLibrary.AutoQuoteHelper
{
    public static class XmlDocumentExtensions
    {
        public static int Delete(this XmlDocument doc, string xpath)
        {

            XmlNodeList nodeList = doc.LastChild.SelectNodes(xpath);
            foreach (XmlNode node in nodeList)
            {
                node.ParentNode.RemoveChild(node);
            }
            return nodeList.Count;
        }
    }
}