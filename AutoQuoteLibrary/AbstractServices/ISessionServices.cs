using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoQuoteLibrary.AutoQuoteHelper;
using System.Xml.Linq;

namespace AutoQuoteLibrary.AbstractServices
{
    public interface ISessionServices
    {
        WebSession Load(ref Guid guid, string zip, Boolean initalize);
        Guid Save(WebSession session);
        XElement VerifyAddress(XElement request);
        void OrderCredit(WebSession session);
        XElement UpdateCoveragesAndDiscounts(WebSession session);
        bool LoadCoveragesAndDiscounts(WebSession session);
        bool LoadCoveragesAndDiscounts(WebSession session, string flexXml);
        bool LoadCoveragesAndDiscounts(WebSession session, XElement response);
        bool Recalculate(WebSession session);
        bool LoadDiscounts(WebSession session);
        bool RatedSave(WebSession session);
        bool RecalculateAndReload(WebSession websession);
    }
}
