using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using AutoQuoteLibrary;
using UDILibrary.Log;
using System.ComponentModel;
using System.Configuration;

namespace AutoQuoteLibrary.BL
{
    public enum StatesEnum
    {
        [Description("NONE")]
        NONE = 0,
        [Description("ALABAMA")]
        AL = 1,
        [Description("ARIZONA")]
        AZ = 2,
        [Description("ARKANSAS")]
        AR = 3,
        [Description("CALIFORNIA")]
        CA = 4,
        [Description("COLORADO")]
        CO = 5,
        [Description("CONNECTICUT")]
        CT = 6,
        [Description("DELAWARE")]
        DE = 7,
        [Description("DISTRICT OF COLUMBIA")]
        DC = 8,
        [Description("FLORIDA")]
        FL = 9,
        [Description("GEORGIA")]
        GA = 10,
        [Description("IDAHO")]
        ID = 11,
        [Description("ILLINOIS")]
        IL = 12,
        [Description("INDIANA")]
        IN = 13,
        [Description("IOWA")]
        IA = 14,
        [Description("KANSAS")]
        KS = 15,
        [Description("KENTUCKY")]
        KY = 16,
        [Description("LOUISIANA")]
        LA = 17,
        [Description("MAINE")]
        ME = 18,
        [Description("MARYLAND")]
        MD = 19,
        [Description("MASSACHUSETTS")]
        MA = 20,
        [Description("MICHIGAN")]
        MI = 21,
        [Description("MINNESOTA")]
        MN = 22,
        [Description("MISSISSIPPI")]
        MS = 23,
        [Description("MISSOURI")]
        MO = 24,
        [Description("MONTANA")]
        MT = 25,
        [Description("NEBRASKA")]
        NE = 26,
        [Description("NEVADA")]
        NV = 27,
        [Description("NEW HAMPSHIRE")]
        NH = 28,
        [Description("NEW JERSEY")]
        NJ = 29,
        [Description("NEW MEXICO")]
        NM = 30,
        [Description("NEW YORK")]
        NY = 31,
        [Description("NORTH CAROLINA")]
        NC = 32,
        [Description("NORTH DAKOTA")]
        ND = 33,
        [Description("OHIO")]
        OH = 34,
        [Description("OKLAHOMA")]
        OK = 35,
        [Description("OREGON")]
        OR = 36,
        [Description("PENNSYLVANIA")]
        PA = 37,
        [Description("RHODE ISLAND")]
        RI = 38,
        [Description("SOUTH CAROLINA")]
        SC = 39,
        [Description("SOUTH DAKOTA")]
        SD = 40,
        [Description("TENNESSEE")]
        TN = 41,
        [Description("TEXAS")]
        TX = 42,
        [Description("UTAH")]
        UT = 43,
        [Description("VERMONT")]
        VT = 44,
        [Description("VIRGINIA")]
        VA = 45,
        [Description("WASHINGTON")]
        WA = 46,
        [Description("WEST VIRGINIA")]
        WV = 47,
        [Description("WISCONSIN")]
        WI = 48,
        [Description("WYOMING")]
        WY = 49,
        [Description("HAWAII")]
        HI = 52,
        [Description("ALASKA")]
        AK = 54,
    }
    public class SessionPlugin
    {
        private static readonly SessionPlugin _sessionServices = new SessionPlugin();

