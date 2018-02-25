using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace QuoteFlowPlugin
{
    interface IZipServices
    {
        /// <summary>
        /// Method used to return the cities from a split zip code.
        /// </summary>
        /// <param name="request">Request XML containing a zip code</param>
        /// <returns>Response xml containing applicable cities</returns>
        XElement SplitZip(XElement request);
    }
}
