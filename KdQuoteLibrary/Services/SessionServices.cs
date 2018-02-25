using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Collections;
using System.Configuration;
using SynchronousPluginHelper;
using SynchronousPluginHelper.BindFlow.Parameters;
//using SynchronousPluginHelper.BindFlow.Session;
//using QuoteFlowPlugin;
using KdQuoteLibrary.QuoteFlowHelper;

using UDILibrary.UDIExtensions.XMLSerialization;
using SiteLibrary;
using KdQuoteLibrary.Interfaces;

namespace KdQuoteLibrary.Services
{
    public class SessionServices : ISessionServices
    {
        //private static readonly SessionServices _sessionServices = new SessionServices();

        //private SessionServices()
        //{

        //}

        //public static SessionServices Instance
        //{
        //    get
        //    {
        //        return _sessionServices;
        //    }
        //}
        
        public WebSession Load(ref Guid guid, string zip, string ctid, string quoteNo = "")
        {
            try
            {
                WebSessionDRC websession = new WebSessionDRC();
                XElement request;
                if ((guid != null) && (guid != Guid.Empty))
                    request = new XElement("Request", new XElement("Guid", guid.ToString()), new XElement("ZipCode", zip), new XElement("ClickThruPartnerInfo", new XElement("CTID", ctid), new XElement("Custom", ""), new XElement("MarketKeyCode", ""), new XElement("SalesPhone", ""), new XElement("SalesHours", ""), new XElement("AMFAccountNumber", ""), new XElement("LandingPage", ""), new XElement("HTTPReferrer", ""), new XElement("Affinity", new XElement("IsAffinity", ""), new XElement("IsAgent", ""), new XElement("Logo", ""), new XElement("Description", ""))));
                else if (!string.IsNullOrWhiteSpace(quoteNo))
                    request = new XElement("Request", new XElement("QuoteNo", quoteNo), new XElement("ZipCode", zip), new XElement("ClickThruPartnerInfo", new XElement("CTID", ctid), new XElement("Custom", ""), new XElement("MarketKeyCode", ""), new XElement("SalesPhone", ""), new XElement("SalesHours", ""), new XElement("AMFAccountNumber", ""), new XElement("LandingPage", ""), new XElement("HTTPReferrer", ""), new XElement("Affinity", new XElement("IsAffinity", ""), new XElement("IsAgent", ""), new XElement("Logo", ""), new XElement("Description", ""))));
                else
                    request = new XElement("Request", new XElement("ZipCode", zip), new XElement("ClickThruPartnerInfo", new XElement("CTID", ctid), new XElement("Custom", ""), new XElement("MarketKeyCode", ""), new XElement("SalesPhone", ""), new XElement("SalesHours", ""), new XElement("AMFAccountNumber", ""), new XElement("LandingPage", ""), new XElement("HTTPReferrer", ""), new XElement("Affinity", new XElement("IsAffinity", ""), new XElement("IsAgent", ""), new XElement("Logo", ""), new XElement("Description", ""))));
                
                XElement response; 

                XMLSyncProcess process = new XMLSyncProcess(new XMLSyncHeader("QuoteFlowPlugin", "SessionServices", "Load"), request);

                //using (ProcessWCF client = new ProcessWCF())
                //{
                //    response = client.Execute(process);
                //    //response = new QuoteFlowPlugin.SessionServices().Load(request);
                //}
                response = XElement.Load(@"C:\VS projects\KemperQuoteAng2\LoadResponseXml2.txt");

                guid = new Guid(response.Element("DRCXML").Element("RETURN").Element("AiisQuoteMaster").Element("AddInfo").Element("Guid").Value);
                websession.Quote = new AutoQuote.Autoquote();
                websession.Quote.deserialize(response.Element("DRCXML").ToString(), null);
                websession.AddInfo = new AddInfo();
                websession.AddInfo = response.Element("DRCXML").Element("RETURN").Element("AiisQuoteMaster").Element("AddInfo").ToString().Deserialize<AddInfo>();
                websession.AddInfo.ClickThruPartnerInfo.Keywords = "";
                websession.AddInfo.ErrorMessages = new List<ErrorMessage>(); //clear any errors from last save.

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
                            Applied = element.Attribute("ID").Value.DiscountApply(websession)
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

                if (response.Element("Groups") != null)
                {
                    websession.GroupsXml = response.Element("Groups");
                    websession.Groups = new List<Group>();
                    foreach (XElement xe in response.Element("Groups").Elements())
                    {
                        websession.Groups.Add(new Group
                        {
                            Name = xe.Attribute("Name").Value,
                            AssociationNumber = xe.Attribute("AssociationNumber").Value,
                            DiscountLevel = xe.Attribute("DiscountLevel").Value,
                            GroupClass = xe.Attribute("GroupClass").Value,
                            GroupType = xe.Attribute("GroupType").Value
                        });
                    }
                }
                websession.Guid = guid;

                websession.IsVibeState = websession.Quote.getCustomer().getAddressStateCode().IsVibeState();
                websession.IsWebModelState = websession.Quote.getCustomer().getAddressStateCode().IsWebModelState();
                //websession.QuoteRedirect = GetQuoteRedirect(websession.Quote.getCustomer().getAddressStateCode());
                //websession.IsZipCodeDown = CheckStateDownByZip(websession.Quote.getCustomer().getAddressStateCode(), websession.Quote.getCustomer().getZipCode1().ToString() );

               


                return websession;
            }
            catch (Exception ex)
            {
                LoggingServices.Instance.logError(ex.Message, "Load", UDILibrary.Log.LogSeverity.Error);
                WebSessionDRC websession = new WebSessionDRC();
                websession.AddErrorMessage("Load", "", "KdQuoteLibrary.SessionServices", ex.Message);
                return websession;
            }
            
        }

