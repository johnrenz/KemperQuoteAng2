using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using KdQuoteLibrary.QuoteFlowHelper;
using SynchronousPluginHelper.BindFlow.Parameters;

namespace KdQuoteLibrary.Interfaces
{
    public interface ISessionServices
    {
        WebSession Load(ref Guid guid, string zip, string ctid, string quoteNo = "");
        Guid Save(WebSession session);
        XElement VerifyAddress(XElement request);
        void OrderCredit(WebSession session);
        bool LoadCoveragesAndDiscounts(WebSession session);
        bool LoadDiscounts(WebSession session);
        bool RatedSave(WebSession session);
        bool Recalculate(WebSession websession, bool reload = false);
        GetSalesInfoResponse GetSalesInfo(WebSession session);
    }
}
