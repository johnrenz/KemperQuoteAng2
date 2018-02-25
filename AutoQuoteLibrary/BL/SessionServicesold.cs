using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
//using QuoteFlowPlugin.BL;
using UDILibrary.Log;

namespace AutoQuoteLibrary.BL
{
    public class SessionServices : ISessionServices
    {
        public XElement Load(XElement request)
        {
            Quote quote = new Quote();
            Boolean newQuote = false;
            XElement response = new XElement("Response");
            LookupServices lookup = new LookupServices();
            QuestionServices questions = new QuestionServices();
            DiscountServices discounts = new DiscountServices();
            AccidentViolationServices accidentViolations = new AccidentViolationServices();
            //tc #7516 01-31-2011 - Stock information
            TickerServices ticker = new TickerServices();
            //ysang PRD16032 3/15/2011
            bool isRecall = false;
            //ysang TST12696 for 7537 1/13/2012
            string refQuote=""; 
            
            if (request.Element("Guid") != null && request.Element("Guid").Value != String.Empty)
            {
                
                using (var context = new AutoQuoteEntities4())
                {
                    var session = from s in context.tbl_web_session
                                where s.guid.ToString().Equals(request.Element("Guid").Value)
                                select s;
                    if (session.Count() == 1)
                    {
                        newQuote = false;
                        quote.Deserialize(XElement.Parse(session.First().drc_xml));
                    }
                    else
                    {
                        newQuote = true;
                        SetDefaults(quote);
                    }
                    
                }
                if (request.Element("ZipCode") != null && request.Element("ZipCode").Value != String.Empty)
                {
                    quote.AiisQuoteMaster.getCustomer().setZipCode1(Int32.Parse(request.Element("ZipCode").Value));
                }
                //ysang 7479 
                //tc #7479 12-21-2011 - Don't do this replace if there is no CTID
                if (quote.AiisQuoteMaster.getQuoteInfo().getQuoteNo0() == "" && request.Element("ClickThruPartnerInfo") != null &&
                    request.Element("ClickThruPartnerInfo").Element("CTID") != null &&
                    !String.IsNullOrEmpty(request.Element("ClickThruPartnerInfo").Element("CTID").Value))
                {//this is new
                    quote.AddInfo.Element("ClickThruPartnerInfo").ReplaceWith(request.Element("ClickThruPartnerInfo"));
                }
                //ysang TST12696 for 7537 1/13/2012
                else
                {
                    if (string.IsNullOrEmpty(request.Element("ClickThruPartnerInfo").Element("CTID").Value) && request.Element("Referral") != null)
                    {
                        if (request.Element("Referral").Element("ReferrerQuoteNo") != null && request.Element("Referral").Element("ReferrerQuoteNo").Value != String.Empty)
                            refQuote = request.Element("Referral").Element("ReferrerQuoteNo").Value;
                    }
                }
            }
            else
            {
                newQuote = true;
                //wsun ssr7409 
                SetNewQuoteRequestValue(quote,request);


                lookup.SetDefaults(quote);
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
			
          
			lookup.SetSystemDate(quote);
            lookup.SetRiskState(quote);
            //tc #7590 03-01-2011 - Split Zip Codes
            lookup.SetSplitZip(quote);

            ////ysang prd18899 10/31/2011 add this logic move to ud3plugin
            //lookup.SetComputeZip(quote);
            //fcaglar SSR07102 05-17-2011 - CA new quote flow
            lookup.SetProductInfo(quote);

            //ysang PRD16032 3/15/2011 we don't need to set ctid and change brand when the quote is complete and recall
            if (quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID").Value != null)
            {   //this need to reset keycode and account no I90344 and 76900
                if (quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID").Value == "10452")
                    isRecall = false;
            }
            //ysang 7123
            bool isCrossSell = false;           
            if (isRecall) //look for phone 
            {
                //ysang 4/5/2011 prd16168 we need to set up date when recall
                lookup.ResetQuoteEffectiveDates(quote);

                lookup.SetSalesPhoneAndHours(quote);
                //SSR7537 WLU 10/17/2011
                if (quote.AddInfo == null || quote.AddInfo.Element("ClickThruPartnerInfo") == null || quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID") == null ||
                    String.IsNullOrEmpty(quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID").Value))
                {
                    if (quote.AddInfo.Element("Application").Value.ToUpper().Trim() != "PORTAL")
                        lookup.SetCTInfoByMarketKey(quote);
                }
                //ysang PRD20353  3/7/2012 for edit driver/vehicle
                else if (quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID").Value != "")
                {
                    lookup.SetCTID(quote);
                }

                //udinzs PRD19103
                if (quote.AddInfo != null || quote.AddInfo.Element("ClickThruPartnerInfo") != null || quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID") != null ||
                    String.IsNullOrEmpty(quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID").Value) == false)
                {
                    if (quote.AddInfo.Element("Application").Value.ToUpper().Trim() == "PORTAL" && quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID").Value != "")
                    {
                        lookup.SetCTID(quote);
                    }
                }
                
            }
            else
            {
                if (request.Element("HOPolicy") != null)
                {
                    if (request.Element("HOPolicy").Element("Policy") != null && request.Element("HOPolicy").Element("Policy").Value != String.Empty 
                        //|| quote.AddInfo.Element("ClickThruPartnerInfo").Element("HOPolicy") != null && quote.AddInfo.Element("ClickThruPartnerInfo").Element("HOPolicy").Value != string.Empty)
                        || request.Element("HOPolicy").Element("Quote") != null && request.Element("HOPolicy").Element("Quote").Value != String.Empty)                        
                    {
                        isCrossSell = true;
                        //UDILibrary.Log.APPLOG.Log("udquoteflowplugin.load", request.ToString());
                    }
                    
                }
                //ysang ssr7537 from udpi 
                if (request.Element("ShareAndSave") != null && quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID") != null && quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID").Value == "")
                {
                    if (request.Element("ShareAndSave").Element("qreferrer") != null)
                    {
                        
                        if (request.Element("ShareAndSave").Element("qreferrer").Value != "")
                        {
                            lookup.SetCTInfoByMarketKey(quote, request.Element("ShareAndSave").Element("qreferrer").Value);

                        }
                        UDILibrary.Log.APPLOG.Log("QuoteFlowPlugin.load", "ShareAndSave 22", request.Element("ShareAndSave").Element("qreferrer").Value, LogSeverity.Warning);
                    }
                }
                else
                {
                    if (!isCrossSell && string.IsNullOrEmpty(refQuote))
                    {
                        lookup.SetCTID(quote);                       
                    }

                }
            }
            
            //ysang 7537 1/13/2012
            if (refQuote.Length > 0)
                lookup.SetCTInfoByMarketKey(quote, refQuote);

            //ysang 7123 cross sell 2/25/2011   
            if (isCrossSell && quote.AiisQuoteMaster.getQuoteInfo().getQuoteNo0().Length==0)
            {
                isCrossSell = lookup.SetPreFilleHoData(quote, request.Element("HOPolicy"), true);
                lookup.SetCTIDByMarketKey(quote);
            }

            //ysang PRD16032 3/15/2011 move neeta get portal Phone here            
            if (quote.AddInfo.Element("Application").Value.ToUpper().Trim() == "PORTAL")
            {
                if (quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID").Value != null)
                {
                    if (quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID").Value == "0" || quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID").Value.Trim().Length == 0)
                    {
                        lookup.SetPortalPhone(quote);
                    }
                }
                else
                {
                    lookup.SetPortalPhone(quote);
                }
            }
            lookup.SetAffinityDetails(quote);
            lookup.SetBillingFlag(quote);
            lookup.SetDiscounts(quote);
            //SSR08086 udiaes 12/2/2011
            lookup.SetExtSvcInfo(quote);

            if (newQuote)
            {
                lookup.SetDefaultPayPlan(quote);
            }

            if (request.Element("Solicitation") != null && request.Element("Solicitation").Element("ID") != null && request.Element("Solicitation").Element("ID").Value != String.Empty)
            {
                lookup.SetSolicitation(quote, request.Element("Solicitation"));
            }

            

            //tc #6823 09-17-2010
            //tc #6716 12-06-2010 - Cycle
            if (request.Element("Referral") != null)
            {
                if ((request.Element("Referral").Element("Referrer") != null && request.Element("Referral").Element("Referrer").Value != String.Empty) || (request.Element("Referral").Element("ReferrerQuoteNo") != null && request.Element("Referral").Element("ReferrerQuoteNo").Value != String.Empty))
                {
                    lookup.SetReferral(quote, request.Element("Referral"));
                }
            }
          
           
            response.Add(quote.Serialize());

            //off from udlfex, quoeflow also load again 
            if (quote.AddInfo.Element("RiskState").Value != "" )
            {                
                response.Add(questions.Load(quote.AddInfo.Element("RiskState").Value));

                //ysang TST09673 for 7123 3/25/2011    
               // response.Add(discounts.Load(quote.AddInfo.Element("RiskState").Value));
               
                XElement elmdoc = discounts.Load(quote.AddInfo.Element("RiskState").Value);
                //APPLOG.Error("QuoteFlowPlugin", "sessionserivce.Load :", "load discount", new Exception(elmdoc.ToString()));                            
                if (quote.AddInfo.Element("ClickThruPartnerInfo").Element("RenterAndAuto") != null && quote.AddInfo.Element("ClickThruPartnerInfo").Element("RenterAndAuto").Value == "YES")
                {
                    string sElm = elmdoc.ToString().Replace("{multi_policy_discount_display}", "By quoting renters with auto insurance, we have already included the Multi-Policy Discount in your auto quote."); ;
                    //quote.AddInfo.Element(new XElement("Guid", quote.AddInfo.Element("Guid").Value));

                    XElement disc = XElement.Parse(sElm);
                    response.Add(disc);
                }
                    //ys PRD21783 7/17/2012 for cross sell
                //else if ((quote.AddInfo.Element("ClickThruPartnerInfo").Element("HOPolicy") != null && quote.AddInfo.Element("ClickThruPartnerInfo").Element("HOPolicy").Value.Length>0)
                else if (isCrossSell)
                {
                    if ((request.Element("HOPolicy").Element("Policy") != null && request.Element("HOPolicy").Element("Policy").Value.Length > 0)
                    || (quote.AddInfo.Element("HOPolicy").Element("Policy") != null && quote.AddInfo.Element("HOPolicy").Element("Policy").Value.Length > 0)
                        || (request.Element("HOPolicy").Element("Quote") != null && request.Element("HOPolicy").Element("Quote").Value.Length > 0))                       
                    {
                        string sElm = elmdoc.ToString().Replace("{multi_policy_discount_display}", lookup.BuildMPDMessage(quote));
                        //APPLOG.Error("QuoteFlowPlugin", "sessionserivce.Load :", "load discount" , new Exception(sElm));
                        XElement disc = XElement.Parse(sElm);
                        response.Add(disc);
                    }
                }

                else
                {

                    response.Add(elmdoc);
                }
                response.Add(accidentViolations.Load(quote.AddInfo.Element("RiskState").Value));
            }
            //tc #7516 01-31-2011 - Stock information
            response.Add(ticker.Load());

            // fcaglar SSR07488 - Add new groups to CA Website(s) 06/01/2011
            if (quote.AddInfo.Element("RiskState").Value == "CA")
            {
                response.Add(lookup.GetGroups(quote.AddInfo.Element("RiskState").Value, quote));
            }
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
        //ysang 7123 5/1/2011
        public XElement LoadQuoteWithHOPolicy(XElement request)
        {
            //CustomerNetworkPlugin.LogError.Write("QuoteFlowPlugin", "SessionServices.LoadQuoteWithHOPolicy 1", request.ToString(), 2);

            XElement response = new XElement("Response");
            Quote quote = new Quote();
           
          
            LookupServices lookup = new LookupServices();

        
            quote.AddInfo.Element("Guid").Value = Guid.NewGuid().ToString("D").ToUpper();
           
            if (request.Element("ZipCode") != null && request.Element("ZipCode").Value != "")
            {
                quote.AiisQuoteMaster.getCustomer().setZipCode1(Int32.Parse(request.Element("ZipCode").Value));
            }

            lookup.SetDefaults(quote);

            lookup.SetRiskState(quote);
            //ysang 7123 cross sell 2/25/2011   
           
             lookup.SetPreFilleHoData(quote, request.Element("HOPolicy"),false);
             lookup.SetCTIDByMarketKey(quote);


             if (quote.AddInfo.Element("CurrentPage") != null)
             {
                 quote.AddInfo.Element("CurrentPage").Value = "generalinfo_" + quote.AddInfo.Element("RiskState").Value;
             }
             else
             {
                 quote.AddInfo.Add(new XElement("CurrentPage", "generalinfo_" + quote.AddInfo.Element("RiskState").Value));
             }
             //quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID").Value
             if (request.Element("ClickThruPartnerInfo") != null && request.Element("ClickThruPartnerInfo").Element("CTID").Value !="")
             {
                 quote.AddInfo.Element("ClickThruPartnerInfo").Element("CTID").Value = request.Element("ClickThruPartnerInfo").Element("CTID").Value;
             }
            response.Add(quote.Serialize());

            string sDRCXML = response.ToString();

            XElement GuidElement = Save(XElement.Parse(sDRCXML));

           //string _guid = GuidElement.Value.ToString();

           return GuidElement;
        }

        //udinzs ssr 8284
        public XElement LoadForPhoneNum(XElement request)
        {
            XElement response = new XElement("Response");
            LookupServices lookup = new LookupServices();
            string phoneNum = "";
            if (request.Element("QuoteNo") != null && request.Element("QuoteNo").Value != String.Empty)
            {
                phoneNum = lookup.GetSalesPhoneByQuoteNo(request.Element("QuoteNo").Value); 
            }
            else if (request.Element("Guid") != null && request.Element("Guid").Value != String.Empty)
            {
                phoneNum = lookup.GetSalesPhoneByGUID(request.Element("Guid").Value);
            }
            response.Add (new XElement("SalesPhoneNum", phoneNum));
            return response;
        }

        public void SetDefaults(Quote quote)
        {
            String dateKey = "LookupDefaults_DRCDate";
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
                DataTable dt = _lookupDAO.LookupState(zip);
                if (dt.Rows.Count > 0)
                {
                    state = dt.Rows[0]["state"].ToString().ToUpper();
                    isVibleState = Convert.ToBoolean(dt.Rows[0]["is_vibe_state"]);
                    isVibe6MOState = Convert.ToBoolean(dt.Rows[0]["is_vibe_6mo_state"]);  //SSR6873 WLU 6/5/2012
                }
            }

            if (date == null)
            {
                date = _lookupDAO.LookupDRCDate();
                CacheManager.Add(dateKey, date, CacheManager.ExpireEverySixtySecond);
            }

            quote.AiisQuoteMaster.getQuoteInfo().setQuotePrintTest(2);
            quote.AiisQuoteMaster.getQuoteInfo().setResponseNo(2);
            quote.AiisQuoteMaster.getQuoteInfo().setQuoteTransType(0);
            quote.AiisQuoteMaster.getQuoteInfo().setOrigQuoteDate((DateTime)date);
            quote.AiisQuoteMaster.getQuoteInfo().setPolicyEffDate(((DateTime)date).AddDays(1));

            quote.AiisQuoteMaster.getCustomer().setMarketSourceAdq(1);
            //ysang 7584
            //fcaglar SSR07102 02-10-2011 - CA new quote flow
            //if (state == "CA")
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

    }
}