        public static SessionPlugin Instance
        {
            get
            {
                return _sessionServices;
            }
        }
        public XElement Load(XElement request)
        {
            Quote quote = new Quote();
            QuestionPlugin questions = new QuestionPlugin();
            DiscountPlugin discounts = new DiscountPlugin();
            AccidentViolationPlugin accidentViolations = new AccidentViolationPlugin();
            if (request.Element("ZipCode") != null && request.Element("ZipCode").Value != String.Empty)
            {
                quote.AiisQuoteMaster.getCustomer().setZipCode1(Int32.Parse(request.Element("ZipCode").Value));
            }
            
            //Boolean newQuote = false;
            XElement response = new XElement("Response");
            //bool isRecall = false;
            string refQuote="";
            
            
            if (request.Element("Guid") != null && request.Element("Guid").Value != String.Empty)
            {

                quote.AddInfo.Element("Guid").Value = request.Element("Guid").Value;
                using (var context = new AutoQuoteEntitie7())
                {
                    Guid guid = Guid.Empty;
                    Guid.TryParse(request.Element("Guid").Value, out guid);
                    var session = from s in context.tbl_web_session
                                  where s.guid.Equals(guid)
                                select s;
                    if (session.Count() == 1)
                    {
                        //newQuote = false;
                        quote.Deserialize(XElement.Parse(session.First().drc_xml));
                    }
                    else
                    {
                        //newQuote = true;
                        SetDefaults(quote);
                    }
                    
                }
                if (quote.AiisQuoteMaster.getQuoteInfo().getQuoteNo0() == "" && request.Element("ClickThruPartnerInfo") != null &&
                    request.Element("ClickThruPartnerInfo").Element("CTID") != null &&
                    !String.IsNullOrEmpty(request.Element("ClickThruPartnerInfo").Element("CTID").Value))
                {
                    quote.AddInfo.Element("ClickThruPartnerInfo").ReplaceWith(request.Element("ClickThruPartnerInfo"));
                }
                else
                {
                    //if (string.IsNullOrEmpty(request.Element("ClickThruPartnerInfo").Element("CTID").Value) && request.Element("Referral") != null)
                    //{
                    //    if (request.Element("Referral").Element("ReferrerQuoteNo") != null && request.Element("Referral").Element("ReferrerQuoteNo").Value != String.Empty)
                    //        refQuote = request.Element("Referral").Element("ReferrerQuoteNo").Value;
                    //}
                }
            }
            else
            {
                //newQuote = true;
                SetNewQuoteRequestValue(quote,request);
                SetDefaults(quote);
            }

            //tc #8250 12-27-2011 - Redirect
            if (request.Element("Redirect") != null && request.Element("Redirect").Value != "")
            {
                if (quote.AddInfo.Element("Redirect") == null)
                {
                    quote.AddInfo.Element("Redirect").Add(new XElement("Redirect", request.Element("Redirect").Value));
                }
                else
                {
                    quote.AddInfo.Element("Redirect").Value = request.Element("Redirect").Value;
                }
            }

            //ysang 7123 3/18/2011 for new landing pages: surehits and noSurehits 
            if (request.Element("ClickThruPartnerInfo") != null && request.Element("ClickThruPartnerInfo").Element("RenterAndAuto") != null)
            {
                if (request.Element("ClickThruPartnerInfo").Element("RenterAndAuto").Value != "")
                {
                    if (quote.AddInfo.Element("ClickThruPartnerInfo").Element("RenterAndAuto") == null)
                        quote.AddInfo.Element("ClickThruPartnerInfo").Add(new XElement("RenterAndAuto", request.Element("ClickThruPartnerInfo").Element("RenterAndAuto").Value));
                    else
                        quote.AddInfo.Element("ClickThruPartnerInfo").Element("RenterAndAuto").Value = request.Element("ClickThruPartnerInfo").Element("RenterAndAuto").Value;
                }
            }
			
			quote.AddInfo.Element("SystemDate").Value = DateTime.Now.Month.ToString("00") + "/" + DateTime.Now.Day.ToString("00") + "/" + DateTime.Now.Year.ToString("0000");
            SetRiskState(quote);
            SetSplitZip(quote);

            SetProductInfo(quote);

            //if (quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID").Value != null)
            //{   //this need to reset keycode and account no I90344 and 76900
            //    if (quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID").Value == "10452")
            //        isRecall = false;
            //}
            
            ResetQuoteEffectiveDates(quote);
            SetSalesPhoneAndHours(quote);
            SetCTInfoByMarketKey(quote);
            
            SetCTInfoByMarketKey(quote);

            //SetAffinityDetails(quote);
            //SetBillingFlag(quote);
            SetDiscounts(quote);
            ////SSR08086 udiaes 12/2/2011
            //SetExtSvcInfo(quote);

            //if (newQuote)
            //{
            //    //SetDefaultPayPlan(quote);
            //}

            //if (request.Element("Solicitation") != null && request.Element("Solicitation").Element("ID") != null && request.Element("Solicitation").Element("ID").Value != String.Empty)
            //{
            //    SetSolicitation(quote, request.Element("Solicitation"));
            //}

            

            ////tc #6823 09-17-2010
            ////tc #6716 12-06-2010 - Cycle
            //if (request.Element("Referral") != null)
            //{
            //    if ((request.Element("Referral").Element("Referrer") != null && request.Element("Referral").Element("Referrer").Value != String.Empty) || (request.Element("Referral").Element("ReferrerQuoteNo") != null && request.Element("Referral").Element("ReferrerQuoteNo").Value != String.Empty))
            //    {
            //        SetReferral(quote, request.Element("Referral"));
            //    }
            //}
          
           
            response.Add(quote.Serialize());

            //off from udlfex, quoeflow also load again 
            if (quote.AddInfo.Element("RiskState").Value != "" )
            {                
                response.Add(questions.Load(quote.AddInfo.Element("RiskState").Value));

                //ysang TST09673 for 7123 3/25/2011    
               // response.Add(discounts.Load(quote.AddInfo.Element("RiskState").Value));
               
                XElement elmdoc = discounts.Load(quote.AddInfo.Element("RiskState").Value);
                ////APPLOG.Error("QuoteFlowPlugin", "sessionserivce.Load :", "load discount", new Exception(elmdoc.ToString()));                            
                if (quote.AddInfo.Element("ClickThruPartnerInfo").Element("RenterAndAuto") != null && quote.AddInfo.Element("ClickThruPartnerInfo").Element("RenterAndAuto").Value == "YES")
                {
                    string sElm = elmdoc.ToString().Replace("{multi_policy_discount_display}", "By quoting renters with auto insurance, we have already included the Multi-Policy Discount in your auto quote."); ;
                    //quote.AddInfo.Element(new XElement("Guid", quote.AddInfo.Element("Guid").Value));

                    XElement disc = XElement.Parse(sElm);
                    response.Add(disc);
                }
                    //ys PRD21783 7/17/2012 for cross sell
                //else if ((quote.AddInfo.Element("ClickThruPartnerInfo").Element("HOPolicy") != null && quote.AddInfo.Element("ClickThruPartnerInfo").Element("HOPolicy").Value.Length>0)
                //else if (isCrossSell)
                //{
                //    if ((request.Element("HOPolicy").Element("Policy") != null && request.Element("HOPolicy").Element("Policy").Value.Length > 0)
                //    || (quote.AddInfo.Element("HOPolicy").Element("Policy") != null && quote.AddInfo.Element("HOPolicy").Element("Policy").Value.Length > 0)
                //        || (request.Element("HOPolicy").Element("Quote") != null && request.Element("HOPolicy").Element("Quote").Value.Length > 0))                       
                //    {
                //        string sElm = elmdoc.ToString().Replace("{multi_policy_discount_display}", lookup.BuildMPDMessage(quote));
                //        //APPLOG.Error("QuoteFlowPlugin", "sessionserivce.Load :", "load discount" , new Exception(sElm));
                //        XElement disc = XElement.Parse(sElm);
                //        response.Add(disc);
                //    }
                //}

                else
                {

                    response.Add(elmdoc);
                }
                response.Add(accidentViolations.Load(quote.AddInfo.Element("RiskState").Value));
            }
            //tc #7516 01-31-2011 - Stock information
            //response.Add(ticker.Load());

            //// fcaglar SSR07488 - Add new groups to CA Website(s) 06/01/2011
            //if (quote.AddInfo.Element("RiskState").Value == "CA")
            //{
            //    response.Add(lookup.GetGroups(quote.AddInfo.Element("RiskState").Value, quote));
            //}
            return response;
        }
        //ssr7409,11/16/2011 wsun refactor 
        private void SetNewQuoteRequestValue(Quote quote,XElement request)
        {
            quote.AddInfo.Element("Guid").Value = Guid.NewGuid().ToString("D").ToUpper();
            //ysang 1/3/2011 prd14929
            if (request.Element("ZipCode") != null && request.Element("ZipCode").Value != "")
            {
                quote.AiisQuoteMaster.getCustomer().setZipCode1(Int32.Parse(request.Element("ZipCode").Value));
            }
            if (request.Element("ClickThruPartnerInfo") != null)
            {
                quote.AddInfo.Element("ClickThruPartnerInfo").ReplaceWith(request.Element("ClickThruPartnerInfo"));
            }
        }

