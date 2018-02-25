using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KdQuoteLibrary.QuoteFlowHelper;

namespace KdQuoteLibrary.Interfaces
{
    public interface INavigationServices
    {
        Page GetNextPage(WebSession session);
        Boolean IsDNQ(WebSession session);
        Boolean IsCustomerService(WebSession session);
        Boolean IsRequired(Page page, WebSession session);
    }
}
