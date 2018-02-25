using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoQuoteLibrary.AutoQuoteHelper;

namespace AutoQuoteLibrary.AbstractServices
{
    public interface INavigationServices
    {
        Page GetNextPage(WebSession session);
        Boolean IsDNQ(WebSession session);
        Boolean IsCustomerService(WebSession session);
        Boolean IsRequired(Page page, WebSession session);
    }
}