        public XElement Save(XElement request)
        {
            Quote quote = new Quote();
            XElement response = new XElement("Response");

            quote.Deserialize(request.Element("DRCXML"));
            quote.Save();

            response.Add(new XElement("Guid", quote.AddInfo.Element("Guid").Value));

            return response;
        }
        
        public void SetDefaults(Quote quote)
        {
            DateTime date = DateTime.Now;

            //fcaglar SSR07102 02-10-2011 - CA new quote flow
            //This is run only for initial load. SetRiskState already sets the state name
            //however, it is called after this method is called.
            string state = string.Empty;
            bool isVibleState = true;
            //SSR6873 WLU 6/5/2012
            bool isVibe6MOState = false;
            if (quote.AddInfo.Element("RiskState").Value == String.Empty)
            {
                Int32 zip = (Int32)quote.AiisQuoteMaster.getCustomer().getZipCode1();
                using (var context = new AutoQuoteEntitie7())
                {
                    var states = from z in context.tbl_web_state_zip_ranges
                                     join s in context.states_master
                                        on z.state equals s.state_master
                                     where z.min_zip <= zip
                                     && z.max_zip >= zip
                                     select s;

                    if (states.Count() > 0)
                    {
                        state = states.First().state_master;
                        isVibleState = states.First().is_vibe_state;
                        isVibe6MOState = states.First().is_vibe_6mo_state;
                    }
                }
            }

            
            quote.AiisQuoteMaster.getQuoteInfo().setQuotePrintTest(2);
            quote.AiisQuoteMaster.getQuoteInfo().setResponseNo(2);
            quote.AiisQuoteMaster.getQuoteInfo().setQuoteTransType(0);
            quote.AiisQuoteMaster.getQuoteInfo().setOrigQuoteDate((DateTime)date);
            quote.AiisQuoteMaster.getQuoteInfo().setPolicyEffDate(((DateTime)date).AddDays(1));

            quote.AiisQuoteMaster.getCustomer().setMarketSourceAdq(1);
            
            if (isVibleState)
            {
                quote.AiisQuoteMaster.getCustomer().setProductVersion(4);
                quote.AiisQuoteMaster.getCustomer().setMarketSourceAdq(1);
                quote.AiisQuoteMaster.getCustomer().setSpecialCorresNo(7);
            }
            else
            {
                quote.AiisQuoteMaster.getCustomer().setProductVersion(2);
                quote.AiisQuoteMaster.getCustomer().setSpecialCorresNo(0);
            }

            quote.AiisQuoteMaster.getCustomer().setContactTypeNo(4);

            quote.AiisQuoteMaster.getPolicyInfo().setUserIdNo(888);
            quote.AiisQuoteMaster.getPolicyInfo().setLocationNo(88);
            quote.AiisQuoteMaster.getPolicyInfo().setChannelMethod(1);

            quote.AiisQuoteMaster.getPolicyInfo().setVersionDate((DateTime)date);
            quote.AiisQuoteMaster.getPolicyInfo().setQuoteEffDate(((DateTime)date).AddDays(1));
            quote.AiisQuoteMaster.getPolicyInfo().setIssueDate((DateTime)date);

            //PRD13423 wsun 8/25/2010, policyInfo.ipEffDate should be same as systemdate
            quote.AiisQuoteMaster.getPolicyInfo().setEffDate((DateTime)date);

            quote.AiisQuoteMaster.getPolicyInfo().setExpDate(((DateTime)date).AddDays(1));
            quote.AiisQuoteMaster.getPolicyInfo().setMethodCvForms(1);

            //fcaglar SSR07102 02-10-2011 - CA new quote flow
            //if (state == "CA")
            if (!isVibleState)
            {
                quote.AiisQuoteMaster.getPolicyInfo().setTermFactor(0.5);
                quote.AiisQuoteMaster.getPolicyInfo().setPrefPayLevel(0);
            }
            else
            {
                //SSR6873 WLU 6/5/2012
                if (isVibe6MOState)
                    quote.AiisQuoteMaster.getPolicyInfo().setTermFactor(0.5);
                else
                    quote.AiisQuoteMaster.getPolicyInfo().setTermFactor(1);
                quote.AiisQuoteMaster.getPolicyInfo().setPrefPayLevel(7);
            }

            quote.AiisQuoteMaster.getPolicyInfo().setRateAdjTerm(12);
            quote.AiisQuoteMaster.getPolicyInfo().setProductCode(1);
        }

