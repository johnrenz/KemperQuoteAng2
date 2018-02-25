using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AutoQuoteLibrary.BL
{
    interface ISessionServices
    {
        /// <summary>
        /// Method used to load questions and initial variables or an existing quote.
        /// </summary>
        /// <param name="request">Request XML containing either a zip code, quote number, or guid</param>
        /// <returns>Response XML containing questions and drcxml</returns>
        XElement Load(XElement request);

        /// <summary>
        /// Method used to save drcxml.
        /// </summary>
        /// <param name="drcXML">Request XML containing the DRC XML</param>
        /// <returns>Response XML containing the guid</returns>
        XElement Save(XElement request);
        /// <summary>
        /// Method used to load HO data drcxml.
        /// </summary>
        /// <param name="drcXML">Request XML containing the DRC XML</param>
        /// <returns>Response XML containing the guid</returns>
        XElement LoadQuoteWithHOPolicy(XElement request);
    }
}