        public Guid Save(WebSession session) //request.Element("DRCXML")
        {
            //DateTime systemDate = DateTime.Now;
            //DateTime.TryParse(session.AddInfo.SystemDate, out systemDate);

            //switch (session.AddInfo.CurrentPage)
            //{
            //    case "Household":
            //        if ((((WebSessionDRC)session).Quote.getCustomer().getNoOfAdd3Yrs() == 0) &&
            //            (((WebSessionDRC)session).Quote.getCustomer().getAddressVerificationTest() == 1))
            //        {
            //            OrderCredit(session);
            //        }
            //        break;
            //    case "PriorInsurance":
            //        if (((WebSessionDRC)session).Quote.getCustomer().getSocialSecurityNo().Length > 4)
            //        {
            //            OrderCredit(session);
            //        }
            //        break;
            //    case "PriorAddress":
            //        if (((WebSessionDRC)session).Quote.getCustomer().getNoOfAdd3Yrs() > 0)
            //        {
            //            OrderCredit(session);
            //        }
            //        break;
            //}
            //if (session.AddInfo.Drivers != null)
            //{
            //    for (int i = 0; i < session.AddInfo.Drivers.Count; i++)
            //    {
            //        if (((WebSessionDRC)session).Quote.getDrivers().count() > i)
            //        {
            //            if (session.AddInfo.RiskState == "GA")
            //            {
            //                if (session.AddInfo.Drivers[i].AiAge < 25)
            //                {
            //                    if (session.AddInfo.Drivers[i].AiFullTimeStudent == "1")
            //                        if ("A,B".Contains(session.AddInfo.Drivers[i].AiGPA))
            //                            ((WebSessionDRC)session).Quote.getDrivers().item(i).setGoodStudentDate(DateTime.Now);
            //                }
            //            }
            //            if (session.AddInfo.Drivers[i].AiDrivCourse == "2")
            //            {
            //                switch (session.AddInfo.RiskState)
            //                {
            //                    case "GA":
            //                        if (session.AddInfo.Drivers[i].AiAge < 25)
            //                        {
            //                            ((WebSessionDRC)session).Quote.getDrivers().item(i).setDrivTrainingTest(1);
            //                            if (session.AddInfo.Drivers[i].AiFullTimeStudent == "1")
            //                                if ("A,B".Contains(session.AddInfo.Drivers[i].AiGPA))
            //                                    ((WebSessionDRC)session).Quote.getDrivers().item(i).setGoodStudentDate(DateTime.Now);
            //                        }
            //                        else
            //                        {
            //                            ((WebSessionDRC)session).Quote.getDrivers().item(i).setDdcDiscount(1);
            //                            DateTime ddcDate = DateTime.Now.AddYears(-1);
            //                            ((WebSessionDRC)session).Quote.getDrivers().item(i).setDdcDate(ddcDate);
            //                        }
            //                        break;
            //                    case "SC":
            //                        if (session.AddInfo.Drivers[i].AiAge < 25)
            //                            ((WebSessionDRC)session).Quote.getDrivers().item(i).setDdcDiscount(0);
            //                        else
            //                            ((WebSessionDRC)session).Quote.getDrivers().item(i).setDdcDiscount(1);
            //                        break;

            //                    case "NJ":
            //                        ((WebSessionDRC)session).Quote.getDrivers().item(i).setDdcDiscount(1);
            //                        break;
            //                    default:
            //                        ((WebSessionDRC)session).Quote.getDrivers().item(i).setDdcDiscount(0);
            //                        break;
            //                }
            //            }
            //            else
            //            {
            //                ((WebSessionDRC)session).Quote.getDrivers().item(i).setDdcDiscount(0);
            //            }
            //        }
            //    }
            //}
            //XElement request = XElement.Parse("<Request><DRCXML><RETURN><AiisQuoteMaster>" + ((WebSessionDRC)session).Quote.serialize(null) + session.AddInfo.Serialize() + "</AiisQuoteMaster></RETURN></DRCXML></Request>");
            
            ////QuoteFlowPlugin.SessionServices ss = new QuoteFlowPlugin.SessionServices();
            ////XElement response = ss.Save(request);
            ////return new Guid(response.Element("Guid").Value);

            //XElement response;

            //XMLSyncProcess process = new XMLSyncProcess(new XMLSyncHeader("QuoteFlowPlugin", "SessionServices", "Save"), request);

            //using (ProcessWCF client = new ProcessWCF())
            //{
            //    response = client.Execute(process);
            //}
            
            return new Guid("5A635109-50D2-4268-95E6-6DEA6007EE32");
        }