        public bool IsValidZip(int zipcode)
        {
            using (var context = new AutoQuoteEntitie7())
            {
                var states = from z in context.tbl_web_state_zip_ranges
                             join s in context.states_master
                                on z.state equals s.state_master
                             where z.min_zip <= zipcode
                             && z.max_zip >= zipcode
                             select s;

                if (states.Count() > 0)
                    return true;
            }
            return false;
        }
        public void SetRiskState(Quote quote)
        {
            if (quote.AddInfo.Element("RiskState").Value == String.Empty)
            {
                if (quote.AiisQuoteMaster.getCustomer().getRiskState() > 0)
                {
                    quote.AddInfo.Element("RiskState").Value = ((StatesEnum)quote.AiisQuoteMaster.getCustomer().getRiskState()).ToString();
                }
                else if (quote.AiisQuoteMaster.getCustomer().getAddressStateCode() != String.Empty)
                {
                    quote.AddInfo.Element("RiskState").Value = quote.AiisQuoteMaster.getCustomer().getAddressStateCode().ToUpper();
                }
                else
                {
                    Int32 zip = (Int32)quote.AiisQuoteMaster.getCustomer().getZipCode1();

                    using (var context = new AutoQuoteEntitie7())
                    {
                        var states = from z in context.tbl_web_state_zip_ranges
                                     join s in context.states_master
                                        on z.state equals s.state_master
                                     where z.min_zip <= zip
                                     && z.max_zip >= zip
                                     select s;

                        if (states.Count() > 0)
                        {
                            string state = states.First().state_master;
                            int number = (int)states.First().state_number;
                            string redirect = states.First().quote_redirect;

                            if (!String.IsNullOrEmpty(redirect))
                            {
                                //ys SSR08401  4/2/2012 append to crredir.asp ?State=" & sStateCode & "&Zip=" & sZipCode
                                if (redirect.IndexOf("cmredir") > 0) //if ../csrrefer.asp?state=FL
                                {
                                    quote.AddInfo.Element("CurrentPage").Value = redirect + "?State=" + state + "&Zip=" + zip;
                                }
                            }
                            else
                            {
                                quote.AddInfo.Element("CurrentPage").Value = "Vehicles";
                            }

                            quote.AddInfo.Element("RiskState").Value = state;
                            quote.AiisQuoteMaster.getCustomer().setAddressStateCode(state);
                            quote.AiisQuoteMaster.getCustomer().setMasterStateNo(number);
                        }
                        else
                        {
                            quote.AddInfo.Element("CurrentPage").Value = "";
                        }
                    }
                }
            }
        }

