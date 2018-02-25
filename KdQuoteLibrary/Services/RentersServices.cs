using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Collections;
using SynchronousPluginHelper;
using KdQuoteLibrary.QuoteFlowHelper;
using KdQuoteLibrary.ScenariosWS;
using KdQuoteLibrary.AccountMasterWS;
using UDILibrary.Environmental;

namespace KdQuoteLibrary.Services
{
    public class RentersServices : BaseServices
    {
        private static RentersServices _instance;
        public static RentersServices Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RentersServices();
                return _instance;
            }
        }

        public XElement LoadCoverages(WebSessionDRC session)
        {
            Int32 account = LoadAccount(session.Quote.getCustomer().getMarketKeyCode());
            Int32 form = 3;
            String units = "1";
            if (session.Quote.getPolicyInfo().getHoUnitsInBldg() > 0)
                units = session.Quote.getPolicyInfo().getHoUnitsInBldg().ToString();
            DateTime date = session.Quote.getPolicyInfo().getEffDate();
            String zip = session.Quote.getCustomer().getZipCode1().ToString("00000");
            String key = "KdQuoteLibrary_RenterServices_LoadCoverages_" + account + "_" + form + "_" + date + "_" + zip + "_" + units;
            XElement scenarios = (XElement)CacheManager.GetData(key);
            
            if (scenarios == null)
            {
                ScenariosSoapClient ws = new ScenariosSoapClient();
                XElement coverages = XElement.Parse(ws.LookupCoverages(account, form, date, zip));
                scenarios = coverages.Element(coverages.GetDefaultNamespace() + "Scenarios");
                scenarios.Elements().Where(x => x.Attribute("Units").Value != units).Remove();
                CacheManager.Add(key, scenarios, CacheManager.ExpireEveryDayAtSix);
            }

            return scenarios;
        }

        public Int32 LoadAccount(String keycode)
        {
            String key = "KdQuoteLibrary_RenterServices_LoadCoverages_" + keycode;
            Int32? account = (Int32?)CacheManager.GetData(key);

            if (account == null)
            {
                AccountMasterSoapClient ws = new AccountMasterSoapClient();
                string result = ws.RetrieveAccountByMarketKeyCode(keycode);
                if (result == null)
                    account = 61300; //default to corporate click thru
                else
                {
                    account = Int32.Parse(result.Split('|')[0]);
                    CacheManager.Add(key, account, CacheManager.ExpireEveryDayAtSix);
                }
            }

            return (Int32)account;
        }
    }
}