        public XElement VerifyAddress(XElement request)
        {
            AddressServicePlugin.AddressService addressService = new AddressServicePlugin.AddressService();
            
            return  addressService.GetCorrectAddressInfoByXmlString(request);
        }
        public void OrderCredit(WebSession session)
        {
            //XElement request = new XElement("CreditServiceReq");
            ////PA rate differences - force cred.Inquire() instead of cred.FrugalInquiry() to ensure credit model is returned.
            //request.Add(new XElement("CreditScoreEffDate", "00/00/0000"));
            ////request.Add(new XElement("CreditScoreEffDate", session.Quote.getPolicyInfo().getQuoteEffDate().ToShortDateString()));
            //request.Add(new XElement("AdSysID", "1")); //quote
            //request.Add(new XElement("UserID","888"));
            //request.Add(new XElement("UserName","FROM WEB"));
            //request.Add(new XElement("RiskState",((WebSessionDRC)session).Quote.getCustomer().getAddressStateCode()));
            //request.Add(new XElement("UnderwritingCompany", ((WebSessionDRC)session).Quote.getPolicyInfo().getUnderwritingCoNo().ToString()));
            //request.Add(new XElement("QuoteNo",""));
            //request.Add(new XElement("EffectiveDate", ((WebSessionDRC)session).Quote.getQuoteInfo().getPolicyEffDate().ToShortDateString()));
            //request.Add(new XElement("MasterStateCode", ((WebSessionDRC)session).Quote.getCustomer().getAddressStateCode()));
            //request.Add(new XElement("Address",
            //    new XElement("AddressLine1", ((WebSessionDRC)session).Quote.getCustomer().getAddressLine1() ?? ""),
            //    new XElement("AddressLine2", ((WebSessionDRC)session).Quote.getCustomer().getAddressLine2() ?? ""),
            //    new XElement("City", ((WebSessionDRC)session).Quote.getCustomer().getAddressCity() ?? ""),
            //    new XElement("State", ((WebSessionDRC)session).Quote.getCustomer().getAddressStateCode() ?? ""),
            //    new XElement("Zip", ((WebSessionDRC)session).Quote.getCustomer().getZipCode1().ToString("0000"))
            //    ));
            //if (!string.IsNullOrWhiteSpace(session.AddInfo.AddressLine1))
            //{
            //    request.Add(new XElement("PreviousAddress",
            //        new XElement("Address",
            //              new XElement("AddressLine1",session.AddInfo.AddressLine1 ?? ""),
            //              new XElement("Apartment", session.AddInfo.AddressLine2 ?? ""),
            //              new XElement("City",session.AddInfo.AddressCity ?? ""),
            //              new XElement("State",session.AddInfo.AddressState ?? ""),
            //              new XElement("Zip",session.AddInfo.AddressZip ?? "")
            //    )));
            //}
            //request.Add(new XElement("DrivFirst", ((WebSessionDRC)session).Quote.getDrivers().item(0).getDrivFirst()));
            //request.Add(new XElement("DrivMiddle", ((WebSessionDRC)session).Quote.getDrivers().item(0).getDrivMiddle()));
            //request.Add(new XElement("DrivLast", ((WebSessionDRC)session).Quote.getDrivers().item(0).getDrivLast()));
            //request.Add(new XElement("DrivSex", ((WebSessionDRC)session).Quote.getDrivers().item(0).getDrivSex().ToString()));
            //request.Add(new XElement("SSN", ((WebSessionDRC)session).Quote.getCustomer().getSocialSecurityNo()));
            //request.Add(new XElement("DOB", ((WebSessionDRC)session).Quote.getDrivers().item(0).getBirthDateOfDriv().ToShortDateString()));

            //XMLSyncProcess process = new XMLSyncProcess(new XMLSyncHeader("CreditServicesPlugin", "CreditServices", "OrderCredit"), request);

            //XElement response;
            //using (ProcessWCF client = new ProcessWCF())
            //{
            //    response = client.Execute(process);
            //}
            
            //if (response.Element("CreditInfo") != null)
            //{
            //    if ((response.Element("ReturnValue") != null) &&
            //        (response.Element("ReturnValue").Value == "0"))
            //    {
            //        DateTime creditScoreEffDate = DateTime.MinValue;
            //        int creditSource = 0;
            //        if (response.Element("CreditInfo").Element("CreditScoreEffDate") != null)
            //            DateTime.TryParse(response.Element("CreditInfo").Element("CreditScoreEffDate").Value, out creditScoreEffDate);
            //        if (response.Element("CreditInfo").Element("CreditSource") != null)
            //            int.TryParse(response.Element("CreditInfo").Element("CreditSource").Value, out creditSource);
            //        ((WebSessionDRC)session).Quote.getPolicyInfo().setCreditScoreEffDate(creditScoreEffDate);
            //        ((WebSessionDRC)session).Quote.getPolicyInfo().setCreditSource(creditSource);
            //    }
            //    int creditScoreType = 0;
            //    if (response.Element("CreditInfo").Element("CreditScoreType") != null)
            //        int.TryParse(response.Element("CreditInfo").Element("CreditScoreType").Value, out creditScoreType);
            //    string creditScore = "0";
            //    if (response.Element("CreditInfo").Element("CreditScore") != null)
            //        creditScore = response.Element("CreditInfo").Element("CreditScore").Value;
            //    else
            //        if (response.Element("CreditInfo").Element("DefaultScore") != null)
            //            creditScore = response.Element("CreditInfo").Element("DefaultScore").Value;

            //    ((WebSessionDRC)session).Quote.getPolicyInfo().setCreditScoreType(creditScoreType);
            //    ((WebSessionDRC)session).Quote.getCustomer().setCreditScore(creditScore.PadLeft(4, '0'));

            //    int creditVendor = 0;
            //    if (response.Element("CreditInfo").Element("CreditVendor") != null)
            //        int.TryParse(response.Element("CreditInfo").Element("CreditVendor").Value, out creditVendor);
            //    ((WebSessionDRC)session).Quote.getPolicyInfo().setCreditVendor(creditVendor);
            //    if (response.Element("CreditInfo").Element("CreditModel") != null)
            //        ((WebSessionDRC)session).Quote.getCustomer().setCreditModel(response.Element("CreditInfo").Element("CreditModel").Value);
            //    if (response.Element("CreditInfo").Element("ReturnedSSN") != null)
            //        ((WebSessionDRC)session).Quote.getCustomer().setSocialSecurityNo(response.Element("CreditInfo").Element("ReturnedSSN").Value);
            //    if ((response.Element("CreditInfo").Element("MortgageExists") != null) &&
            //        (response.Element("CreditInfo").Element("MortgageExists").Value == "1"))
            //        ((WebSessionDRC)session).Quote.getCustomer().setHomeownerVerifyTest(1);
            //    if (response.Element("CreditInfo").Element("CreditReportId") != null)
            //        ((WebSessionDRC)session).AddInfo.CredRptID = response.Element("CreditInfo").Element("CreditReportId").Value ?? "0";
            //}
			
        }

        
        public bool LoadCoveragesAndDiscounts(WebSession session)
        {
            XElement request = XElement.Parse("<Request><Guid>" + session.Guid.ToString() + "</Guid></Request>");

            decimal pligaFee = 0; //optionFee1
            decimal TotalStateFeeOption1 = 0;
            XMLSyncProcess process = new XMLSyncProcess(new XMLSyncHeader("UD3Plugin", "AutoQuoteServices", "LoadQuote"), request);
            using (ProcessWCF client = new ProcessWCF())
            {
                try
                {
                    XElement response = client.Execute(process);

                    if (response.Element("Coverages") != null)
                    {
                        if (response.Element("Coverages").Element("PolicyCoverages") != null)
                        {
                            session.PolicyCoverages = LinqUtilities.DeserializePolicyCoverages(response.Element("Coverages"));
                            foreach (Coverage c in session.PolicyCoverages)
                            {
                                if (c.CovCode == "OptionFee1")
                                    pligaFee = c.PremiumNumeric; //optionFee1
                            }
                        }
                        if (response.Element("Coverages").Element("VehicleCoverages") != null)
                        {
                            session.VehicleCoverages = LinqUtilities.DeserializeVehicleCoverages(response.Element("Coverages"));                            
                        }
                        if (response.Element("Coverages").Element("EnhancedCoverages") != null)
                        {
                            session.EnhancedCoverages = LinqUtilities.DeserializeEnhancedCoverages(response.Element("Coverages"));
                        }
                        if (response.Element("Discounts").Element("DiscountCoverages") != null)
                        {
                            session.CoveragePageDiscounts = LinqUtilities.DeserializeCoverageDiscounts(response, (WebSessionDRC)session);
                            session.TotalDiscountSavings = 0;
                            session.TotalDiscountSavingsWithoutPreferredPayer = 0;
                            foreach (Discount d in session.CoveragePageDiscounts)
                            {
                                session.TotalDiscountSavings += d.AmountNumeric;
                                if (d.ID != "PreferredPayerDiscount")
                                    if (d.Purchased)
                                        session.TotalDiscountSavingsWithoutPreferredPayer += d.AmountNumeric;
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
                            for (int i = 0; i < ((WebSessionDRC)session).Quote.getDownPayOptions().count(); i++)
                            {
                                payPlanIndexTable[((WebSessionDRC)session).Quote.getDownPayOptions().item(i).getDpoBplan()] = i;
                            }
                            //TODO finish LinqUtilities DeserializePayPlans().
                            //session.PayPlans = LinqUtilities.DeserializePayPlans(response.Element("PayPlans"), session);// ;
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
                                string termText = (((WebSessionDRC)session).Quote.getPolicyInfo().getTermFactor() == .5) ? " total" : "/year";
                                switch (pp.Name)
                                {
                                    case "Quarterly":
                                        pp.Installments = 3;
                                        pp.DownDivisor = 4;
                                        pp.InstallmentType = "Quarterly";
                                        pp.Installment = (pp.Yearly - pligaFee - TotalStateFeeOption1) / 4;
                                        pp.Downpayment = pp.Installment + pligaFee + TotalStateFeeOption1;
                                        pp.Description = string.Format("{0:c} Quarterly ({1:c}" + termText + ")", pp.Installment, pp.Yearly);
                                        break;
                                    case "Semi-Annually":
                                        pp.Installments = 1;
                                        pp.DownDivisor = 2;
                                        pp.InstallmentType = "Semi-Annually";
                                        pp.Installment = (pp.Yearly - pligaFee - TotalStateFeeOption1) / 2;
                                        pp.Downpayment = pp.Installment + pligaFee + TotalStateFeeOption1;
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
                                        if ("AZ,IL,IN,OH,TN,TX,WI".Contains(((WebSessionDRC)session).Quote.getCustomer().getAddressStateCode()))
                                        {
                                            pp.Installments = 11;
                                            pp.DownDivisor = 12;
                                            pp.Downpayment = pp.Yearly / 12;
                                            pp.Installment = (pp.Yearly - pp.Downpayment) / 11;
                                        }
                                        else
                                        {
                                            pp.Installments = 10;
                                            pp.DownDivisor = 11;
                                            pp.Downpayment = (pp.Yearly - pligaFee - TotalStateFeeOption1) / 11 + pligaFee + TotalStateFeeOption1;
                                            pp.Installment = (pp.Yearly - pp.Downpayment) / 10;
                                        }
                                        pp.InstallmentType = "Monthly";
                                        pp.Description = string.Format("{0:c} Monthly ({1:c}" + termText + ")", pp.Installment, pp.Yearly);
                                        break;
                                    case "1/8 Down":
                                        if ("AZ,IL,IN,OH,TN,TX,WI".Contains(((WebSessionDRC)session).Quote.getCustomer().getAddressStateCode()))
                                        {
                                            pp.Installments = 11;
                                            pp.DownDivisor = 8;
                                            pp.Downpayment = pp.Yearly / 8;
                                            pp.Installment = (pp.Yearly - pp.Downpayment) / 11;
                                        }
                                        else
                                        {
                                            pp.Installments = 10;
                                            pp.DownDivisor = 8;
                                            pp.Downpayment = (pp.Yearly - pligaFee - TotalStateFeeOption1) / 8 + pligaFee + TotalStateFeeOption1;
                                            pp.Installment = (pp.Yearly - pp.Downpayment) / 10;
                                        }
                                        pp.InstallmentType = "Monthly";
                                        pp.Description = string.Format("{0:c} Monthly ({1:c}" + termText + ")", pp.Installment, pp.Yearly);
                                        break;
                                    case "1/6 Down":
                                        if ("AZ,IL,IN,OH,TN,TX,WI".Contains(((WebSessionDRC)session).Quote.getCustomer().getAddressStateCode()))
                                        {
                                            pp.Installments = 11;
                                            pp.DownDivisor = 6;
                                            pp.Downpayment = pp.Yearly / 6;
                                            pp.Installment = (pp.Yearly - pp.Downpayment) / 11;
                                        }
                                        else
                                        {
                                            pp.Installments = 10;
                                            pp.DownDivisor = 6;
                                            pp.Downpayment = (pp.Yearly - pligaFee - TotalStateFeeOption1) / 6 + pligaFee + TotalStateFeeOption1;
                                            pp.Installment = (pp.Yearly - pp.Downpayment) / 10;
                                        }
                                        pp.InstallmentType = "Monthly";
                                        pp.Description = string.Format("{0:c} Monthly ({1:c}" + termText + ")", pp.Installment, pp.Yearly);
                                        break;

                                    case "Payroll":
                                        pp.Installments = 0;
                                        pp.DownDivisor = 1;
                                        pp.InstallmentType = "Payroll Deduction";
                                        pp.Description = string.Format("{0:c} Payroll Deduction", pp.Yearly);
                                        break;

                                    case "2 Payments(Non Vibe)":
                                        pp.Yearly = session.TotalPremium;
                                        pp.Installments = 1;
                                        pp.DownDivisor = 2;
                                        pp.InstallmentType = "Payment";
                                        if (payPlanIndexTable["2 PAYMENTS"] != null)
                                        {
                                            pp.Downpayment = (decimal)((WebSessionDRC)session).Quote.getDownPayOptions().item((int)payPlanIndexTable["2 PAYMENTS"]).getDpoRdnamt();
                                            pp.Installment = (decimal)((WebSessionDRC)session).Quote.getDownPayOptions().item((int)payPlanIndexTable["2 PAYMENTS"]).getDpoInstamt();
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
                                            pp.Downpayment = (decimal)((WebSessionDRC)session).Quote.getDownPayOptions().item((int)payPlanIndexTable["3 PAYMENTS"]).getDpoRdnamt();
                                            pp.Installment = (decimal)((WebSessionDRC)session).Quote.getDownPayOptions().item((int)payPlanIndexTable["3 PAYMENTS"]).getDpoInstamt();
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
                                            pp.Downpayment = (decimal)((WebSessionDRC)session).Quote.getDownPayOptions().item((int)payPlanIndexTable["4 PAYMENTS"]).getDpoRdnamt();
                                            pp.Installment = (decimal)((WebSessionDRC)session).Quote.getDownPayOptions().item((int)payPlanIndexTable["4 PAYMENTS"]).getDpoInstamt();
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
                                            AutoQuote.DownPayOption dpo = ((WebSessionDRC)session).Quote.getDownPayOptions().item((int)payPlanIndexTable["MONTHLY"]);
                                            pp.Downpayment = (decimal)dpo.getDpoRdnamt();
                                            pp.Installment = (decimal)(((WebSessionDRC)session).Quote.getCoverages().item(0).getSixMonthPremiums().getSmTotalPolPrem() +
                                                ((WebSessionDRC)session).Quote.getPolicyInfo().getPolicyFee() +
                                                ((WebSessionDRC)session).Quote.getPolicyInfo().getOptionFee1() -
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
                                            pp.Downpayment = (decimal)((WebSessionDRC)session).Quote.getDownPayOptions().item((int)payPlanIndexTable["PAY IN FULL"]).getDpoRdnamt();
                                            pp.Installment = (decimal)((WebSessionDRC)session).Quote.getDownPayOptions().item((int)payPlanIndexTable["PAY IN FULL"]).getDpoInstamt();
                                        }
                                        pp.Yearly = pp.Downpayment;
                                        pp.Description = string.Format("{0:c} Annually (Pay-In-Full)", pp.Yearly);
                                        session.AnnualPayPlanDiscountSavings = pp.Discount;
                                        break;
                                    case "Payroll Deduction(Non Vibe)":
                                        pp.Yearly = (decimal)((WebSessionDRC)session).Quote.getCoverages().item(0).getPolicyCoverage().getPrdPrem();
                                        pp.Installments = 0;
                                        pp.DownDivisor = 1;
                                        pp.InstallmentType = "Payroll Deduction";
                                        pp.Description = string.Format("{0:c} Payroll Deduction", pp.Yearly);
                                        break;
                                }
                                pp.Value = string.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}", pp.Installments, pp.Installment, pp.Downpayment, pp.Discount, pp.InstallmentType, pp.Yearly, pp.DownDivisor, pp.ID);
                                //jrenz ssr09870 4/15/2016 Pay plans that don't apply to this quote's state will not have payPlanIndexTable entry, so will calculate $0 payments.
                                if (pp.Downpayment > 0 || pp.Installment > 0)
                                {
                                    session.PayPlans.Add(pp);
                                    if (response.Element("PayPlans").GetValue("SelectedPayPlan") == "")
                                    {
                                        if (e.Attribute("id").Value == "4")
                                        {
                                            session.SelectedPayPlan = pp;
                                        }
                                    }
                                    if (e.Attribute("id").Value == response.Element("PayPlans").GetValue("SelectedPayPlan"))
                                    {
                                        session.SelectedPayPlan = pp;
                                    }
                                    if (session.SelectedPayPlan == null)
                                        session.SelectedPayPlan = pp;
                                }
                                else
                                {
                                    //udinzs ssr9953 payroll
                                    if (pp.Installment == 0 && pp.Name.ToUpper().Contains("PAYROLL"))
                                    {
                                        session.PayPlans.Add(pp);
                                        if (e.Attribute("id").Value == response.Element("PayPlans").GetValue("SelectedPayPlan"))
                                        {
                                            session.SelectedPayPlan = pp;
                                        }
                                    }
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
                                            if ((e2.Attribute("default").Value == "Y") ||
                                                (session.AddInfo.HOIRenterInfo.HOIRenterDeductible == 0))
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
        }

        
        
        public bool LoadDiscounts(WebSession session)
        {
            XElement request = XElement.Parse("<Request><Guid>" + session.Guid.ToString() + "</Guid></Request>");

            XMLSyncProcess process = new XMLSyncProcess(new XMLSyncHeader("UD3Plugin", "AutoQuoteServices", "LoadQuote"), request);
            using (ProcessWCF client = new ProcessWCF())
            {
                try
                {
                    XElement response = client.Execute(process);

                    LinqUtilities.GetDiscounts(((WebSessionDRC)session), response);
                }
                catch (Exception ex)
                {
                    LoggingServices.Instance.logError(ex.Message, "LoadDiscounts", UDILibrary.Log.LogSeverity.Error);
                    session.AddErrorMessage("LoadDiscounts", "", "KdQuoteLibrary", ex.Message + ";" + ex.StackTrace);
                    return false;
                }
                return true;
            }
        }
        public bool RatedSave(WebSession session)
        {
            //if (!session.IsVibeState)
            //{
            //    ((WebSessionDRC)session).Quote.getPolicyInfo().setTermFactor(0.5);
            //    ((WebSessionDRC)session).Quote.getPolicyInfo().setPrefPayLevel(0);
            //}
            //else
            //{
            //    ((WebSessionDRC)session).Quote.getPolicyInfo().setTermFactor(1);
            //    ((WebSessionDRC)session).Quote.getPolicyInfo().setPrefPayLevel(7);
            //}
            //((WebSessionDRC)session).Quote.getPolicyInfo().setHoProduct("0");
            //bool getNonVibeMPD = false;
            //double mpdPrem = 0;
            //double nonMpdPrem = 0;
            //if (session.AddInfo.HOIRenterInfo != null)
            //    if (session.AddInfo.HOIRenterInfo.HOIRenterProvide == HOIRenterInfo.EnumRenterProvide.Yes)
            //        //rate twice to get savings for non vibe.
            //        if (!session.IsVibeState)
            //                getNonVibeMPD = true;
            ////jrenz ssr09870 4/12/2016
            //if (((WebSessionDRC)session).Quote.getPolicyInfo().getNoOfDaysLapsed() == 0)
            //    ((WebSessionDRC)session).Quote.getPolicyInfo().setNoOfDaysLapsed(2);
            //if (((WebSessionDRC)session).Quote.getCustomer().getCurrentCarrierNo() == 0)
            //    ((WebSessionDRC)session).Quote.getCustomer().setCurrentCarrierNo(19);
            //if (((WebSessionDRC)session).Quote.getCustomer().getCurrentCarrierType() == 0)
            //    ((WebSessionDRC)session).Quote.getCustomer().setCurrentCarrierType(3);
            //if (((WebSessionDRC)session).Quote.getCustomer().getCurrentLimits() == 0)
            //{
            //    if (((WebSessionDRC)session).Quote.getCustomer().getCurrentCarrierNo() != 20)
            //        ((WebSessionDRC)session).Quote.getCustomer().setCurrentLimits(6);
            //}
            //XElement request = XElement.Parse("<Request><DRCXML><RETURN><AiisQuoteMaster>" + ((WebSessionDRC)session).Quote.serialize(null) + session.AddInfo.Serialize() + "</AiisQuoteMaster></RETURN></DRCXML></Request>");

            //XMLSyncProcess process;

            //using (ProcessWCF client = new ProcessWCF())
            //{
            //    try
            //    {
            //        XElement response;
            //        if (getNonVibeMPD) //rate twice to get with and w/out MPD
            //        {
            //            ((WebSessionDRC)session).Quote.getPolicyInfo().setMultiPolicyTest(2);
            //            ((WebSessionDRC)session).Quote.getPolicyInfo().setHoProduct("2");
            //            request = XElement.Parse("<Request><DRCXML><RETURN><AiisQuoteMaster>" + ((WebSessionDRC)session).Quote.serialize(null) + session.AddInfo.Serialize() + "</AiisQuoteMaster></RETURN></DRCXML></Request>");
            //            process = new XMLSyncProcess(new XMLSyncHeader("UD3Plugin", "AutoQuoteServices", "RatedSaveWDiscounts"), request);
            //            response = client.Execute(process);
            //            //response = new UD3Plugin.AutoQuoteServices().RatedSaveWDiscounts(request);
            //            ((WebSessionDRC)session).Quote = new AutoQuote.Autoquote();
            //            ((WebSessionDRC)session).Quote.deserialize(response.Element("DRCXML").ToString(), null);
            //            mpdPrem = ((WebSessionDRC)session).Quote.getPolicyInfo().getTotalPremium();
            //            ((WebSessionDRC)session).Quote.getPolicyInfo().setMultiPolicyTest(0);
            //            ((WebSessionDRC)session).Quote.getPolicyInfo().setHoProduct("");
            //            request = XElement.Parse("<Request><DRCXML><RETURN><AiisQuoteMaster>" + ((WebSessionDRC)session).Quote.serialize(null) + session.AddInfo.Serialize() + "</AiisQuoteMaster></RETURN></DRCXML></Request>");
            //        }

            //        process = new XMLSyncProcess(new XMLSyncHeader("UD3Plugin", "AutoQuoteServices", "RatedSaveWDiscounts"), request);
            //        response = client.Execute(process);
            //        //response = new UD3Plugin.AutoQuoteServices().RatedSaveWDiscounts(request);
            //        ((WebSessionDRC)session).Quote = new AutoQuote.Autoquote();
            //        ((WebSessionDRC)session).Quote.deserialize(response.Element("DRCXML").ToString(), null);
            //        session.AddInfo = new AddInfo();
            //        session.AddInfo = response.Element("DRCXML").Element("RETURN").Element("AiisQuoteMaster").Element("AddInfo").ToString().Deserialize<AddInfo>();

            //        if (getNonVibeMPD) //rate twice to get with and w/out MPD
            //        {
            //            nonMpdPrem = ((WebSessionDRC)session).Quote.getPolicyInfo().getTotalPremium();
            //            session.AddInfo.multipolicydiscountNumeric = (decimal)(nonMpdPrem - mpdPrem);
            //            session.AddInfo.multipolicydiscount = session.AddInfo.multipolicydiscountNumeric.ToString("C");
            //        }

            //        LinqUtilities.GetDiscounts(((WebSessionDRC)session), response);

            //    }
            //    catch (Exception ex)
            //    {
            //        if (ex.Message.Contains("<FilterID>"))
            //        {
            //            session.AddErrorMessage("RatedSave", "", "KdQuoteLibrary", session.AddInfo.DNQ.Reason = ex.Message.Substring(ex.Message.IndexOf("<FilterID>") + 10, ex.Message.IndexOf("</FilterID>") - (ex.Message.IndexOf("<FilterID>") + 10)));
            //            return false;
            //        }
            //        if (ex.Message.Contains("<DNQ-Quote>"))
            //        {
            //            if (ex.Message.Contains("<DNQReason>"))
            //            {
            //                session.AddInfo.DNQ.Reason = ex.Message.Substring(ex.Message.IndexOf("<DNQReason>") + 11, ex.Message.IndexOf("</DNQReason>") - (ex.Message.IndexOf("<DNQReason>") + 11));
            //            }
            //            if (ex.Message.Contains("<DNQDescription>"))
            //            {
            //                session.AddInfo.DNQ.Description = ex.Message.Substring(ex.Message.IndexOf("<DNQDescription>") + 16, ex.Message.IndexOf("</DNQDescription>") - (ex.Message.IndexOf("<DNQDescription>") + 16));
            //            }
            //            session.AddInfo.DNQ.Knockout = "yes";
            //            return true;
            //        }
            //        if (ex.Message.Contains("<ErrorInfo>"))
            //        {
            //            LoggingServices.Instance.logError(ex.Message + ";" + ex.StackTrace, "RatedSave", UDILibrary.Log.LogSeverity.Error);
            //            session.AddErrorMessage("RatedSave", "", "KdQuoteLibrary", ex.Message.Substring(ex.Message.IndexOf("<ErrorInfo>") + 11, ex.Message.IndexOf("</ErrorInfo>")));
            //            return false;
            //        }
            //        LoggingServices.Instance.logError(ex.Message, "RatedSave", UDILibrary.Log.LogSeverity.Error);
            //        session.AddErrorMessage("RatedSave", "", "KdQuoteLibrary", "Error in RatedSaveWDiscounts" + ex.Message);
            //        return false;
            //    }
            //}
            return true;
            
        }

        
        public bool Recalculate(WebSession websession, bool reload = false)
        {
            //XElement newCovXml = SessionUtilities.UpdateCoveragesAndDiscounts(websession);
            //if (newCovXml == null)
            //    return false;
            ////jrenz 6/23/2015 Hack to force multipolicydiscountTest in UD3Plugin.AutoQuoteServices.UpdateSelectedDiscounts during QuoteCoverage page.
            //if (newCovXml.Element("BuyNow") != null)
            //    newCovXml.Element("BuyNow").Value = "NO";
            //else
            //    newCovXml.Add(new XElement("BuyNow", "NO"));
            //if (!SessionUtilities.RecalculateQuote(newCovXml, websession))
            //    return false;
            ////reload to get update quote premiums
            //if (!SessionUtilities.GetQuote(websession))
            //    return false;
            //if (reload)
            //    if (!LoadCoveragesAndDiscounts(websession))
            //        return false;
            return true;
        }
        
        //udinzs 9510
        public bool CheckStateDownByZip(string state, string zipcode)
        {
            return false;
           //return SiteLibrary.StateServices.CheckStateDownByZip(state, zipcode);
        }

        public string GetQuoteRedirect(string state)
        {
            return SiteLibrary.StateServices.GetAutoQuoteRedirect(state);
        }

        public GetSalesInfoResponse GetSalesInfo(WebSession session)
        {
            int amfAccountNo = (int)((WebSessionDRC)session).Quote.getPolicyInfo().getAmfAccountNo();
            String keycode = ((WebSessionDRC)session).Quote.getCustomer().getMarketKeyCode(); 
            int clickThruID = 0;
            //jrenz SSR 9868 and SSR 9769 portal recalls 
            int.TryParse(session.AddInfo.ClickThruPartnerInfo.CTID, out clickThruID);

            String domain = System.Web.HttpContext.Current.Request.Url.Host;

            String origApp = "UD.com";
            int marketBrand = ((WebSessionDRC)session).Quote.getPolicyInfo().getMarketBrand();

            String key = "KdQuoteLibrary_SessionServices_GetSalesHours_" + amfAccountNo + "_" + keycode + "_" + clickThruID + "_" + domain;
            GetSalesInfoResponse salesInfo = null; 
            
            GetSalesInfoRequest request = new GetSalesInfoRequest { AMFAccountNo = amfAccountNo, MarketKeyCode = keycode, CTID = clickThruID, Domain = domain, OrigApp = origApp, MarketBrand = marketBrand };
            XMLSyncProcess process = new XMLSyncProcess(new XMLSyncHeader("BindFlowPlugin", "LookupServices", "GetSalesInfo"), XElement.Parse(request.Serialize<GetSalesInfoRequest>()), new XMLSyncDebug("KdQuoteLibrary"));

            using (ProcessWCF client = new ProcessWCF())
            {
                salesInfo = client.Execute(process).ToString().Deserialize<GetSalesInfoResponse>();

            }
            return salesInfo;

            //GetSalesInfoResponse salesInfo = (GetSalesInfoResponse)CacheManager.GetData(key);

            //if (salesInfo == null)
            //{
            //CacheManager.Add(key, salesInfo, CacheManager.ExpireEveryDayAtSix);
        }
    }
}