        public void SetSplitZip(Quote quote)
        {
            String zip = quote.AiisQuoteMaster.getCustomer().getZipCode1().ToString("00000");
            String key = "SplitZip_" + zip;

            using (var context = new AutoQuoteEntitie7())
            {
                var zips = from z in context.tbl_web_split_zips
                           where z.zip.Equals(zip)
                           orderby z.definition
                           select z;

                if (quote.AddInfo.Element("SplitZip") != null)
                {
                    quote.AddInfo.Element("SplitZip").Remove();
                }

                quote.AddInfo.Add(new XElement("SplitZip", new XAttribute("zip", zip)));

                foreach (var splitzip in zips)
                {
                    XElement city = new XElement("City");
                    city.Add(new XAttribute("territory", splitzip.terr));
                    city.Add(splitzip.definition);
                    quote.AddInfo.Element("SplitZip").Add(city);
                }
            }
        }

        public void SetProductInfo(Quote quote)
        {
            if (quote.AddInfo.Element("RiskState").Value == "CA")
            {
                quote.AiisQuoteMaster.getCustomer().setProductVersion(2);
                quote.AiisQuoteMaster.getCustomer().setSpecialCorresNo(0);
                quote.AiisQuoteMaster.getPolicyInfo().setTermFactor(0.5);
                quote.AiisQuoteMaster.getPolicyInfo().setPrefPayLevel(0);
            }
            else if (quote.AddInfo.Element("RiskState").Value == "ID" || quote.AddInfo.Element("RiskState").Value == "LA" || quote.AddInfo.Element("RiskState").Value == "UT")
                quote.AiisQuoteMaster.getPolicyInfo().setTermFactor(0.5);
        }

