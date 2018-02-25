using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace KdQuoteLibrary.QuoteFlowHelper
{
    public static class Credit
    {
        //from Flex4 UDQuoteLibrary OrederCreditService.as
        public static bool IsCreditRequired(AutoQuote.Autoquote quote)
        {
            bool result;
            if (quote.getCustomer().getSocialSecurityNo().Length > 0)
                if (quote.getPolicyInfo().getCreditScoreType() < 4)
                    result = true;
                else
                    result = false;
            else
                if (quote.getPolicyInfo().getCreditScoreType() < 2)
                    result = true;
                else
                    result = false;

            if ((quote.getPolicyInfo().getCreditScoreType() == 2) ||
                (quote.getPolicyInfo().getCreditScoreType() == 5))
            {
                if (quote.getPolicyInfo().getCreditScoreEffDate().AddDays(60).CompareTo(DateTime.Now) < 0)
                {
                    result = true;
                }
            }
            if (quote.getCustomer().getAddressStateCode() == "CA")
                result = false;

            return result;
        }
    }
}
