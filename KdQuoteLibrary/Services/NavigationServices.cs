using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KdQuoteLibrary.QuoteFlowHelper;
using KdQuoteLibrary.Interfaces;

namespace KdQuoteLibrary.Services
{
    public class NavigationServices : INavigationServices
    {
        //private static readonly NavigationServices _navigationServices = new NavigationServices();
        private List<Page> _flow = new List<Page>();

        public NavigationServices()
        {
            _flow.Add(new Page { Controller = "Quote", Action = "Index" });
            _flow.Add(new Page { Controller = "Quote", Action = "Vehicles" });
            _flow.Add(new Page { Controller = "Quote", Action = "VehiclesUse" });
            _flow.Add(new Page { Controller = "Quote", Action = "Drivers" });
            _flow.Add(new Page { Controller = "Quote", Action = "AccidentsViolations" });
            _flow.Add(new Page { Controller = "Quote", Action = "Groups" });
            _flow.Add(new Page { Controller = "Quote", Action = "Household" });
            _flow.Add(new Page { Controller = "Quote", Action = "PriorInsurance" });
            _flow.Add(new Page { Controller = "Quote", Action = "PleaseWaitDiscounts" });
            _flow.Add(new Page { Controller = "Quote", Action = "Discounts" });
            _flow.Add(new Page { Controller = "Quote", Action = "Coverages" });
        }

        //public static NavigationServices Instance
        //{
        //    get
        //    {
        //        return _navigationServices;
        //    }
        //}

        public Page GetNextPage(WebSession session)
        {
            Page current = _flow.Find(x => x.Action.Equals(session.AddInfo.CurrentPage, StringComparison.CurrentCultureIgnoreCase));
            return getNextPage(current, session);
        }

        private Page getNextPage(Page current, WebSession session)
        {
            Page next = null;

            if (IsDNQ(session))
            {
                next = new Page { Controller = "Quote", Action = "DNQ" };
            }
            else if (IsCustomerService(session))
            {
                next = new Page { Controller = "Quote", Action = "CustomerService" };
            }
            while (next == null)
            {
                if (_flow.IndexOf(current) < _flow.Count - 1)
                    current = _flow[_flow.IndexOf(current) + 1];

                if (IsRequired(current, session))
                    next = current;
            }

            return next;
        }

        public Boolean IsDNQ(WebSession session)
        {
            if (session.AddInfo == null)
                return false;
            if (session.AddInfo.DNQ == null)
                return false;
            if (string.IsNullOrWhiteSpace(session.AddInfo.DNQ.Knockout))
                return false;
    
            if (session.AddInfo.DNQ.Knockout.ToLower() == "yes")
                return true;
            else
                return false;
        }

        public Boolean IsCustomerService(WebSession session)
        {
            if (session.AddInfo == null)
                return false;
            if (session.AddInfo.DNQ == null)
                return false;
            if (string.IsNullOrWhiteSpace(session.AddInfo.DNQ.Filter))
                return false;
            else
                return true;
        }

        public Boolean IsRequired(Page page, WebSession session)
        {
            switch (page.Action)
            {
                case "AccidentsViolations":
                    if (session.HasAccidentsViolations)
                        return true;
                    else
                        return false;
                case "Groups":
                    if (((WebSessionDRC)session).Quote.getCustomer().getAddressStateCode() == "CA")
                        return true;
                    else
                        return false;
                case "Discounts":
                    if (session.IsVibeState)
                        return true;
                    else
                        return false;
                case "PriorInsurance":
                    if (((WebSessionDRC)session).Quote.getCustomer().getAddressStateCode() == "CA")
                        return false;
                    else
                        return true;
            }
            return true;
        }

    }
}