        public void ResetQuoteEffectiveDates(Quote quote)
        {
            DateTime date = DateTime.Now;
            //String dateKey = "LookupDefaults_DRCDate";
            //DateTime? date = (DateTime?)CacheManager.GetData(dateKey);
            //if (date == null)
            //{
            //    date = _lookupDAO.LookupDRCDate();
            //    CacheManager.Add(dateKey, date, CacheManager.ExpireEverySixtySecond);
            //}

            DateTime effectiveBegin = quote.AiisQuoteMaster.getPolicyInfo().getEffDate();
            DateTime effectiveEnd = quote.AiisQuoteMaster.getPolicyInfo().getExpDate();
            DateTime PolQtEffDt = quote.AiisQuoteMaster.getPolicyInfo().getQuoteEffDate();


            DateTime today = (DateTime)date;
            DateTime tomorrow = today.AddDays(1);
            //ysang  5/26/2011 tst10057 for ssr6845
            int status = quote.AiisQuoteMaster.getQuoteInfo().getQuoteTransType();
            if (status == 2 || quote.AiisQuoteMaster.getCustomer().getQuasiBindTest() != 0) // bound           
            {
                // Don't mess with the dates
                //CustomerNetworkPlugin.LogError.Write("QuoteFlowPlugin", "LookupServices.ResetQuoteEffectiveDates:", quote.AiisQuoteMaster.getQuoteInfo().getQuoteNo0() + ": bound quote ", 2);
                //jrenz SSR8391 3/19/2012
                string quoteNo = quote.AiisQuoteMaster.getQuoteInfo().getQuoteNo0();
                string guidString = "";
                if (quote.AddInfo != null)
                {
                    guidString = quote.AddInfo.Element("Guid").Value;
                }
                LogUtility.LogError(quote.AiisQuoteMaster.getQuoteInfo().getQuoteNo0() + ": bound quote ","AutoQuoteFlow", "BL.Sessionservices", "ResetQuoteEffectiveDates");

            }
            else
            {
                if (effectiveBegin < tomorrow)
                {
                    effectiveBegin = today;
                    effectiveEnd = tomorrow;
                    PolQtEffDt = tomorrow;
                }
                else
                {
                    effectiveEnd = effectiveBegin.AddDays(1);
                    PolQtEffDt = effectiveEnd;
                }
            }


            DateTime policyEffDt = quote.AiisQuoteMaster.getQuoteInfo().getPolicyEffDate();
            DateTime effBeginPlusADay = effectiveBegin.AddDays(1);

            if (policyEffDt < effBeginPlusADay)
            {
                quote.AiisQuoteMaster.getQuoteInfo().setPolicyEffDate(effBeginPlusADay); //effectiveBegin
            }


            quote.AiisQuoteMaster.getPolicyInfo().setEffDate(effectiveBegin);
            quote.AiisQuoteMaster.getPolicyInfo().setExpDate(effectiveEnd);
            quote.AiisQuoteMaster.getPolicyInfo().setQuoteEffDate(PolQtEffDt);

            // while we are at it - set the version date
            //MD: policy date to determine which version of ratemaker
            string state = quote.AiisQuoteMaster.getCustomer().getAddressStateCode();
            if (state == "MD")
            {
                quote.AiisQuoteMaster.getPolicyInfo().setVersionDate(PolQtEffDt);
            }
            else
            {
                quote.AiisQuoteMaster.getPolicyInfo().setVersionDate(today);
            }
            //I am not sure we need to se these???
            if (quote.AiisQuoteMaster.getQuoteInfo().getQuoteNo0() != "") //1/4/2012wsun ssr7409 for www, recall =true but no quote#
                quote.AiisQuoteMaster.getQuoteInfo().setQuoteTransType(1);
            else
                quote.AiisQuoteMaster.getQuoteInfo().setQuoteTransType(0);

            if (state == "CA")
            {
                quote.AiisQuoteMaster.getCustomer().setProductVersion(2);
            }
            else
            {
                quote.AiisQuoteMaster.getCustomer().setProductVersion(4);
            }
        }

