using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using KdQuoteLibrary.Interfaces;
using KdQuoteLibrary.QuoteFlowHelper;

namespace KdQuoteLibrary.Services
{
    public class SessionServicesLocalPlugin : ISessionServices
    {
        public WebSession Load(ref Guid guid, string zip, string ctid, string quoteNo = "")
        {
            try
            {
                WebSession websession = new WebSession();
                XElement request;
                if ((guid != null) && (guid != Guid.Empty)
                    request = new XElement("Request", new XElement("Guid", guid.ToString()), new XElement("ZipCode", zip), new XElement("ClickThruPartnerInfo", new XElement("CTID", ctid), new XElement("Custom", ""), new XElement("MarketKeyCode", ""), new XElement("SalesPhone", ""), new XElement("SalesHours", ""), new XElement("AMFAccountNumber", ""), new XElement("LandingPage", ""), new XElement("HTTPReferrer", ""), new XElement("Affinity", new XElement("IsAffinity", ""), new XElement("IsAgent", ""), new XElement("Logo", ""), new XElement("Description", ""))));
                else if (!string.IsNullOrWhiteSpace(quoteNo))
                    request = new XElement("Request", new XElement("QuoteNo", quoteNo), new XElement("ZipCode", zip), new XElement("ClickThruPartnerInfo", new XElement("CTID", ctid), new XElement("Custom", ""), new XElement("MarketKeyCode", ""), new XElement("SalesPhone", ""), new XElement("SalesHours", ""), new XElement("AMFAccountNumber", ""), new XElement("LandingPage", ""), new XElement("HTTPReferrer", ""), new XElement("Affinity", new XElement("IsAffinity", ""), new XElement("IsAgent", ""), new XElement("Logo", ""), new XElement("Description", ""))));
                else
                    request = new XElement("Request", new XElement("ZipCode", zip), new XElement("ClickThruPartnerInfo", new XElement("CTID", ctid), new XElement("Custom", ""), new XElement("MarketKeyCode", ""), new XElement("SalesPhone", ""), new XElement("SalesHours", ""), new XElement("AMFAccountNumber", ""), new XElement("LandingPage", ""), new XElement("HTTPReferrer", ""), new XElement("Affinity", new XElement("IsAffinity", ""), new XElement("IsAgent", ""), new XElement("Logo", ""), new XElement("Description", ""))));

                XElement response = null;
                response = SessionPlugin.Instance.Load(request);

                guid = new Guid(response.Element("DRCXML").Element("RETURN").Element("AiisQuoteMaster").Element("AddInfo").Element("Guid").Value);
                websession.Quote = new AutoQuote.Autoquote();
                websession.Quote.deserialize(response.Element("DRCXML").ToString(), null);
                websession.AddInfo = new AddInfo();
                websession.AddInfo = response.Element("DRCXML").Element("RETURN").Element("AiisQuoteMaster").Element("AddInfo").ToString().Deserialize<AddInfo>();
                websession.AddInfo.ErrorMessages.Clear();
                websession.AddInfo.ClickThruPartnerInfo.Keywords = "";

                websession.Discounts = new List<Discount>();
                if (response.Element("Discounts") != null)
                {
                    foreach (XElement element in response.Element("Discounts").Elements())
                    {
                        websession.Discounts.Add(new Discount()
                        {
                            ID = element.Attribute("ID").Value,
                            Name = element.Element("Name").Value.FormatDiscountName(),
                            Description = (string.IsNullOrWhiteSpace(element.Element("Description").Value) ? element.Element("Name").Value : element.Element("Description").Value).FormatDiscountDescription(websession),
                            //rules com from flex UDQuoteLibrary.BL.Services.Discounts
                            Applied = DiscountApply(element.Attribute("ID").Value, websession)
                        });
                    }
                }

                websession.Questions = new List<Question>();
                if (response.Element("Questions") != null)
                {
                    foreach (XElement element in response.Element("Questions").Elements())
                    {
                        Question q = new Question();
                        q.ID = element.Attribute("ID").Value;
                        q.Options = new List<Option>();
                        foreach (XElement optionElement in element.Element("Options").Elements("Option"))
                        {
                            //hack: PAQuestions.xml 30/60 should have value 10.
                            if (optionElement.Value == "$30,000/$60,000" && optionElement.Attribute("value").Value == "2")
                                q.Options.Add(new Option()
                                {
                                    Description = optionElement.Value,
                                    Value = "10"
                                });
                            else
                                q.Options.Add(new Option()
                                {
                                    Description = optionElement.Value,
                                    Value = optionElement.Attribute("value").Value
                                });
                        }
                        websession.Questions.Add(q);
                    }
                }

                //load list of incidents from Plugin
                websession.IncidentSelections = new List<Incident>();
                foreach (XElement xe in response.Element("AccidentViolations").Elements())
                {
                    decimal amount = 0;
                    if (xe.Element("Amount") != null)
                        decimal.TryParse(xe.Element("Amount").Value, out amount);
                    websession.IncidentSelections.Add(new Incident()
                    {
                        ID = xe.Attribute("ID").Value,
                        Number = xe.Element("Number").Value,
                        Amount = amount,
                        Description = xe.Element("Description").Value,
                        Type = Incident.ToIncidentType(xe.Attribute("Type").Value),
                        Date = DateTime.MinValue
                    });
                }

                websession.Guid = guid;

                websession.IsVibeState = Utilities.IsVibeState(websession.Quote.getCustomer().getAddressStateCode());
                websession.IsWebModelState = Utilities.IsWebModelState(websession.Quote.getCustomer().getAddressStateCode());

                return websession;
            }
            catch (Exception ex)
            {
                LogUtility.LogError(ex.Message, "AutoQuoteLibrary", "SessionServices", "Load");
                WebSession websession = new WebSession();
                websession.AddErrorMessage("Load", "", "KdQuoteLibrary.SessionServices", ex.Message);
                return websession;
            }

        }




        private bool DiscountApply(string discountID, WebSession websession)
        {
            string state = websession.AddInfo.RiskState;
            DateTime systemDate = DateTime.Now;
            DateTime.TryParse(websession.AddInfo.SystemDate, out systemDate);

            switch (discountID)
            {
                case "MulticarDiscount":
                    if (websession.Quote.getVehicles().count() > 1)
                        return true;
                    else
                        return false;
                case "AirBagDiscount":
                    return false;
                case "PassiveRestraintDiscount":
                    if (state == "NV")
                    {
                        for (int i = 0; i < websession.Quote.getVehicles().count(); i++)
                            if (websession.Quote.getVehicles().item(i).getAirBagTest() > 0)
                                return true;
                    }
                    if (state == "PA")
                    {
                        for (int j = 0; j < websession.Quote.getVehicles().count(); j++)
                        {
                            if (websession.Quote.getVehicles().item(j).getAirBagTest() > 0)
                                return true;
                            if (websession.Quote.getVehicles().item(j).getPassiveRestraint() > 0)
                                return true;
                        }
                    }
                    return false;

                case "ESsignatureDiscount":
                    return false;
                case "WebDiscount":
                    return false;
                case "MingleMateDiscount":
                    if ((websession.AddInfo.Referral != null) ||
                        (websession.Quote.getPolicyInfo().getMingleMateDis() == 1))
                        return true;
                    break;
                case "MultPolicyDiscount":
                    return false;
                case "PaperlessDiscount":
                    if (websession.Quote.getPolicyInfo().getPaperlessDis() == 1)
                        return true;
                    else
                        return false;
                case "ADisabledvcDiscount":
                    if ("IL,PA,MN,IA,NJ".Contains(state))
                        for (int j = 0; j < websession.Quote.getVehicles().count(); j++)
                            if (websession.Quote.getVehicles().item(j).getDisablingDevice() > 0)
                                return true;
                            else
                                if (state == "NJ" && (websession.Quote.getVehicles().item(j).getWindowEtchingTest() > 0))
                                return true;
                    return false;
                case "WelcomeBackDiscount":
                    if (websession.Quote.getPolicyInfo().getWelcomeBackDis() > 0)
                        return true;
                    else
                        return false;
                case "ComeBackAndSaveDiscount":
                    if (websession.Quote.getPolicyInfo().getComeBackDis() > 0)
                        return true;
                    else
                        return false;
                case "MarriedDiscount":
                    if (state != "MI")
                    {
                        if (websession.Quote.getDrivers().count() > 0)
                        {
                            if ((websession.Quote.getDrivers().item(0).getDrivMarriedSingle() == 1) ||
                                (websession.Quote.getDrivers().item(0).getDrivMarriedSingle() == 10))
                            {
                                websession.Quote.getPolicyInfo().setMarriedDis(1);
                                return true;
                            }
                            if ("IN,MN,OR,IL,CO,IA,SC,NJ,WI,VA,MO,NV,OH,AZ,TX,CT,KS,TN,MD,ID,LA,UT".Contains(state))
                            {
                                if (websession.Quote.getDrivers().item(0).getDrivMarriedSingle() == 6)
                                {
                                    websession.Quote.getPolicyInfo().setMarriedDis(1);
                                    return true;
                                }
                            }
                        }
                    }
                    websession.Quote.getPolicyInfo().setMarriedDis(0);
                    return false;
                case "MatureDriverDiscount":

                    if (state == "MI")
                    {
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if (websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(65) < systemDate)
                                return true;
                    }
                    if ("IN,MN,NV,IL,CO,VA,NJ,MD".Contains(state))
                    {
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if ((websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(65) < systemDate) &&
                                (websession.Quote.getDrivers().item(i).getDrivTrainingDis() > 0)) //AddInfo.Drivers.Driver[i].AiDrivCourse
                                return true;
                    }
                    if ("TN,ID".Contains(state))
                    {
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if ((websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(55) < systemDate) &&
                                (websession.Quote.getDrivers().item(i).getDrivTrainingDis() > 0) && //AddInfo.Drivers.Driver[i].AiDrivCourse
                                (!chargeableAccidentsOrViolations(websession.Quote, i)))
                                return true;
                    }
                    if (state == "OR" || state == "SC")
                    {
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if ((websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(25) < systemDate) &&
                            (!chargeableAccidentsOrViolations(websession.Quote, i)))
                                return true;
                    }
                    if (state == "OH" || state == "CT")
                    {
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if (websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(60) < systemDate)
                                return true;
                    }

                    return false;
                case "RetroLoyaltyDiscount":
                    if ((websession.Quote.getPolicyInfo().getRetroLoyaltyLevel() == 2) ||
                        (websession.Quote.getPolicyInfo().getRetroLoyaltyLevel() == 3) ||
                        (websession.Quote.getPolicyInfo().getRetroLoyaltyLevel() == 4))
                        return true;
                    else
                        return false;
                case "SafeSoundDiscount":
                    if ("MI,ID,LA".Contains(state))
                    {
                        if (websession.Quote.getDrivers().count() > 1)
                            if (websession.Quote.getPolicyInfo().getNoOfDaysLapsed() != 2) //no lapse=2
                                for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                                    if (websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(19) < systemDate) //a drive over 19
                                        for (int j = 0; j < websession.Quote.getAccidents().count(); j++)
                                            if (websession.Quote.getAccidents().item(i).getSdipAccComp() > 0)
                                                return true;
                    }
                    if (state == "UT")
                    {
                        if (websession.Quote.getDrivers().count() > 1)
                            if (websession.Quote.getPolicyInfo().getNoOfDaysLapsed() != 8) //innocent no prior=8
                                for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                                    if (websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(19) < systemDate) //a drive over 19
                                        for (int j = 0; j < websession.Quote.getAccidents().count(); j++)
                                            if (websession.Quote.getAccidents().item(i).getSdipAccComp() > 0)
                                                return true;
                    }
                    return false;
                case "PreferPayerDiscount":
                    if (websession.Quote.getCoverages().count() > 0)
                        if (websession.Quote.getCoverages().item(0).getSixMonthPremiums().getSmTPrefPayPrem() > 0)
                            return true;
                    return false;
                case "GroupDiscount":
                    if (websession.Quote.getDrivers().count() > 0)
                        if (websession.Quote.getCoverages().count() > 0)
                            if (websession.Quote.getCoverages().item(0).getSixMonthPremiums().getSmTGroupdiscPrem() > 0)
                                return true;
                    return false;
                case "DdcDiscount":
                    if (state == "KS")
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if (websession.Quote.getDrivers().item(i).getDrivTrainingDis() > 0) //AddInfo.Drivers.Driver[i].AiDrivCourse
                                return true;
                    if (state == "SC")
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if (websession.Quote.getDrivers().item(i).getDrivTrainingDis() > 0) //AddInfo.Drivers.Driver[i].AiDrivCourse
                                if (websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(25) < systemDate)
                                    if (chargeableAccidentsOrViolations(websession.Quote, i))
                                        return true;
                    if (state == "UT")
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if (websession.Quote.getDrivers().item(i).getDrivTrainingDis() > 0) //AddInfo.Drivers.Driver[i].AiDrivCourse
                                if (websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(25) < systemDate)
                                    return true;
                    return false;
                case "VehicleRecoveryDiscount":
                    for (int i = 0; i < websession.Quote.getVehicles().count(); i++)
                        if (websession.Quote.getVehicles().item(i).getVehicleRecoveryTest() > 0)
                            return true;
                    return false;
                case "ContinuousDiscount":
                    if ("IA,SC,NJ,WI,VA".Contains(state))
                        if (websession.Quote.getPolicyInfo().getNoOfDaysLapsed() > 1)
                            return true;
                    return false;
                case "GoodDriverDiscount":
                    for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                        if (websession.Quote.getDrivers().item(i).getGoodDriverDis() > 0)
                            return true;
                    return false;
                case "HomeownershipDiscount":
                    if (websession.Quote.getCustomer().getRentOwnTest() == 1)
                        return true;
                    else
                        return false;
                case "GoodStudentDiscount":
                    if (websession.Quote.getCoverages().count() > 0)
                        if (websession.Quote.getCoverages().item(0).getSixMonthPremiums().getSmTGoodStudentPrem() > 0)
                            return true;
                    return false;
                default:
                    return false;
            }
            return false;
        }

        private bool chargeableAccidentsOrViolations(AutoQuote.Autoquote quote, int driver)
        {
            for (int i = 0; i < quote.getAccidents().count(); i++)
            {
                if (quote.getAccidents().item(i).getAccCompChargedTest() == 1)
                    if (quote.getAccidents().item(i).getDrivNoOfAccComp() == driver)
                        return true;
            }
            for (int i = 0; i < quote.getViolations().count(); i++)
            {
                if (quote.getViolations().item(i).getViolChargedTest() == 1)
                    if (quote.getViolations().item(i).getDrivNoOfViol() == driver)
                        return true;
            }
            return false;
        }

        public Guid Save(WebSession session) //request.Element("DRCXML")
        {
            DateTime systemDate = DateTime.Now;
            DateTime.TryParse(session.AddInfo.SystemDate, out systemDate);

            switch (session.AddInfo.CurrentPage)
            {
                case "Household":
                    if ((session.Quote.getCustomer().getNoOfAdd3Yrs() == 0) &&
                        (session.Quote.getCustomer().getAddressVerificationTest() == 1))
                    {
                        OrderCredit(session);
                    }
                    break;
                case "PriorInsurance":
                    if (session.Quote.getCustomer().getSocialSecurityNo().Length > 4)
                    {
                        OrderCredit(session);
                    }
                    break;
                case "PriorAddress":
                    if (session.Quote.getCustomer().getNoOfAdd3Yrs() > 0)
                    {
                        OrderCredit(session);
                    }
                    break;
            }
            if (session.AddInfo.Drivers != null)
            {
                for (int i = 0; i < session.AddInfo.Drivers.Count; i++)
                {
                    if (session.AddInfo.RiskState == "GA")
                    {
                        if (session.AddInfo.Drivers[i].AiAge < 25)
                        {
                            if (session.AddInfo.Drivers[i].AiFullTimeStudent == "1")
                                if ("A,B".Contains(session.AddInfo.Drivers[i].AiGPA))
                                    session.Quote.getDrivers().item(i).setGoodStudentDate(DateTime.Now);
                        }
                    }
                    if (session.AddInfo.Drivers[i].AiDrivCourse == "2")
                    {
                        switch (session.AddInfo.RiskState)
                        {
                            case "GA":
                                if (session.AddInfo.Drivers[i].AiAge < 25)
                                {
                                    session.Quote.getDrivers().item(i).setDrivTrainingTest(1);
                                    if (session.AddInfo.Drivers[i].AiFullTimeStudent == "1")
                                        if ("A,B".Contains(session.AddInfo.Drivers[i].AiGPA))
                                            session.Quote.getDrivers().item(i).setGoodStudentDate(DateTime.Now);
                                }
                                else
                                {
                                    session.Quote.getDrivers().item(i).setDdcDiscount(1);
                                    DateTime ddcDate = DateTime.Now.AddYears(-1);
                                    session.Quote.getDrivers().item(i).setDdcDate(ddcDate);
                                }
                                break;
                            case "SC":
                                if (session.AddInfo.Drivers[i].AiAge < 25)
                                    session.Quote.getDrivers().item(i).setDdcDiscount(0);
                                else
                                    session.Quote.getDrivers().item(i).setDdcDiscount(1);
                                break;

                            case "NJ":
                                session.Quote.getDrivers().item(i).setDdcDiscount(1);
                                break;
                            default:
                                session.Quote.getDrivers().item(i).setDdcDiscount(0);
                                break;
                        }
                    }
                    else
                    {
                        session.Quote.getDrivers().item(i).setDdcDiscount(0);
                    }
                }
            }
            XElement request = XElement.Parse("<Request><DRCXML><RETURN><AiisQuoteMaster>" + session.Quote.serialize(null) + session.AddInfo.Serialize() + "</AiisQuoteMaster></RETURN></DRCXML></Request>");

            XElement response = SessionPlugin.Instance.Save(request);
            return new Guid(response.Element("Guid").Value);

        }

        public XElement VerifyAddress(XElement request)
        {
            AddressServicePlugin.AddressService addressService = new AddressServicePlugin.AddressService();

            return addressService.GetCorrectAddressInfoByXmlString(request);
        }
        public void OrderCredit(WebSession session)
        {
            XElement request = new XElement("CreditServiceReq");
            request.Add(new XElement("CreditScoreEffDate", session.Quote.getPolicyInfo().getQuoteEffDate().ToShortDateString()));
            request.Add(new XElement("AdSysID", "1")); //quote
            request.Add(new XElement("UserID", "888"));
            request.Add(new XElement("UserName", "FROM WEB"));
            request.Add(new XElement("RiskState", session.Quote.getCustomer().getAddressStateCode()));
            request.Add(new XElement("UnderwritingCompany", session.Quote.getPolicyInfo().getUnderwritingCoNo().ToString()));
            request.Add(new XElement("QuoteNo", ""));
            request.Add(new XElement("EffectiveDate", session.Quote.getQuoteInfo().getPolicyEffDate().ToShortDateString()));
            request.Add(new XElement("MasterStateCode", session.Quote.getCustomer().getAddressStateCode()));
            request.Add(new XElement("Address",
                new XElement("AddressLine1", session.Quote.getCustomer().getAddressLine1() ?? ""),
                new XElement("AddressLine2", session.Quote.getCustomer().getAddressLine2() ?? ""),
                new XElement("City", session.Quote.getCustomer().getAddressCity() ?? ""),
                new XElement("State", session.Quote.getCustomer().getAddressStateCode() ?? ""),
                new XElement("Zip", session.Quote.getCustomer().getZipCode1().ToString("0000"))
                ));
            if (!string.IsNullOrWhiteSpace(session.AddInfo.AddressLine1))
            {
                request.Add(new XElement("PreviousAddress",
                    new XElement("Address",
                          new XElement("AddressLine1", session.AddInfo.AddressLine1 ?? ""),
                          new XElement("Apartment", session.AddInfo.AddressLine2 ?? ""),
                          new XElement("City", session.AddInfo.AddressCity ?? ""),
                          new XElement("State", session.AddInfo.AddressState ?? ""),
                          new XElement("Zip", session.AddInfo.AddressZip ?? "")
                )));
            }
            request.Add(new XElement("DrivFirst", session.Quote.getDrivers().item(0).getDrivFirst()));
            request.Add(new XElement("DrivMiddle", session.Quote.getDrivers().item(0).getDrivMiddle()));
            request.Add(new XElement("DrivLast", session.Quote.getDrivers().item(0).getDrivLast()));
            request.Add(new XElement("DrivSex", session.Quote.getDrivers().item(0).getDrivSex().ToString()));
            request.Add(new XElement("SSN", session.Quote.getCustomer().getSocialSecurityNo()));
            request.Add(new XElement("DOB", session.Quote.getDrivers().item(0).getBirthDateOfDriv().ToShortDateString()));

            XElement response = null;
            CreditPlugin cr = new CreditPlugin();
            response = cr.OrderCredit(request);

            if (response.Element("CreditInfo") != null)
            {
                if ((response.Element("ReturnValue") != null) &&
                    (response.Element("ReturnValue").Value == "0"))
                {
                    DateTime creditScoreEffDate = DateTime.MinValue;
                    int creditSource = 0;
                    if (response.Element("CreditInfo").Element("CreditScoreEffDate") != null)
                        DateTime.TryParse(response.Element("CreditInfo").Element("CreditScoreEffDate").Value, out creditScoreEffDate);
                    if (response.Element("CreditInfo").Element("CreditSource") != null)
                        int.TryParse(response.Element("CreditInfo").Element("CreditSource").Value, out creditSource);
                    session.Quote.getPolicyInfo().setCreditScoreEffDate(creditScoreEffDate);
                    session.Quote.getPolicyInfo().setCreditSource(creditSource);
                }
                int creditScoreType = 0;
                if (response.Element("CreditInfo").Element("CreditScoreType") != null)
                    int.TryParse(response.Element("CreditInfo").Element("CreditScoreType").Value, out creditScoreType);
                string creditScore = "0";
                if (response.Element("CreditInfo").Element("CreditScore") != null)
                    creditScore = response.Element("CreditInfo").Element("CreditScore").Value;
                else
                    if (response.Element("CreditInfo").Element("DefaultScore") != null)
                    creditScore = response.Element("CreditInfo").Element("DefaultScore").Value;

                session.Quote.getPolicyInfo().setCreditScoreType(creditScoreType);
                session.Quote.getCustomer().setCreditScore(creditScore.PadLeft(4, '0'));

                int creditVendor = 0;
                if (response.Element("CreditInfo").Element("CreditVendor") != null)
                    int.TryParse(response.Element("CreditInfo").Element("CreditVendor").Value, out creditVendor);
                session.Quote.getPolicyInfo().setCreditVendor(creditVendor);
                if (response.Element("CreditInfo").Element("CreditModel") != null)
                    session.Quote.getCustomer().setCreditModel(response.Element("CreditInfo").Element("CreditModel").Value);
                if (response.Element("CreditInfo").Element("ReturnedSSN") != null)
                    session.Quote.getCustomer().setSocialSecurityNo(response.Element("CreditInfo").Element("ReturnedSSN").Value);
                if ((response.Element("CreditInfo").Element("MortgageExists") != null) &&
                    (response.Element("CreditInfo").Element("MortgageExists").Value == "1"))
                    session.Quote.getCustomer().setHomeownerVerifyTest(1);
                if (response.Element("CreditInfo").Element("CreditReportId") != null)
                    session.AddInfo.CredRptID = response.Element("CreditInfo").Element("CreditReportId").Value ?? "0";
            }

        }

        public XElement UpdateCoveragesAndDiscounts(WebSession session)
        {
            XElement response = null;
            XElement request = XElement.Parse("<Request><Guid>" + session.Guid.ToString() + "</Guid></Request>");

            try
            {
                response = XElement.Parse(UD3Plugin.Instance().GetXMLFromWebSessionFlex(session.Guid.ToString()));
                if (response == null)
                    return null;
                if (response.Element("Coverages") != null)
                {
                    if (response.Element("Coverages").Element("PolicyCoverages") != null)
                        foreach (XElement covElement in response.Element("Coverages").Element("PolicyCoverages").Elements())
                        {
                            Coverage cov = session.PolicyCoverages.Find(c => c.CovCode == covElement.GetValue("CovCode"));
                            if (cov != null)
                                if (covElement.Element("Limits") != null)
                                    if (covElement.Element("Limits").Element("SelectedLimitValue") != null)
                                    {
                                        covElement.Element("Limits").Element("SelectedLimitValue").Value = cov.SelectedLimit.Value;
                                    }
                        }
                    if (response.Element("Coverages").Element("VehicleCoverages") != null)
                        foreach (XElement vehElement in response.Element("Coverages").Element("VehicleCoverages").Elements("Vehicle"))
                        {
                            VehicleCoverage vc = session.VehicleCoverages.Find(v => v.VehicleNumber == vehElement.GetValue("VehIndex"));
                            foreach (XElement covElement in vehElement.Elements("Coverage"))
                            {
                                Coverage cov = vc.Coverages.Find(c => c.CovCode == covElement.GetValue("CovCode"));
                                if (cov != null)
                                    if (covElement.Element("Limits") != null)
                                        covElement.Element("Limits").Element("SelectedLimitValue").Value = cov.SelectedLimit.Value;
                            }
                        }

                    if (response.Element("Coverages").Element("EnhancedCoverages") != null)
                        foreach (XElement covElement in response.Element("Coverages").Element("EnhancedCoverages").Elements())
                        {
                            Coverage cov = session.EnhancedCoverages.Find(c => c.CovCode == covElement.GetValue("CovCode"));
                            if (cov != null)
                                if (covElement.Element("Purchased") != null)
                                    covElement.Element("Purchased").Value = cov.Purchased ? "true" : "false";
                        }
                }

                if (response.Element("Discounts").Element("DiscountCoverages") != null)
                    //sb = new StringBuilder();
                    foreach (XElement disElement in response.Element("Discounts").Element("DiscountCoverages").Elements())
                    {
                        string name = disElement.GetValue("Name").FormatDiscountDescription(session);
                        if (name.Contains("Network"))
                        {
                            name = name.Replace("iMingle", "");
                        }

                        Discount dis = session.CoveragePageDiscounts.Find(d => d.Name == name);

                        if (dis != null)
                        {

                            disElement.Element("Purchased").Value = dis.Purchased ? "true" : "false";
                        }
                    }

                if (response.Element("PayPlans") != null)
                {
                    string[] valuesArray = session.SelectedPayPlan.Value.Split('~');
                    if (valuesArray.Length > 7)
                        response.Element("PayPlans").Element("SelectedPayPlan").Value = valuesArray[7];

                }
                if (response.Element("InstantRenters") != null)
                {
                    if (response.Element("InstantRenters").Element("HOIRenterInfo") != null)
                    {
                        if (response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterProvide").Value == "YES")
                        {
                            if (response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterProperty") != null)
                                response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterProperty").Value = session.AddInfo.HOIRenterInfo.HOIRenterProperty.ToString();
                            if (response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterLiability") != null)
                                response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterLiability").Value = session.AddInfo.HOIRenterInfo.HOIRenterLiability.ToString();
                            if (response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterDeductible") != null)
                                response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterDeductible").Value = session.AddInfo.HOIRenterInfo.HOIRenterDeductible.ToString();
                            if (response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterInculded") != null)
                                response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterInculded").Value = session.AddInfo.HOIRenterInfo.HOIRenterInculded.ToString();
                            if (response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterPremium") != null)
                                response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterPremium").Value = session.AddInfo.HOIRenterInfo.HOIRenterPremium.ToString();
                        }
                    }
                }
                LoadCoveragesAndDiscounts(session, response);
                return response;
            }
            catch (Exception ex)
            {
                LogUtility.LogError(ex.Message + ";guid=" + session.Guid, "AutoQuoteLibrary", "SessionServices", "UpdateCoveragesAndDiscounts");
                LogUtility.LogError(ex.Message + ";guid=" + session.Guid, "AutoQuoteLibrary", "SessionServices", "UpdateCoveragesAndDiscounts");
                session.AddErrorMessage("UpdateCoveragesAndDiscounts", session.AddInfo.CurrentPage, "KdQuoteLibrary", ex.Message);
                return null;
            }
        }

        public bool LoadCoveragesAndDiscounts(WebSession session)
        {
            try
            {

                XElement request = XElement.Parse("<Request><Guid>" + session.Guid.ToString() + "</Guid></Request>");

                string flexXml = UD3Plugin.Instance().GetXMLFromWebSessionFlex(session.Guid.ToString());
                return LoadCoveragesAndDiscounts(session, flexXml);
            }
            catch (Exception ex)
            {
                LogUtility.LogError(ex.Message, "AutoQuoteFlow", "SessionServices", "LoadCoveragesAndDiscounts -1");
                return false;
            }
        }
        public bool LoadCoveragesAndDiscounts(WebSession session, string flexXml)
        {
            try
            {
                XElement response = XElement.Parse(flexXml);
                return LoadCoveragesAndDiscounts(session, response);
            }
            catch (Exception ex)
            {
                LogUtility.LogError(ex.Message, "AutoQuoteFlow", "SessionServices", "LoadCoveragesAndDiscounts -2");
                return false;
            }
        }
        public bool LoadCoveragesAndDiscounts(WebSession session, XElement response)
        {

            try
            {

                if (response.Element("Coverages") != null)
                {
                    if (response.Element("Coverages").Element("PolicyCoverages") != null)
                    {
                        session.PolicyCoverages = new List<Coverage>();
                        foreach (XElement covElement in response.Element("Coverages").Element("PolicyCoverages").Elements())
                        {
                            Coverage cov = new Coverage();
                            cov.CovCode = covElement.GetValue("CovCode");
                            cov.CovInputType = covElement.GetValue("CovInputType");
                            cov.IncludeCaptionInLayout = true;
                            cov.IncludePremiumInLayout = true;
                            if (cov.CovCode == "pipProfile")
                            {
                                cov.IncludeCaptionInLayout = false;
                                cov.IncludePremiumInLayout = false;
                            }
                            if (covElement.Element("Limits") == null)
                            {
                                cov.Limits = new List<Limit>(); //empty list
                                cov.SelectedLimit = new Limit()
                                {
                                    Abbrev = "",
                                    Caption = "",
                                    IsNoCov = "",
                                    Value = "",
                                    SortOrder = ""
                                };
                                if (cov.CovInputType == "label")
                                {
                                    cov.SelectedLimit.Caption = covElement.GetValue("LabelDescription");
                                    cov.SelectedLimit.Value = covElement.GetValue("LabelValue");
                                }

                            }
                            else
                            {
                                cov.SelectedValue = covElement.Element("Limits").GetValue("SelectedLimitValue");
                                cov.Limits = new List<Limit>();
                                foreach (XElement limitElement in covElement.Element("Limits").Elements("Limit"))
                                {
                                    Limit lim = new Limit()
                                    {
                                        Abbrev = limitElement.GetValue("Abbrev"),
                                        Caption = limitElement.GetValue("Caption"),
                                        IsNoCov = limitElement.GetValue("IsNoCov"),
                                        Value = limitElement.GetValue("Value"),
                                        SortOrder = limitElement.GetValue("SortOrder")
                                    };
                                    cov.Limits.Add(lim);
                                    if (limitElement.GetValue("Value") == cov.SelectedValue)
                                    {
                                        cov.SelectedLimit = lim;
                                        switch (cov.CovCode)
                                        {
                                            case "PIPFPB":
                                                cov.Caption = limitElement.GetValue("Caption").formatFPB();
                                                break;
                                            case "PIPCombined":
                                                cov.Caption = limitElement.GetValue("Caption").formatPIPCombined();
                                                break;
                                            default:
                                                cov.Caption = limitElement.GetValue("Caption");
                                                break;
                                        }
                                    }
                                }
                            }
                            if (cov.SelectedLimit == null)
                                if (cov.Limits[0] != null)
                                    cov.SelectedLimit = cov.Limits[0];
                            cov.Name = (covElement.GetValue("Desc"));
                            cov.HelpText = (covElement.GetValue("FAQText"));
                            cov.SuppressRendering = bool.Parse(covElement.GetValue("SuppressRendering").ToLower() == "true" ? "true" : "false");
                            cov.WebQuestionID = covElement.GetValue("WebQuestionID");
                            if (cov.WebQuestionID == "POLCOV_UMPDDED")
                                cov.Premium = "";
                            else
                            {
                                if (response.Element("Coverages").Element("PolicyPremiums") != null)
                                {
                                    var prems = (from c in response.Element("Coverages").Element("PolicyPremiums").Elements("Premium")
                                                 where c.GetValue("CovCode") == cov.CovCode
                                                 select c);
                                    if ((prems == null) ||
                                        (prems.Count() == 0))
                                        cov.Premium = "";
                                    else
                                    {
                                        var prem = prems.First();
                                        if ((string.IsNullOrWhiteSpace(prem.GetValue("Amount"))) ||
                                            (prem.GetValue("Amount") == "0") ||
                                            (!prem.GetValue("Amount").IsNumeric()))
                                            cov.Premium = "";
                                        else
                                            cov.Premium = string.Format("{0:c}", decimal.Parse(prem.GetValue("Amount")));
                                    }
                                }
                            }
                            session.PolicyCoverages.Add(cov);
                        }
                    }
                    if (response.Element("Coverages").Element("VehicleCoverages") != null)
                    {
                        session.VehicleCoverages = new List<VehicleCoverage>();
                        foreach (XElement vehElement in response.Element("Coverages").Element("VehicleCoverages").Elements("Vehicle"))
                        {
                            VehicleCoverage vc = new VehicleCoverage();
                            vc.Coverages = new List<Coverage>();
                            vc.Year = vehElement.GetValue("Year");
                            vc.Make = vehElement.GetValue("Make");
                            vc.Model = vehElement.GetValue("Model");
                            vc.VehicleNumber = vehElement.GetValue("VehIndex");
                            foreach (XElement covElement in vehElement.Elements("Coverage"))
                            {
                                Coverage cov = new Coverage();
                                cov.CovCode = covElement.GetValue("CovCode");
                                cov.Name = covElement.GetValue("Desc");
                                cov.HelpText = covElement.GetValue("FAQText");
                                cov.SuppressRendering = bool.Parse(covElement.GetValue("SuppressRendering").ToLower() == "true" ? "true" : "false");
                                cov.CovInputType = covElement.GetValue("CovInputType");
                                cov.WebQuestionID = covElement.GetValue("WebQuestionID");
                                if (covElement.Element("Limits") == null)
                                {
                                    cov.Limits = new List<Limit>(); //empty list
                                    cov.SelectedLimit = new Limit()
                                    {
                                        Abbrev = "",
                                        Caption = "",
                                        IsNoCov = "",
                                        Value = "",
                                        SortOrder = ""
                                    };
                                }
                                else
                                {
                                    cov.SelectedValue = covElement.Element("Limits").GetValue("SelectedLimitValue");
                                    cov.Limits = new List<Limit>();
                                    foreach (XElement limitElement in covElement.Element("Limits").Elements("Limit"))
                                    {
                                        Limit lim = new Limit()
                                        {
                                            Abbrev = limitElement.GetValue("Abbrev"),
                                            Caption = limitElement.GetValue("Caption"),
                                            IsNoCov = limitElement.GetValue("IsNoCov"),
                                            Value = limitElement.GetValue("Value"),
                                            SortOrder = limitElement.GetValue("SortOrder")
                                        };
                                        cov.Limits.Add(lim);
                                        if (cov.SelectedValue == limitElement.GetValue("Value"))
                                        {
                                            cov.SelectedLimit = lim;
                                            cov.Caption = limitElement.GetValue("Caption");
                                        }
                                    }
                                }
                                if (response.Element("Coverages").Element("VehiclePremiums") != null)
                                {
                                    foreach (XElement vehPremElement in response.Element("Coverages").Element("VehiclePremiums").Elements("Vehicle"))
                                    {
                                        if (vehPremElement.GetValue("VehIndex") == vehElement.GetValue("VehIndex"))
                                        {
                                            var prems = (from c in vehPremElement.Elements("Premium")
                                                         where c.GetValue("CovCode") == cov.CovCode
                                                         select c);
                                            if ((prems == null) ||
                                                (prems.Count() == 0))
                                                cov.Premium = "";
                                            else
                                            {
                                                var prem = prems.First();
                                                if ((string.IsNullOrWhiteSpace(prem.GetValue("Amount"))) ||
                                                    (prem.GetValue("Amount") == "0") ||
                                                    (!prem.GetValue("Amount").IsNumeric()))
                                                    cov.Premium = "";
                                                else
                                                    cov.Premium = string.Format("{0:c}", decimal.Parse(prem.GetValue("Amount")));
                                            }
                                            break;
                                        }
                                    }
                                }
                                vc.Coverages.Add(cov);
                            }
                            session.VehicleCoverages.Add(vc);
                        }
                    }
                    if (response.Element("Coverages").Element("EnhancedCoverages") != null)
                    {
                        session.EnhancedCoverages = new List<EnhancedCoverage>();
                        foreach (XElement covElement in response.Element("Coverages").Element("EnhancedCoverages").Elements("EnhancedCoverage"))
                        {
                            EnhancedCoverage cov = new EnhancedCoverage();
                            cov.CovCode = covElement.GetValue("CovCode");
                            cov.Name = covElement.GetValue("Name");
                            cov.WebQuestionID = covElement.GetValue("WebQuestionID");
                            cov.Desc = covElement.GetValue("Desc");
                            cov.CovInputType = covElement.GetValue("CovInputType");
                            if (response.Element("Coverages").Element("EnhancedPremiums") != null)
                            {
                                var prems = (from c in response.Element("Coverages").Element("EnhancedPremiums").Elements("Premium")
                                             where c.GetValue("Name") == cov.Name
                                             select c);
                                if ((prems == null) ||
                                    (prems.Count() == 0))
                                    cov.Premium = "";
                                else
                                {
                                    var prem = prems.First();
                                    if ((string.IsNullOrWhiteSpace(prem.GetValue("Amount"))) ||
                                        (!prem.GetValue("Amount").IsNumeric()) ||
                                        (prem.GetValue("Amount") == "0"))
                                    {
                                        cov.Premium = "";
                                        cov.PremiumNumeric = 0;
                                    }
                                    else
                                    {
                                        cov.Premium = string.Format("{0:c}", decimal.Parse(prem.GetValue("Amount"))); //per month
                                        cov.PremiumNumeric = decimal.Parse(prem.GetValue("Amount")); //per month
                                    }
                                }
                            }
                            if (cov.Name == "Protect Your Wallet")
                            {
                                cov.Bundle = new List<BundledCoverage> {
                                        new BundledCoverage() {
                                            header = "Accident Forgiveness",
                                            description = "We won’t raise your rates just because you have an accident – even if it’s your fault." },
                                        new BundledCoverage() {
                                            header = "Renewal Assurance",
                                            description = "Don’t worry about getting dropped by your insurance company. As long as you continue to drive safely (for example, no DUIs) and meet a few other easy requirements, we won’t “break up” with you. That’s peace of mind you deserve." },
                                        new BundledCoverage() {
                                            header = "Disappearing Deductible",
                                            description = "We’ll take $100 off your Collision deductible right now.  Then, we’ll keep lowering it by $100 each year you’re accident free. (Note: Pennsylvania law does not allow this deductible to be less than $100.)" }
                                    };
                            }
                            if (cov.Name == "Protect Your Sanity")
                            {
                                cov.Desc = "When the unexpected happens, get back on the road with Roadside Assistance, <br>Rental Car Reimbursement and Trip Interruption.";
                                cov.Bundle = new List<BundledCoverage>
                                    {
                                        new BundledCoverage() {
                                            header = "Roadside Assistance",
                                            description = "Getting stuck on the side of the road isn’t any fun. We’ll send someone to change your tire, bring you gas, jumpstart your vehicle, help if you’re locked out, or give you a tow."
                                        },
                                        new BundledCoverage() {
                                            header = "Rental Car Reimbursement",
                                            description = "If your car is out of commission due to a covered loss, get back on the road with a rental car. We’ll reimburse you for up to $30 per day with a $600 maximum benefit."
                                        },
                                        new BundledCoverage() {
                                            header = "Trip Interruption",
                                            description = "When you’re on a road trip, an accident or mechanical breakdown can really slow you down. If you’re stuck more than 100 miles from home, we’ll provide up to $600 to use for things like transportation and lodging."
                                        }
                                    };
                            }
                            if (cov.Name == "Protect Your Family")
                            {
                                cov.Desc = "Keep your loved ones better protected with Enhanced Car Seat Replacement, <br>Pet Protection and Dependent Protection.";
                                cov.Bundle = new List<BundledCoverage>
                                    {
                                        new BundledCoverage() {
                                            header = "Enhanced Car Seat Replacement",
                                            description = "We know that a car seat’s safety can be compromised after an accident. That’s why we’ll replace your child car seats after a crash regardless of damage and without requiring you to pay a deductible."
                                        },
                                        new BundledCoverage() {
                                            header = "Pet Protection",
                                            description = "Your pets mean a lot to you. That’s why we’ll cover up to $2,000 for injury or death to your furry family members due to an accident. Dogs and cats apply."
                                        },
                                        new BundledCoverage() {
                                            header = "Dependent Protection",
                                            description = "Taking care of your children is your most important duty. That’s why we’ll provide up to $1,000 in daycare costs if an accident leaves you unable to care for your child. We’ll also provide $10,000 to your dependants if you die due to an accident."
                                        }
                                    };
                            }
                            session.EnhancedCoverages.Add(cov);
                        }
                    }
                    session.CoveragePageDiscounts = new List<Discount>();
                    if (response.Element("Discounts").Element("DiscountCoverages") != null)
                    {
                        session.TotalDiscountSavings = 0;
                        session.TotalDiscountSavingsWithoutPreferredPayer = 0;
                        foreach (XElement e2 in response.Element("Discounts").Element("DiscountCoverages").Elements())
                        {
                            Discount newDisc = new Discount();
                            newDisc.Name = e2.GetValue("Name").FormatDiscountDescription(session);
                            newDisc.ShortDescription = e2.GetValue("Description").FormatDiscountDescription(session);
                            newDisc.ExpandedDesc = e2.GetValue("ExpandedDesc").FormatDiscountDescription(session);
                            newDisc.Purchased = bool.Parse((e2.GetValue("Purchased").ToLower() == "true") || (e2.GetValue("Purchased") == "1") ? "true" : "false");
                            newDisc.CanBeDeleted = bool.Parse(e2.GetValue("CanBeDeleted").ToLower() == "true" ? "true" : "false");
                            if (newDisc.Name.Contains("Preferred"))
                                newDisc.ID = "PreferredPayerDiscount";
                            if (newDisc.Name.Contains("Multi-Car"))
                                newDisc.ID = "MultiCarDiscount";
                            if (newDisc.Name.Contains("Homeownership"))
                                newDisc.ID = "HomeownershipDiscount";
                            if (newDisc.Name.Contains("Network"))
                            {
                                newDisc.ID = "NetworkDiscount";
                                newDisc.Name = newDisc.Name.Replace("iMingle", "");
                            }
                            if (newDisc.Name.Contains("Anti-Theft"))
                                newDisc.ID = "AntiTheftDiscount";
                            if (newDisc.Name.Contains("Free-A-Tree"))
                                newDisc.ID = "FreeATree";
                            if (newDisc.Name.Contains("Multi-Policy"))
                                newDisc.ID = "MultiPolicy";
                            if (newDisc.Name.Contains("Happily Married"))
                                newDisc.ID = "HappilyMarried";
                            if (newDisc.Name.Contains("Safe"))
                                newDisc.ID = "SafeAndSound";
                            if (newDisc.Name.Contains("Passive Restraint"))
                                newDisc.ID = "PassiveRestraint";
                            if (newDisc.Name.Contains("Focused Driver"))
                                newDisc.ID = "FocusedDriver";
                            if (newDisc.Name.Contains("Esignature"))
                                newDisc.ID = "EsignatureDiscount";
                            foreach (XElement e in response.Element("Discounts").Element("DiscountPremiums").Elements())
                            {
                                if (e.GetValue("Name") == e2.GetValue("Name"))
                                {
                                    if ((string.IsNullOrWhiteSpace(e.GetValue("Amount"))) ||
                                        (!e.GetValue("Amount").IsNumeric()) ||
                                        (e.GetValue("Amount") == "0"))
                                    {
                                        newDisc.Amount = "";
                                        newDisc.AmountNumeric = 0;
                                    }
                                    else
                                    {
                                        newDisc.AmountNumeric = decimal.Parse(e.GetValue("Amount"));
                                        newDisc.Amount = string.Format("{0:c}", decimal.Parse(e.GetValue("Amount")));
                                    }
                                    session.TotalDiscountSavings += newDisc.AmountNumeric;
                                    if (newDisc.ID != "PreferredPayerDiscount")
                                        if (newDisc.Purchased)
                                            session.TotalDiscountSavingsWithoutPreferredPayer += newDisc.AmountNumeric;
                                }
                            }
                            session.CoveragePageDiscounts.Add(newDisc);
                        }
                    }
                    if (response.Element("Coverages").Element("CalculatedPremiums") != null)
                    {
                        decimal prem = 0;
                        decimal.TryParse(response.Element("Coverages").Element("CalculatedPremiums").GetValue("Amount"), out prem);
                        session.TotalPremium = prem;
                    }
                    if (response.Element("PayPlans") != null)
                    {
                        Hashtable payPlanIndexTable = new Hashtable(10);
                        for (int i = 0; i < session.Quote.getDownPayOptions().count(); i++)
                        {
                            payPlanIndexTable[session.Quote.getDownPayOptions().item(i).getDpoBplan()] = i;
                        }
                        session.PayPlans = new List<PayPlan>();
                        foreach (XElement e in response.Element("PayPlans").Elements("PayPlan"))
                        {
                            PayPlan pp = new PayPlan();
                            pp.Name = e.GetValue("Name");
                            pp.ID = e.Attribute("id").Value;
                            decimal discount = 0;
                            decimal.TryParse(e.GetValue("PreferredPayerDiscount"), out discount);
                            pp.Discount = discount;
                            pp.Yearly = session.TotalPremium - session.TotalDiscountSavingsWithoutPreferredPayer - pp.Discount;
                            pp.Downpayment = 0;
                            pp.Installment = 0;
                            string termText = (session.Quote.getPolicyInfo().getTermFactor() == .5) ? " total" : "/year";
                            switch (pp.Name)
                            {
                                case "Quarterly":
                                    pp.Installments = 3;
                                    pp.DownDivisor = 4;
                                    pp.InstallmentType = "Quarterly";
                                    pp.Downpayment = pp.Yearly / 4;
                                    pp.Installment = pp.Downpayment;
                                    pp.Description = string.Format("{0:c} Quarterly ({1:c}" + termText + ")", pp.Installment, pp.Yearly);
                                    break;
                                case "Semi-Annually":
                                    pp.Installments = 1;
                                    pp.DownDivisor = 2;
                                    pp.InstallmentType = "Semi-Annually";
                                    pp.Downpayment = pp.Yearly / 2;
                                    pp.Installment = pp.Downpayment;
                                    pp.Description = string.Format("{0:c} Semi-Annually ({1:c}" + termText + ")", pp.Installment, pp.Yearly);
                                    break;
                                case "Annually (Pay-In-Full)":
                                    pp.Installments = 0;
                                    pp.DownDivisor = 1;
                                    pp.InstallmentType = "Annually";
                                    pp.Downpayment = pp.Yearly;
                                    pp.Installment = 0;
                                    pp.Description = string.Format("{0:c} Annually (Pay-In-Full)", pp.Yearly);
                                    session.AnnualPayPlanDiscountSavings = pp.Discount;
                                    break;
                                case "1/11 Down":
                                    pp.Installments = 10;
                                    pp.DownDivisor = 11;
                                    pp.InstallmentType = "Monthly";
                                    pp.Downpayment = pp.Yearly / 11;
                                    pp.Installment = (pp.Yearly - pp.Downpayment) / 10;
                                    pp.Description = string.Format("{0:c} Monthly ({1:c}" + termText + ")", pp.Installment, pp.Yearly);
                                    break;
                                case "1/8 Down":
                                    pp.Installments = 10;
                                    pp.DownDivisor = 8;
                                    pp.InstallmentType = "Monthly";
                                    pp.Downpayment = pp.Yearly / 8;
                                    pp.Installment = (pp.Yearly - pp.Downpayment) / 10;
                                    pp.Description = string.Format("{0:c} Monthly ({1:c}" + termText + ")", pp.Installment, pp.Yearly);
                                    break;
                                case "1/6 Down":
                                    pp.Installments = 10;
                                    pp.DownDivisor = 6;
                                    pp.InstallmentType = "Monthly";
                                    pp.Downpayment = pp.Yearly / 6;
                                    pp.Installment = (pp.Yearly - pp.Downpayment) / 10;
                                    pp.Description = string.Format("{0:c} Monthly ({1:c}" + termText + ")", pp.Installment, pp.Yearly);
                                    break;

                                case "2 Payments(Non Vibe)":
                                    pp.Yearly = session.TotalPremium;
                                    pp.Installments = 1;
                                    pp.DownDivisor = 2;
                                    pp.InstallmentType = "Payment";
                                    if (payPlanIndexTable["2 PAYMENTS"] != null)
                                    {
                                        pp.Downpayment = (decimal)session.Quote.getDownPayOptions().item((int)payPlanIndexTable["2 PAYMENTS"]).getDpoRdnamt();
                                        pp.Installment = (decimal)session.Quote.getDownPayOptions().item((int)payPlanIndexTable["2 PAYMENTS"]).getDpoInstamt();
                                    }
                                    pp.Description = string.Format("{0:c} Semi-Annually ({1:c}" + termText + ")", pp.Installment, pp.Yearly);
                                    break;
                                case "3 Payments(Non Vibe)":
                                    pp.Yearly = session.TotalPremium;
                                    pp.Installments = 2;
                                    pp.DownDivisor = 3;
                                    pp.InstallmentType = "Payments";
                                    if (payPlanIndexTable["3 PAYMENTS"] != null)
                                    {
                                        pp.Downpayment = (decimal)session.Quote.getDownPayOptions().item((int)payPlanIndexTable["3 PAYMENTS"]).getDpoRdnamt();
                                        pp.Installment = (decimal)session.Quote.getDownPayOptions().item((int)payPlanIndexTable["3 PAYMENTS"]).getDpoInstamt();
                                    }
                                    pp.Description = string.Format("{0:c} Installments ({1:c}" + termText + ")", pp.Installment, pp.Yearly);
                                    break;
                                case "4 Payments(Non Vibe)":
                                    pp.Yearly = session.TotalPremium;
                                    pp.Installments = 3;
                                    pp.DownDivisor = 4;
                                    pp.InstallmentType = "Payments";
                                    if (payPlanIndexTable["4 PAYMENTS"] != null)
                                    {
                                        pp.Downpayment = (decimal)session.Quote.getDownPayOptions().item((int)payPlanIndexTable["4 PAYMENTS"]).getDpoRdnamt();
                                        pp.Installment = (decimal)session.Quote.getDownPayOptions().item((int)payPlanIndexTable["4 PAYMENTS"]).getDpoInstamt();
                                    }
                                    pp.Description = string.Format("{0:c} Quarterly ({1:c}" + termText + ")", pp.Installment, pp.Yearly);
                                    break;
                                case "5 Payments(Non Vibe)":
                                    pp.Yearly = session.TotalPremium;
                                    pp.Installments = 4;
                                    pp.DownDivisor = 4;
                                    pp.InstallmentType = "Payments";
                                    if (payPlanIndexTable["MONTHLY"] != null)
                                    {
                                        AutoQuote.DownPayOption dpo = session.Quote.getDownPayOptions().item((int)payPlanIndexTable["MONTHLY"]);
                                        pp.Downpayment = (decimal)dpo.getDpoRdnamt();
                                        pp.Installment = (decimal)(session.Quote.getCoverages().item(0).getSixMonthPremiums().getSmTotalPolPrem() +
                                            session.Quote.getPolicyInfo().getPolicyFee() +
                                            session.Quote.getPolicyInfo().getOptionFee1() -
                                            dpo.getDpoRdnamt() - dpo.getDpoDnfee()) / (pp.Installments);
                                    }
                                    pp.Description = string.Format("{0:c} Installments ({1:c}" + termText + ")", pp.Installment, pp.Yearly);
                                    break;
                                case "Pay in Full(Non Vibe)":
                                    pp.Installments = 0;
                                    pp.DownDivisor = 1;
                                    pp.InstallmentType = "Annually";
                                    if (payPlanIndexTable["PAY IN FULL"] != null)
                                    {
                                        pp.Downpayment = (decimal)session.Quote.getDownPayOptions().item((int)payPlanIndexTable["PAY IN FULL"]).getDpoRdnamt();
                                        pp.Installment = (decimal)session.Quote.getDownPayOptions().item((int)payPlanIndexTable["PAY IN FULL"]).getDpoInstamt();
                                    }
                                    pp.Yearly = pp.Downpayment;
                                    pp.Description = string.Format("{0:c} Annually (Pay-In-Full)", pp.Yearly);
                                    session.AnnualPayPlanDiscountSavings = pp.Discount;
                                    break;

                            }
                            pp.Value = string.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}", pp.Installments, pp.Installment, pp.Downpayment, pp.Discount, pp.InstallmentType, pp.Yearly, pp.DownDivisor, pp.ID);
                            session.PayPlans.Add(pp);
                            if (response.Element("PayPlans").GetValue("SelectedPayPlan") == "")
                            {
                                if (e.Attribute("id").Value == "4")
                                {
                                    session.SelectedPayPlan = pp;
                                }
                            }
                            if (response.Element("PayPlans").GetValue("SelectedPayPlan") == "0")
                            {
                                if (e.Attribute("id").Value == "1")
                                {
                                    session.SelectedPayPlan = pp;
                                }
                            }
                            if (e.Attribute("id").Value == response.Element("PayPlans").GetValue("SelectedPayPlan"))
                            {
                                session.SelectedPayPlan = pp;
                            }
                        }
                    }
                    session.InstallmentFees = new List<InstallmentFee>();
                    session.HasInstallmentFees = false; //default
                    if ((response.Element("GeneralInfo") != null) &&
                        (response.Element("GeneralInfo").Element("Quote") != null) &&
                        (response.Element("GeneralInfo").Element("Quote").Element("InstallmentFees") != null))
                    {
                        session.InstallmentFees = new List<InstallmentFee> {
                                new InstallmentFee() {
                                    Amount = response.Element("GeneralInfo").Element("Quote").Element("InstallmentFees").GetValue("InstallFeeDirect"),
                                    Name = "InstallFeeDirect"
                                },
                                new InstallmentFee() {
                                    Amount = response.Element("GeneralInfo").Element("Quote").Element("InstallmentFees").GetValue("InstallFeeCC"),
                                    Name = "InstallFeeCC"
                                },
                                new InstallmentFee() {
                                    Amount = response.Element("GeneralInfo").Element("Quote").Element("InstallmentFees").GetValue("InstallFeeEFT"),
                                    Name = "InstallFeeEFT"
                                }
                            };
                        foreach (InstallmentFee fee in session.InstallmentFees)
                        {
                            if ((string.IsNullOrWhiteSpace(fee.Amount)) ||
                                        (!fee.Amount.IsNumeric()) ||
                                        (fee.Amount == "0"))
                            {
                                fee.AmountNumeric = 0;
                            }
                            else
                            {
                                fee.AmountNumeric = decimal.Parse(fee.Amount);
                                session.HasInstallmentFees = true;
                            }
                        }
                    }
                    session.AddInfo.HOIRenterInfo = new HOIRenterInfo();
                    if ((response.Element("InstantRenters") != null) &&
                        (response.Element("InstantRenters").Element("HOIRenterInfo") != null) &&
                        (response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterProvide") != null))
                    {
                        session.AddInfo.HOIRenterInfo.HOIRenterProvide = response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterProvide").Value.ToUpper() == "YES" ? HOIRenterInfo.EnumRenterProvide.Yes : HOIRenterInfo.EnumRenterProvide.No;
                        if (session.AddInfo.HOIRenterInfo.HOIRenterProvide == HOIRenterInfo.EnumRenterProvide.Yes)
                        {
                            int val = 0;
                            int.TryParse(response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterInculded").Value, out val);
                            session.AddInfo.HOIRenterInfo.HOIRenterInculded = val;
                            val = 0;
                            int.TryParse(response.Element("InstantRenters").Element("HOIRenterInfo").Element("HONoOfUnit").Value, out val);
                            session.AddInfo.HOIRenterInfo.HONoOfUnit = val;
                            val = 0;
                            int.TryParse(response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterLiability").Value, out val);
                            session.AddInfo.HOIRenterInfo.HOIRenterLiability = val;
                            val = 0;
                            int.TryParse(response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterProperty").Value, out val);
                            session.AddInfo.HOIRenterInfo.HOIRenterProperty = val;
                            val = 0;
                            int.TryParse(response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterDeductible").Value, out val);
                            session.AddInfo.HOIRenterInfo.HOIRenterDeductible = val;
                            decimal dVal = 0;
                            decimal.TryParse(response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterPremium").Value, out dVal);
                            session.AddInfo.HOIRenterInfo.HOIRenterPremium = dVal;
                            session.RentersPropertySelections = new List<Option>();
                            session.RentersLiabilitySelections = new List<Option>();
                            session.RentersDeductibleSelections = new List<Option>();
                            if ((response.Element("InstantRenters").Element("State") != null) &&
                                (response.Element("InstantRenters").Element("State").Element("Coverages") != null))
                                foreach (XElement e2 in response.Element("InstantRenters").Element("State").Element("Coverages").Elements())
                                {
                                    if (e2.Name == "Property")
                                    {
                                        session.RentersPropertySelections.Add(new Option()
                                        {
                                            Description = e2.Value,
                                            Value = e2.Value,
                                            Selected = e2.Attribute("default").Value == "Y"
                                        });
                                        if (e2.Attribute("default").Value == "Y")
                                            if (session.AddInfo.HOIRenterInfo.HOIRenterProperty == 0)
                                                session.AddInfo.HOIRenterInfo.HOIRenterProperty = int.Parse(e2.Value);
                                    }
                                    if (e2.Name == "Liability")
                                    {
                                        session.RentersLiabilitySelections.Add(new Option()
                                        {
                                            Description = e2.Value,
                                            Value = e2.Value,
                                            Selected = e2.Attribute("default").Value == "Y"
                                        });
                                        if (e2.Attribute("default").Value == "Y")
                                            if (session.AddInfo.HOIRenterInfo.HOIRenterLiability == 0)
                                                session.AddInfo.HOIRenterInfo.HOIRenterLiability = int.Parse(e2.Value);
                                    }
                                    if (e2.Name == "Deductible")
                                    {
                                        session.RentersDeductibleSelections.Add(new Option()
                                        {
                                            Description = e2.Value,
                                            Value = e2.Value,
                                            Selected = e2.Attribute("default").Value == "Y"
                                        });
                                        if (e2.Attribute("default").Value == "Y")
                                            if (session.AddInfo.HOIRenterInfo.HOIRenterDeductible == 0)
                                                session.AddInfo.HOIRenterInfo.HOIRenterDeductible = int.Parse(e2.Value);
                                    }
                                }
                            session.RentersRates = new List<RentersRateScenario>();
                            if ((response.Element("InstantRenters").Element("State") != null) &&
                                (response.Element("InstantRenters").Element("State").Element("Rates") != null))
                            {
                                foreach (XElement e2 in response.Element("InstantRenters").Element("State").Element("Rates").Elements())
                                {
                                    session.RentersRates.Add(new RentersRateScenario()
                                    {
                                        property = e2.Attribute("property").Value,
                                        liability = e2.Attribute("liability").Value,
                                        deductible = e2.Attribute("deductible").Value,
                                        premium = e2.Attribute("premium").Value
                                    });
                                    if ((e2.Attribute("property").Value == session.AddInfo.HOIRenterInfo.HOIRenterProperty.ToString()) &&
                                        (e2.Attribute("liability").Value == session.AddInfo.HOIRenterInfo.HOIRenterLiability.ToString()) &&
                                        (e2.Attribute("deductible").Value == session.AddInfo.HOIRenterInfo.HOIRenterDeductible.ToString()))
                                        session.AddInfo.HOIRenterInfo.HOIRenterPremium = decimal.Parse(e2.Attribute("premium").Value);
                                }
                                session.xRentersRates = response.Element("InstantRenters").Element("State").Element("Rates");
                            }
                        }
                    }
                };
                return true;
            }
            catch (Exception ex)
            {
                session.AddErrorMessage("LoadCoveragesAndDiscounts", session.AddInfo.CurrentPage, "KdQuoteLibrary", ex.Message);
                session.AddErrorMessage("LoadCoveragesAndDiscounts", session.AddInfo.CurrentPage, "KdQuoteLibrary", ex.StackTrace);

                return false;
            }

        }

        public bool Recalculate(WebSession session)
        {

            XElement response = null;

            try
            {
                response = UD3Plugin.Instance().RecalculateQuote(session);

                session.PolicyCoverageErrors = new List<CoverageError>();
                session.VehicleCoverageErrors = new List<CoverageError>();

                if ((response.Element("Coverages") != null) &&
                    (response.Element("Coverages").Element("EditErrors") != null))
                {
                    foreach (XElement errorElement in response.Element("Coverages").Element("EditErrors").Elements("EditError"))
                    {
                        CoverageError err = new CoverageError();
                        err.Message = errorElement.GetValue("EditMessage");
                        if (errorElement.Element("Coverage") != null)
                        {
                            XElement covElement = errorElement.Element("Coverage");
                            err.CovCode = covElement.GetValue("CovCode");
                            err.VehIndex = covElement.GetValue("VehIndex");
                            if ((covElement.GetValue("VehIndex") == "") ||
                                (covElement.GetValue("VehIndex") == "-1"))

                                session.PolicyCoverageErrors.Add(err);
                            else
                                session.VehicleCoverageErrors.Add(err);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("<FilterID>"))
                {
                    session.AddInfo.DNQ.Filter = ex.Message.Substring(ex.Message.IndexOf("<FilterID>") + 10, ex.Message.Length - ex.Message.IndexOf("</FilterID>"));
                    return true;
                }
                if (ex.Message.Contains("<DNQ-Quote>"))
                {
                    if (ex.Message.Contains("<DNQReason>"))
                    {
                        session.AddInfo.DNQ.Reason = ex.Message.Substring(ex.Message.IndexOf("<DNQReason>") + 11, ex.Message.Length - ex.Message.IndexOf("</DNQReason>"));
                    }
                    if (ex.Message.Contains("<DNQDescription>"))
                    {
                        session.AddInfo.DNQ.Description = ex.Message.Substring(ex.Message.IndexOf("<DNQDescription>") + 16, ex.Message.Length - ex.Message.IndexOf("</DNQDescription>"));
                    }
                    session.AddInfo.DNQ.Knockout = "yes";
                    return true;
                }
                session.AddErrorMessage("Recalculate", session.AddInfo.CurrentPage, "KdQuoteLibrary", ex.Message);
                session.AddErrorMessage("Recalculate", session.AddInfo.CurrentPage, "KdQuoteLibrary", ex.StackTrace);
                LogUtility.LogError(ex.Message, "AutoQutoeFlow", "SessionServices", "Recalcultate");
                LogUtility.LogError(ex.StackTrace, "AutoQutoeFlow", "SessionServices", "Recalcultate");
                return false;
            }
        }
        public bool LoadDiscounts(WebSession session)
        {

            try
            {
                string sFlexXml = UD3Plugin.Instance().GetXMLFromWebSessionFlex(session.Guid.ToString());
                XElement response = XElement.Parse(sFlexXml);

                GetDiscounts(session, response);
            }
            catch (Exception ex)
            {
                LogUtility.LogError(ex.Message, "AutoQutoeFlow", "SessionServices", "LoadDiscounts");
                session.AddErrorMessage("LoadDiscounts", "", "KdQuoteLibrary", ex.Message + ";" + ex.StackTrace);
                return false;
            }
            return true;

        }
        public bool RatedSave(WebSession session)
        {
            if (!session.IsVibeState)
            {
                session.Quote.getPolicyInfo().setTermFactor(0.5);
                session.Quote.getPolicyInfo().setPrefPayLevel(0);
            }
            else
            {
                session.Quote.getPolicyInfo().setTermFactor(1);
                session.Quote.getPolicyInfo().setPrefPayLevel(7);
            }

            XElement request = XElement.Parse("<Request><DRCXML><RETURN><AiisQuoteMaster>" + session.Quote.serialize(null) + session.AddInfo.Serialize() + "</AiisQuoteMaster></RETURN></DRCXML></Request>");

            try
            {
                XElement response;

                response = UD3Plugin.Instance().RatedSaveWDiscounts(request);
                session.Quote = new AutoQuote.Autoquote();
                session.Quote.deserialize(response.Element("DRCXML").ToString(), null);
                session.AddInfo = new AddInfo();
                session.AddInfo = response.Element("DRCXML").Element("RETURN").Element("AiisQuoteMaster").Element("AddInfo").ToString().Deserialize<AddInfo>();

                GetDiscounts(session, response);

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("<FilterID>"))
                {
                    ex.Message.Substring(ex.Message.IndexOf("<FilterID>") + 10, ex.Message.IndexOf("</FilterID>") - (ex.Message.IndexOf("<FilterID>") + 10));
                    return true;
                }
                if (ex.Message.Contains("<DNQ-Quote>"))
                {
                    if (ex.Message.Contains("<DNQReason>"))
                    {
                        session.AddInfo.DNQ.Reason = ex.Message.Substring(ex.Message.IndexOf("<DNQReason>") + 11, ex.Message.IndexOf("</DNQReason>") - (ex.Message.IndexOf("<DNQReason>") + 11));
                    }
                    if (ex.Message.Contains("<DNQDescription>"))
                    {
                        session.AddInfo.DNQ.Description = ex.Message.Substring(ex.Message.IndexOf("<DNQDescription>") + 16, ex.Message.IndexOf("</DNQDescription>") - (ex.Message.IndexOf("<DNQDescription>") + 16));
                    }
                    session.AddInfo.DNQ.Knockout = "yes";
                    return true;
                }
                if (ex.Message.Contains("<ErrorInfo>"))
                {
                    LogUtility.LogError(ex.Message, "AutoQuoteFlow", "SessionServices", "RatedSave");
                    session.AddErrorMessage("RatedSave", "", "KdQuoteLibrary", ex.Message.Substring(ex.Message.IndexOf("<ErrorInfo>") + 11, ex.Message.IndexOf("</ErrorInfo>")));
                    return false;
                }
                LogUtility.LogError(ex.Message, "AutoQutoeFlow", "SessionServices", "RatedSave");
                session.AddErrorMessage("RatedSave", "", "KdQuoteLibrary", "Error in RatedSaveWDiscounts");
                return false;
            }
            return true;

        }

        private void GetDiscounts(WebSession session, XElement response)
        {
            session.Discounts = new List<Discount>();
            if ((response.Element("Discounts") != null) &&
                (response.Element("Discounts").Element("DiscountPremiums") != null))
            {
                foreach (XElement e in response.Element("Discounts").Element("DiscountPremiums").Elements())
                {
                    Discount newDisc = new Discount() { Name = e.Element("Name").Value.FormatDiscountDescription(session), Amount = e.Element("Amount").Value };
                    if (newDisc.Name.Contains("Preferred"))
                        newDisc.ID = "PreferredPayerDiscount";
                    if (newDisc.Name.Contains("Multi-Car"))
                        newDisc.ID = "MultiCarDiscount";
                    if (newDisc.Name.Contains("Homeownership"))
                        newDisc.ID = "HomeownershipDiscount";
                    if (newDisc.Name.Contains("Network"))
                    {
                        newDisc.ID = "NetworkDiscount";
                        newDisc.Name = newDisc.Name.Replace("iMingle", "");
                    }
                    if (newDisc.Name.Contains("Anti-Theft"))
                        newDisc.ID = "AntiTheftDiscount";
                    if (newDisc.Name.Contains("Free-A-Tree"))
                        newDisc.ID = "FreeATree";
                    if (newDisc.Name.Contains("Focused Driver"))
                        newDisc.ID = "FocusedDriver";
                    if (newDisc.Name.Contains("Multi-Policy"))
                        newDisc.ID = "MultiPolicy";
                    if (newDisc.Name.Contains("Happily Married"))
                        newDisc.ID = "HappilyMarried";
                    if (newDisc.Name.Contains("Safe"))
                        newDisc.ID = "SafeAndSound";
                    if (newDisc.Name.Contains("Passive Restraint"))
                        newDisc.ID = "PassiveRestraint";
                    if (response.Element("Discounts").Element("DiscountCoverages") != null)
                    {
                        foreach (XElement e2 in response.Element("Discounts").Element("DiscountCoverages").Elements())
                        {
                            if ((e2.Element("Name") != null) &&
                                (e2.Element("Name").Value.FormatDiscountDescription(session)) == newDisc.Name)
                            {
                                if (e2.Element("Description") != null)
                                    newDisc.Description = e2.Element("Description").Value;
                                if (e2.Element("ExpandedDesc") != null)
                                    newDisc.Description += e2.Element("ExpandedDesc").Value;
                                newDisc.Description.FormatDiscountDescription(session);
                                break;
                            }
                        }
                    }
                    session.Discounts.Add(newDisc);
                }
            }
        }
        public bool RecalculateAndReload(WebSession websession)
        {
            XElement newCovXml = UpdateCoveragesAndDiscounts(websession);
            if (newCovXml == null)
                return false;
            if (!SessionServices.Instance.Recalculate(websession))
                return false;
            if (!SessionServices.Instance.LoadCoveragesAndDiscounts(websession))
                return false;
            return true;
        }

    }
}