        public void SetSalesPhoneAndHours(Quote quote)
        {
            String _salesPhone = "800.555.1313";
            quote.AddInfo.Element("ClickThruPartnerInfo").Element("SalesPhone").Value = _salesPhone;
            String sHours = "Monday-Friday 12:00 - 3:00";
            quote.AddInfo.Element("ClickThruPartnerInfo").Element("SalesHours").Value = sHours;
        }

        public void SetCTInfoByMarketKey(Quote quote)
        {
            //bunch of defaults for home edition.
            quote.AiisQuoteMaster.getPolicyInfo().setAmfAccountNo(1300);
            quote.AiisQuoteMaster.getCustomer().setMarketKeyCode("z90149");
            quote.AiisQuoteMaster.getPolicyInfo().setMarketBrand(0);
            quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID").Value = "10160";
            quote.AddInfo.Element("ClickThruPartnerInfo").Element("MarketKeyCode").Value = "z90149";
            quote.AddInfo.Element("ClickThruPartnerInfo").Element("SalesPhone").Value = "800-555-1313";
            quote.AddInfo.Element("ClickThruPartnerInfo").Element("SalesHours").Value = "Thursday-Friday 2:00-3:00pm";
            quote.AddInfo.Element("ClickThruPartnerInfo").Element("AMFAccountNumber").Value = "1300";
            quote.AddInfo.Element("ClickThruPartnerInfo").Element("Affinity").Element("IsAffinity").Value = "false";
            quote.AddInfo.Element("ClickThruPartnerInfo").Element("Affinity").Element("IsAgent").Value = "false";
            quote.AddInfo.Element("ClickThruPartnerInfo").Add(new XElement("LandingPage", "Home.aspx"));
            quote.AddInfo.Element("CurrentPage").Value = "Home.aspx";
            quote.AddInfo.Element("ClickThruPartnerInfo").Element("Affinity").Element("Logo").Value = "somepicture.jpg";
            quote.AddInfo.Element("ClickThruPartnerInfo").Element("Affinity").Element("Description").Value = "Hello world";
            quote.AddInfo.Element("KemperCorpUrl").Value = "www.kemper.com";
        }

        public void SetDefaultPayPlan(Quote quote)
        {
            quote.AddInfo.Element("UseDefaultPayPlan").Value = "true";

            //SSR7929 WLU Set default.
            if (quote.AddInfo.Element("PayRollDeductAcct") == null || quote.AddInfo.Element("PayRollDeductAcct").Value != "2")
            {
                quote.AiisQuoteMaster.getCustomer().setSpecialCorresNo(3);
            }
            else
            {   //udinzs 11/12/2010 SSR07257: VIBE Experimentation #1
                //fcaglar SSR07102 02-11-2011 - CA new quote flow
                if (quote.AddInfo.Element("RiskState").Value == "MN")
                {
                    quote.AiisQuoteMaster.getCustomer().setSpecialCorresNo(1);
                }
                else if (quote.AddInfo.Element("RiskState").Value == "CA")
                {
                    quote.AiisQuoteMaster.getCustomer().setSpecialCorresNo(0);
                }
                else
                {
                    quote.AiisQuoteMaster.getCustomer().setSpecialCorresNo(5);
                }
            }
        }
        public void SetDiscounts(Quote quote)
        {
            try
            {
                string state = quote.AiisQuoteMaster.getCustomer().getAddressStateCode();
                using (var context = new AutoQuoteEntitie7())
                {
                    
                    var discounts = from d in context.states_master
                                    where d.state_master.Equals(state)
                                        select d;
                    if (discounts.Count() == 1)
                    {
                        var dis = discounts.First();
                        Int32 esigDiscount = (Int32)dis.esig_discount;
                        Int32 webDiscount = dis.is_webDisc ? 1 : 0;
                        Int32 passiveRestraintDiscount = 1; //subsystems
                        Int32 instantRenters = dis.allow_ho_instant_renter ? 1 : 0;
                        Int32 embeddedRenters = dis.allow_affinity_embedded_renter ? 1 : 0;
                        Int32 homeownersDiscount = dis.is_homeownerDisc ? 1 : 0;
                        Int32 rentersDiscount = dis.allow_ho_instant_renter ? 1 : 0;
                        Int32 groupDiscount = 0;
                        //select top 1 gr_discount_level from dgrpdisc
                        //where gr_state = @state and gr_group = @group and gr_release_date > 0 and gr_new_eff_date_begin <= convert(char(8), @effdate, 112) and (gr_new_eff_date_end > convert(char(8), @effdate, 112) or gr_new_eff_date_end = 0) 

                        Int32 groupNumber = 0;
                        if (quote.AiisQuoteMaster.getPolicyInfo().getAffinityNo() > 0)
                        {
                            groupNumber = (Int32)quote.AiisQuoteMaster.getPolicyInfo().getAffinityNo();
                        }
                        else if (quote.AiisQuoteMaster.getPolicyInfo().getAssocNo() > 0)
                        {
                            groupNumber = (Int32)quote.AiisQuoteMaster.getPolicyInfo().getAssocNo();
                        }
                        else if (quote.AiisQuoteMaster.getPolicyInfo().getAlumniNo() > 0)
                        {
                            groupNumber = (Int32)quote.AiisQuoteMaster.getPolicyInfo().getAlumniNo();
                        }
                        if (quote.AddInfo.Element("Discounts") != null)
                        {
                            quote.AddInfo.Element("Discounts").Remove();
                        }

                        quote.AddInfo.Add(new XElement("Discounts"));
                        quote.AddInfo.Element("Discounts").Add(new XElement("Esignature", esigDiscount));
                        quote.AddInfo.Element("Discounts").Add(new XElement("Web", webDiscount));
                        quote.AddInfo.Element("Discounts").Add(new XElement("PassiveRestraint", passiveRestraintDiscount));
                        quote.AddInfo.Element("Discounts").Add(new XElement("Renters", rentersDiscount));
                        quote.AddInfo.Element("Discounts").Add(new XElement("Homeowners", homeownersDiscount));
                        quote.AddInfo.Element("Discounts").Add(new XElement("Group", groupDiscount));
                        //wsun 7409 returnandsave discount 11/10/2011
                        quote.AiisQuoteMaster.getPolicyInfo().setComeBackDis(0);
                        quote.AiisQuoteMaster.getPolicyInfo().setWelcomeBackDis(0);
                        if (quote.AddInfo.Element("ReturnDiscount").Value.ToLower().Equals("ccc"))
                        {
                            if (IsQualifyReturnAndSaveDiscount(quote, ReturnAndSaveDiscount.ComeBackandSave))
                            {
                                quote.AiisQuoteMaster.getPolicyInfo().setComeBackDis(1);
                                quote.AddInfo.Element("Discounts").Add(new XElement("ComeBackDis", 1));
                            }
                        }
                        if (quote.AddInfo.Element("ReturnDiscount").Value.ToLower().Equals("www"))
                        {
                            if (IsQualifyReturnAndSaveDiscount(quote, ReturnAndSaveDiscount.WelcomeBack))
                            {
                                quote.AiisQuoteMaster.getPolicyInfo().setWelcomeBackDis(1);
                                quote.AddInfo.Element("Discounts").Add(new XElement("WelcomeBackDis", 1));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError(ex.Message, "AutoQuoteFlow", "BL>Sessionservices","SetDiscounts");
            }
        }
        public bool IsQualifyReturnAndSaveDiscount(Quote quote, ReturnAndSaveDiscount discType)
        {
            //check state
            bool re = true;
            string state = quote.AiisQuoteMaster.getCustomer().getAddressStateCode();
            state = quote.AiisQuoteMaster.getCustomer().getAddressStateCode();
            if (ConfigurationManager.AppSettings["ExcludeReturnAndSaveState"].Contains(state))
            {
                re = false;
            }
            if (re && discType == ReturnAndSaveDiscount.ComeBackandSave)
            {//check quote date
                DateTime dComp = quote.AiisQuoteMaster.getQuoteInfo().getOrigCompleteDate();
                DateTime dQuote = quote.AiisQuoteMaster.getQuoteInfo().getOrigQuoteDate();
                DateTime lastQuoteDate = (dComp.CompareTo(dQuote) < 0) ? dComp : dQuote;
                int iComp = UDILibrary.BaseBusiness.SystemDate.AutoSystemDate.AddDays(-7).CompareTo(lastQuoteDate);
                if (iComp < 0)
                    re = false;
            }
            return re;
        }
    }
}
