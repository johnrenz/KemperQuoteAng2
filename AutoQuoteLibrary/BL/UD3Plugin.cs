using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Web;
using System.Web.UI.HtmlControls;
using AutoQuoteLibrary.AutoQuoteHelper;
using UDILibrary.UDIExtensions.XMLSerialization;

namespace AutoQuoteLibrary.BL
{
    public sealed class Constants
    {
        public static string COV_EDIT_FLAG = "CovEdit-Flag";
        public static string COV_EDIT_FATAL = "CovEdit-Fatal";
        public static string QUOTE_ERRORS = "Quote-Errors";
        public static string DNQ_QUOTE = "DNQ-Quote";
        public static string EMPTY_PREM = "     -   ";
        //tc #6773 08-18-2010 - Filters
        public static string FILTER_QUOTE = "FILTER-Quote";
        //ysang 6715 9/16/2010
		//dmetz 04-19-2012 SSR6873 - ID, LA, UT
        //public static string RENTER_MSG = "When you buy both auto and renters insurance from {CompanyName}, you can save even more.  You can add renters insurance with your auto policy to receive this discount on the next webpage";
        //public static string HOMEOWNER_MSG = "When you buy both auto and home or condo insurance from {CompanyName}, you can save even more.  You can add home or condo insurance with auto policy through our call center at";
		public static string RENTER_MSG = "Buy a Renters policy with your Auto and save on both!  Select your coverage and add to the right!";
		//dmetz 06-14-2012 PRD21420
		public static string HOMEOWNER_MSG = "Buy a Homeowners or Condo policy with your Auto and save on both!  Talk to one of our friendly agents at";
        
		//ysang 7123 2/28/2011
        //public static string HO_MSG = "Since you already have a {LOB} policy with us, we have included the multi-policy discount in your auto insurance quote.  You will also save an additional {$XXX} on your {LOB} policy when you buy an auto policy.";
        //public static string HO_MSG = "You will also get the Multi-Policy Discount and save even more on your {LOB} policy when you buy an auto policy.";
        public static string HO_MSG = "Since you already have a {LOB} policy with us, we have included the multi-policy discount in your auto insurance quote.  You will also get the Multi-Policy Discount and save even more on your {LOB} policy when you buy an auto policy.";
        public static string RENTAUTO_MSG = "By quoting renters with auto insurance, we have already included the Multi-Policy Discount in your auto quote.";
    }
    public class UD3Plugin
    {
        private Hashtable errorInfo = new Hashtable();
	    //jrenz SSR8391 3/26/2012
	    private string guid = "";
        private string flexSessionXML = "";

        private static readonly UD3Plugin _instance = new UD3Plugin();
        public static UD3Plugin Instance()
        {
            return _instance;
        }
        public UD3Plugin()
        {

        }

        public XElement RatedSaveWDiscounts(XElement requestXML)
        {
            XElement ret = null;

            //Save session
            XElement resp = SessionPlugin.Instance.Save(requestXML);

            XElement GuidElement = resp.Element("Guid");
            if (GuidElement == null || String.IsNullOrEmpty(GuidElement.Value))
                throw new Exception("UD3Plugin.RatedSaveWDiscounts: Fail to save session,guid-null");

            guid = GuidElement.Value.ToString();
            
            //Get DRC XML
            string drcXML = "";

            using (var context = new AutoQuoteEntitie7())
            {
                Guid gGuid = Guid.Empty;
                Guid.TryParse(guid, out gGuid);
                var drc = from d in context.tbl_web_session
                          where d.guid.Equals(gGuid)
                          select d;
                if (drc.Count() == 1)
                    drcXML = drc.First().drc_xml;

            }
            
            AutoQuote.Autoquote quote = new AutoQuote.Autoquote();
            quote.deserialize(drcXML, null);
            //home edition coverages
            SetDefaultCoverages(quote);

            resp = Rate(quote, drcXML);

            //If error
            if (resp.Name.LocalName.ToString() == "ErrorInfo")
            {
                LogUtility.LogError(resp.ToString() + "guid=" + guid, "AutoQuoteLibrary", "UD3Plugin", "RatedSaveWDiscounts");
                throw new Exception("Guid-[" + guid + "]" + resp.ToString());
            }
            else //If successful
            {
                string drcXML1 = GetXMLFromWebSession(guid);
                flexSessionXML = GetXMLFromWebSessionFlex(guid);
                
                //Read discounts
                XElement flexSession = XElement.Parse(flexSessionXML);
                XElement discountXML = flexSession.Element("Discounts");

                string xml = "<Response>" + drcXML1 + discountXML.ToString() + "</Response>";
                ret = XElement.Parse(xml);
            }

            return ret;
        }

        public XElement Rate(AutoQuote.Autoquote quote, string drcXML)
        {
            string state = quote.getCustomer().getAddressStateCode();
            string SalesPhoneNumber = "800-555-1212";
            DateTime effDt = quote.getPolicyInfo().getEffDate();
            XmlDocument xml;
            xml = new XmlDocument();
            AddInfo addInfo = new AddInfo();
            addInfo = XElement.Parse("<response>" + drcXML + "</response>").Element("DRCXML").Element("RETURN").Element("AiisQuoteMaster").Element("AddInfo").ToString().Deserialize<AddInfo>();
            guid = addInfo.Guid;
            
            bool getNonVibeMPD = false;
            double mpdPrem = 0;
            double nonMpdPrem = 0;
            if (addInfo.HOIRenterInfo != null)
                if (addInfo.HOIRenterInfo.HOIRenterProvide == HOIRenterInfo.EnumRenterProvide.Yes)
                    //rate twice to get savings for non vibe.
                    if (!Utilities.IsVibeState(quote.getCustomer().getAddressStateCode()))
                        getNonVibeMPD = true;

            if (getNonVibeMPD) //rate twice to get with and w/out MPD
            {
                quote.getPolicyInfo().setMultiPolicyTest(2);
                quote.getPolicyInfo().setHoProduct("2");
                simpleRate(quote);
                mpdPrem = quote.getPolicyInfo().getTotalPremium();
                quote.getPolicyInfo().setMultiPolicyTest(0);
                quote.getPolicyInfo().setHoProduct("");
                    
            }
            simpleRate(quote);
            if (getNonVibeMPD) //rate twice to get with and w/out MPD
            {
                nonMpdPrem = quote.getPolicyInfo().getTotalPremium();
                addInfo.multipolicydiscountNumeric = (decimal)(nonMpdPrem - mpdPrem);
                addInfo.multipolicydiscount = addInfo.multipolicydiscountNumeric.ToString("C");
            }

            //ysang 7123 2/28/2011 move get to here
            //string sHoRenterInfo = GetInstantRentersInfo(quote, drcXML, addInfo);

            if (quote.getQuoteInfo().getQuoteNo0() == "")
                quote.getQuoteInfo().setQuoteNo0(GetNewQuoteNo());

            //tc #6823 09-15-2010
            SaveQuote(quote);
            
            //Get Coverages XML
            string sCovXML = GetCoverageXML(quote, xml, state);

            Boolean Connected = false;
            String Referrer = "";

            //Connected = GetReferrerStatus(quote, ref Referrer);

            //Derive and format (translate) Discounts
            //ysang 7123 3/1/2011 ref addInfo
            String Discounts = GetDiscounts(ref quote, ref state, ref SalesPhoneNumber, false);

            double hoMpd = 0;
            string HOPolicy = ""; 
            //get rent and Auto flag
            string sRenterAuto = ""; // GetRenterAndAuto(addInfo);
            string sRedirect = ""; // GetRedirect(addInfo);
            //int clickThru_ID = 0;
            String GeneralInfo = GetGeneralInfo(ref guid, ref quote, ref state, ref SalesPhoneNumber, ref Connected, ref Referrer, HOPolicy, sRenterAuto, sRedirect);

            //Derive and format (translate) PayPlans
            String PayPlans = GetPayPlans(ref quote, ref drcXML);

            //Set MultiPolicy and Paperless based on prior selection
            //SetSelectedDiscounts(ref Discounts, ref addInfo);

            //Set PayPlan based on prior selection
            //SetSelectedPayPlan(ref PayPlans, ref addInfo);

            //Wrap the xml with Quote and add the guid
            //SSR06839 WLU
            //ysang 7123 2/28/2011 sHoRenterInfo
            //CoveragesXML = "<Quote><Guid>" + guid + "</Guid>" + sCovXML + GeneralInfo + Discounts + GetInstantRentersInfo(quote, drcXML, addInfo) + PayPlans + "</Quote>";
            string sInstantRenters = GetInstantRentersInfo(quote, drcXML);

            string CoveragesXML = "";
            CoveragesXML = "<Quote><Guid>" + guid + "</Guid>" + sCovXML + GeneralInfo + Discounts + sInstantRenters + PayPlans + "</Quote>";

            //*******************************************
            //Save to tbl_web_session
            //*******************************************
            //Add ErrorInfo to AddInfo ??? if error happened before, then it has saved to tbl_web_Session

            //Save session

            SaveDRCXMLtoWebSession(guid, addInfo, quote, state, "");
            SaveFlexXML2SessionFlex(guid, CoveragesXML);

            
            return XElement.Parse("<Quote><Guid>" + guid + "</Guid><QuoteNumber>" + quote.getQuoteInfo().getQuoteNo0().Trim() + "</QuoteNumber></Quote>", LoadOptions.None);
        }
        private void UpdateCoverages(WebSession session)
        {
            Coverage biCov = session.PolicyCoverages.Find(c => c.CovCode == "Bi");
            Coverage pdCov = session.PolicyCoverages.Find(c => c.CovCode == "Pd");
            Coverage medPayCov = session.PolicyCoverages.Find(c => c.CovCode == "MedPay");
            Coverage umbiCov = session.PolicyCoverages.Find(c => c.CovCode == "Umbi");
            Coverage umpdCov = session.PolicyCoverages.Find(c => c.CovCode == "Umpd");
            Coverage umpdDedCov = session.PolicyCoverages.Find(c => c.CovCode == "UmpdOption");
            AutoQuote.PolicyCoverage aqPolCov = session.Quote.getCoverages().item(0).getPolicyCoverage();
            if (biCov != null)
                if (biCov.SelectedLimit.Value.Contains("~"))
                {
                    long val = 0;
                    long val2 = 0;
                    long.TryParse(biCov.SelectedLimit.Value.Split('~')[0], out val);
                    long.TryParse(biCov.SelectedLimit.Value.Split('~')[1], out val2);
                    aqPolCov.setBiPerPerson100(val);
                    aqPolCov.setBiPerOcc100(val2);
                }
            if (pdCov != null)
            {
                long val = 0;
                long.TryParse(pdCov.SelectedLimit.Value, out val);
                aqPolCov.setPdSl100(val);
            }
            if (medPayCov != null)
            {
                long val = 0;
                long.TryParse(medPayCov.SelectedLimit.Value, out val);
                aqPolCov.setMedPay10(val);
            }
            
            AutoQuote.SixMonthPremiums aqSmPrems = session.Quote.getCoverages().item(0).getSixMonthPremiums();
            for (int i = 0; i < session.Quote.getVehicles().count(); i++)
            {
                Coverage compCov = session.PolicyCoverages.Find(c => c.CovCode == "CompDed");
                Coverage collCov = session.PolicyCoverages.Find(c => c.CovCode == "CollDed");
                Coverage towCov = session.PolicyCoverages.Find(c => c.CovCode == "Towing");

                AutoQuote.VehicleCoverage aqVehCov = session.Quote.getCoverages().item(0).getVehicleCoverages().item(i);
                if (umbiCov != null)
                    if (umbiCov.SelectedLimit.Value.Contains("~"))
                    {
                        long val = 0;
                        long val2 = 0;
                        long.TryParse(umbiCov.SelectedLimit.Value.Split('~')[0], out val);
                        long.TryParse(umbiCov.SelectedLimit.Value.Split('~')[1], out val2);
                        aqVehCov.setUmbiPerPerson100(val);
                        aqVehCov.setUmbiPerOcc100(val2);
                    }
                if (umpdCov != null)
                {
                    long val = 0;
                    long.TryParse(umpdCov.SelectedLimit.Value, out val);
                    aqVehCov.setUmPd100(val);
                }
                if (umpdDedCov != null)
                {
                    int val = 0;
                    int.TryParse(umpdDedCov.SelectedLimit.Value, out val);
                    aqVehCov.setUmPdDed(val);
                }
                if (compCov != null)
                {
                    int val = 0;
                    int.TryParse(compCov.SelectedLimit.Value, out val);
                    aqVehCov.setCompDed(val);
                }
                if (collCov != null)
                {
                    int val = 0;
                    int.TryParse(collCov.SelectedLimit.Value, out val);
                    aqVehCov.setCollDed(val);
                }
                if (towCov != null)
                {
                    int val = 0;
                    int.TryParse(towCov.SelectedLimit.Value, out val);
                    aqVehCov.setTowAndLaborPrem(val);
                }
            }
        }
        private void SetDefaultCoverages(AutoQuote.Autoquote theQuote)
        {
            theQuote.setCoverages(new AutoQuote.Coverages());
            theQuote.getCoverages().add();
            theQuote.getCoverages().item(0).setPolicyCoverage(new AutoQuote.PolicyCoverage());

            theQuote.getCoverages().item(0).setVehicleCoverages(new AutoQuote.VehicleCoverages());
            for (int i = 0; i < theQuote.getVehicles().count();i++)
                theQuote.getCoverages().item(0).getVehicleCoverages().add();

            AutoQuote.PolicyCoverage aqPolCov = theQuote.getCoverages().item(0).getPolicyCoverage();
            aqPolCov.setNoOfMinors(0);
            aqPolCov.setNoOfOtc(0);
            aqPolCov.setBiPerPerson100(250);
            aqPolCov.setPipFuneral10(0);
            aqPolCov.setHomeownershipType(0);
            aqPolCov.setStateOption5(0);
            aqPolCov.setInsolvencyFactor(0);
            aqPolCov.setTierLevelUw("C1");
            aqPolCov.setTierLevelCredit("X3");
            aqPolCov.setStateOption2(0);
            aqPolCov.setTierLevel(2);
            aqPolCov.setPipAgg100(0);
            aqPolCov.setPdSl100(250);
            aqPolCov.setUimType(0);
            aqPolCov.setAccDeathPrem(0);
            aqPolCov.setTierFactor(0);
            aqPolCov.setPrdPrem(0);
            aqPolCov.setExtraMed1000(0);
            aqPolCov.setDisappearDedCredit(0);
            aqPolCov.setStateOption6(0);
            aqPolCov.setTierScore(0);
            aqPolCov.setWorkLossTest(0);
            aqPolCov.setNoOfOmittedIncidents(0);
            aqPolCov.setPipWork10(0);
            aqPolCov.setPpiTest(0);
            aqPolCov.setExtLiabPrem(0);
            aqPolCov.setPipMed100(0);
            aqPolCov.setAutoDeathTest(0);
            aqPolCov.setAltQuoteTestCov(0);
            aqPolCov.setExtMedPrem(0);
            aqPolCov.setTortTest(0);
            aqPolCov.setStateOption3(0);
            AutoQuote.IpOptionalCovDates dates = new AutoQuote.IpOptionalCovDates();
            dates.setRateLockDate(DateTime.MinValue);
            dates.setAccForgiveDate(DateTime.MinValue);
            dates.setDisappearDedDate(DateTime.MinValue);
            aqPolCov.setIpOptionalCovDates(dates);
            aqPolCov.setBundle2Prem(0);
            aqPolCov.setLiabBuybackTest(0);
            aqPolCov.setNoOfNonchargeables(0);
            aqPolCov.setCcmcCovTest(0);
            aqPolCov.setStateOption4(0);
            aqPolCov.setBundle3Prem(0);
            aqPolCov.setBundle1Prem(0);
            aqPolCov.setMexicoCovTest(0);
            aqPolCov.setCcmcPrem(0);
            aqPolCov.setPipTest(0);
            aqPolCov.setPipSl1000(0);
            aqPolCov.setPipDed(0);
            aqPolCov.setBrodNoOfPersons(0);
            aqPolCov.setPipPerOcc1000(0);
            aqPolCov.setPipDeath10(0);
            aqPolCov.setStateOption1(0);
            aqPolCov.setLiabDed(0);
            aqPolCov.setObelTest(0);
            aqPolCov.setBiCsl100(0);
            aqPolCov.setNoOfChargeables(0);
            aqPolCov.setPipPerPerson1000(0);
            aqPolCov.setExtMed100(0);
            aqPolCov.setPipWorkDed(0);
            aqPolCov.setAccDeath1000(0);
            aqPolCov.setUmBiDed(0);
            aqPolCov.setUwCoScore(0);
            aqPolCov.setExtMedTest(0);
            aqPolCov.setMedPay10(0);
            aqPolCov.setNoOfVehiclesCov(0);
            aqPolCov.setTotalDisableTest(0);
            aqPolCov.setExtNamedOrRelative(0);
            aqPolCov.setMexicoPrem(0);
            aqPolCov.setUmType(0);
            aqPolCov.setBiPerOcc100(500);

            theQuote.getCoverages().item(0).setSixMonthPremiums(new AutoQuote.SixMonthPremiums());
            AutoQuote.SixMonthPremiums aqSmPrems = theQuote.getCoverages().item(0).getSixMonthPremiums();
            aqSmPrems.setSmTWorkLossBasePrem(0);
            aqSmPrems.setSmTAirBagPrem(0);
            aqSmPrems.setSmInsolvencyPrem(0);
            aqSmPrems.setSmTRetroLoyaltyPrem(0);
            aqSmPrems.setSmVariableExpensePrem(0);
            aqSmPrems.setSmTAwaySchoolPrem(0);
            aqSmPrems.setSmTTotalVehPrem(0);
            aqSmPrems.setSmTPdBasePrem(0);
            aqSmPrems.setSmAccDeathPrem(0);
            aqSmPrems.setSmTWelcomeBackPrem(0);
            aqSmPrems.setSmTPref4payPrem(0);
            aqSmPrems.setSmTPrefMonthlyBPrem(0);
            aqSmPrems.setSmTotalTortPrem(0);
            aqSmPrems.setSmExclDrivSurchPrem(0);
            aqSmPrems.setSmTBiBasePrem(0);
            aqSmPrems.setSmTAssurancePrem(0);
            aqSmPrems.setSmTMultiPolicyPrem(0);
            aqSmPrems.setSmTPetPrem(0);
            aqSmPrems.setSmCommuteSurchPrem(0);
            aqSmPrems.setSmTUimBiBasePrem(0);
            aqSmPrems.setSmTUimPdBasePrem(0);
            aqSmPrems.setSmTSoundReproPrem(0);
            aqSmPrems.setSmTBasePrem(0);
            aqSmPrems.setSmTRateLockPrem(0);
            aqSmPrems.setSmTPipPrem(0);
            aqSmPrems.setSmTCompBasePrem(0);
            aqSmPrems.setSmTRentalRePrem(0);
            aqSmPrems.setSmTPipBasePrem(0);
            aqSmPrems.setSmTotalPolPrem(0);
            aqSmPrems.setSmOtherPolPrem(0);
            aqSmPrems.setSmAccViolPrem(0);
            aqSmPrems.setSmExpenseLoadSurchPrem(0);
            aqSmPrems.setSmTGroupdiscPrem(0);
            aqSmPrems.setSmTUmBiBasePrem(0);
            aqSmPrems.setSmTUmPdBasePrem(0);
            aqSmPrems.setSmTPrefPayPrem(0);
            aqSmPrems.setSmTWorkLossPrem(0);
            aqSmPrems.setSmTPrefMonthlyAPrem(0);
            aqSmPrems.setSmTotalBasicPrem(0);
            aqSmPrems.setSmTPrefMonthlyCPrem(0);
            aqSmPrems.setSmTPaperlessPrem(0);
            aqSmPrems.setSmTDisappearDedPrem(0);
            aqSmPrems.setSmExtLiabPrem(0);
            aqSmPrems.setSmTDiscountPrem(0);

            for (int i = 0; i < theQuote.getVehicles().count(); i++)
            {

                AutoQuote.SixMonthPremium sm = aqSmPrems.add();
                sm.setSmRetiredDrivPrem(0);
                sm.setSmPrefPayrollPrem(0);
                sm.setSmTotalVehBasePrem(0);
                sm.setSmPref1payPrem(0);
                sm.setSmUmPdBasePrem(0);
                sm.setSmAutoLoanPrem(0);
                sm.setSmEftDiscPrem(0);
                sm.setSmAccForgivePrem(0);
                sm.setSmMedPayBasePrem(0);
                sm.setSmComeBackPrem(0);
                sm.setSmTripInterruptPrem(0);
                sm.setSmTotalVehNonPrem(0);
                sm.setSmWorkLossPrem(0);
                sm.setSmSoundReproPrem(0);
                sm.setSmCustomizedPrem(0);
                sm.setSmTotalTaxPrem(0);
                sm.setSmGoodDriverPrem(0);
                sm.setSmPaidInFullPrem(0);
                sm.setSmDisappearDedPrem(0);
                sm.setSmTotalVehBasicPrem(0);
                sm.setSmPpiPrem(0);
                sm.setSmPassiveRestraintPrem(0);
                sm.setSmGoodStudentPrem(0);
                sm.setSmBiBasePrem(0);
                sm.setSmPrefMonthlyBPrem(0);
                sm.setSmPref4payPrem(0);
                sm.setSmRenewalCreditPrem(0);
                sm.setSmMatureDrivPrem(0);
                sm.setSmAntiLockBrakePrem(0);
                sm.setSmTotalVehPrem(0);
                sm.setSmMuniTaxPrem(0);
                sm.setSmRentalPrem(0);
                sm.setSmUmPdPrem(0);
                sm.setSmRentalRePrem(0);
                sm.setSmCarSeatPrem(0);
                sm.setSmWecomeBackPrem(0);
                sm.setSmExtdMedPrem(0);
                sm.setSmUmBiBasePrem(0);
                sm.setSmBiPrem(328);
                sm.setSmTotalDisablePrem(0);
                sm.setSmUimBiPrem(0);
                sm.setSmMarriedPrem(0);
                sm.setSmLossOfUsePrem(0);
                sm.setSmCustCollPrem(0);
                sm.setSmMingleMatePrem(0);
                sm.setSmPipBasePrem(0);
                sm.setSmAssurancePrem(0);
                sm.setSmPrefPayPrem(0);
                sm.setSmPaperlessPrem(0);
                sm.setSmBasicPdSlPrem(0);
                sm.setSmLiabSubsidyFee(0);
                sm.setSmPdSlPrem(0);
                sm.setSmPrefMonthlyAPrem(0);
                sm.setSmPipPrem(0);
                sm.setSmTowAndLaborPrem(0);
                sm.setSmMedPayPrem(0);
                sm.setSmPref2payPrem(0);
                sm.setSmPipFuneralPrem(0);
                sm.setSmCollBasePrem(0);
                sm.setSmRespCollDedPrem(0);
                sm.setSmPetPrem(0);
                sm.setSmCompSubsidyFee(0);
                sm.setSmCollSubsidyFee(0);
                sm.setSmMuniTaxSurchargePrem(0);
                sm.setSmBasicPipPrem(0);
                sm.setSmMultiPolicyPrem(0);
                sm.setSmAutoLoanCompPrem(0);
                sm.setSmDisablingDevicePrem(0);
                sm.setSmRetroLoyaltyPrem(0);
                sm.setSmDependentProtPrem(0);
                sm.setSmUmBiPrem(0);
                sm.setSmMuniTaxDiff(0);
                sm.setSmAutoLoanCollPrem(0);
                sm.setSmContCoveragePrem(0);
                sm.setSmSoundRecTranPrem(0);
                sm.setSmCollPrem(0);
                sm.setSmCustCompPrem(0);
                sm.setSmVehMexicoCovPrem(0);
                sm.setSmPrefMonthlyCPrem(0);
                sm.setSmPdBasePrem(0);
                sm.setSmAutoDeathPrem(0);
                sm.setSmSafeVehPrem(0);
                sm.setSmDdcPrem(0);
                sm.setSmRateLockPrem(0);
                sm.setSmBasicBiPrem(0);
                sm.setSmRentalVehCollPrem(0);
                sm.setSmDrivTrainingPrem(0);
                sm.setSmAutoSurchargePrem(0);
                sm.setSmSafeSoundPrem(0);
                sm.setSmAddlPipPrem(0);
                sm.setSmRentalVehPrem(0);
                sm.setSmOtherVehPrem(0);
                sm.setSmAutoSurchargeDiff(0);
                sm.setSmCatClaimsPrem(0);
                sm.setSmMuniSurchargeDiff(0);
                sm.setSmPayrolldiscPrem(0);
                sm.setSmHomeownerPrem(0);
                sm.setSmCompPrem(0);
                sm.setSmUimBiBasePrem(0);
                sm.setSmCompBasePrem(0);
                sm.setSmAirBagPrem(0);
                sm.setSmMultiCarPrem(0);
                sm.setSmTotalVehNoTaxes(0);
                sm.setSmUimPdPrem(0);
                sm.setSmPrefBiWeeklyPrem(0);
                sm.setSmRentalVehCompPrem(0);
                sm.setSmUimPdBasePrem(0);
                sm.setSmObelPrem(0);
                sm.setSmAddlPipBasePrem(0);
                sm.setSmCovPropertyPrem(0);
            }

            aqSmPrems.setSmTDisablingDevicePrem(0);
            aqSmPrems.setSmTMingleMatePrem(0);
            aqSmPrems.setSmTBundle1Prem(0);
            aqSmPrems.setSmTPrefBiWeeklyPrem(0);
            aqSmPrems.setSmTAddlPipBasePrem(0);
            aqSmPrems.setSmTotalBasicTortPrem(0);
            aqSmPrems.setSmTDdcPrem(0);
            aqSmPrems.setSmTMultiCarPrem(0);
            aqSmPrems.setSmTAffinityPrem(0);
            aqSmPrems.setSmTGoodStudentPrem(0);
            aqSmPrems.setSmTDependentProtPrem(0);
            aqSmPrems.setSmTPref1payPrem(0);
            aqSmPrems.setSmTMedPayBasePrem(0);
            aqSmPrems.setSmTotalVehPremNoTaxes(0);
            aqSmPrems.setSmTBundle3Prem(0);
            aqSmPrems.setSmTPref2payPrem(0);
            if (theQuote.getCustomer().getRentOwnTest() == 1)
                aqSmPrems.setSmTHomeownerPrem(57);
            else
                aqSmPrems.setSmTHomeownerPrem(0);
            aqSmPrems.setSmTCollBasePrem(0);
            aqSmPrems.setSmTotalPremNoTaxes(0);
            aqSmPrems.setSmTBundle2Prem(0);
            aqSmPrems.setSmTComeBackPrem(0);
            aqSmPrems.setSmTMatureDrivPrem(0);
            aqSmPrems.setSmTAccForgivePrem(0);
            aqSmPrems.setSmTContCoveragePrem(0);
            aqSmPrems.setSmExtMedPrem(0);
            aqSmPrems.setSmTPdSlPrem(0);
            aqSmPrems.setSmTUimBiPrem(0);
            aqSmPrems.setSmHighPerfSurchPrem(0);
            aqSmPrems.setSmTBiPrem(0);
            aqSmPrems.setSmTMarriedPrem(0);
            aqSmPrems.setSmTTripInterruptPrem(0);
            aqSmPrems.setSmTCollPrem(0);
            aqSmPrems.setSmTUmBiPrem(0);
            aqSmPrems.setSmTTowAndLaborPrem(0);
            aqSmPrems.setSmTPassiveRestraintPrem(0);
            aqSmPrems.setSmCcmcPrem(0);
            aqSmPrems.setSmTCarSeatPrem(0);
            aqSmPrems.setSmMexicoPrem(0);
            aqSmPrems.setSmTMedPayPrem(0);
            aqSmPrems.setSmTPrefPayrollPrem(0);
            aqSmPrems.setSmTSafeSoundPrem(0);
            aqSmPrems.setSmTCompPrem(0);
            aqSmPrems.setSmTotalNonPrem(0);
            
            AutoQuote.Savings aqSavings = theQuote.getCoverages().item(0).getSavings();
            aqSavings.setHighPerfSurchPrem(0);
            aqSavings.setExclDrivSurchPrem(0);
            aqSavings.setAccViolPrem(0);
            aqSavings.setVariableExpensePrem(0);
            aqSavings.setExpenseLoadSurchPrem(0);
            aqSavings.setCommuteSurchPrem(0);
            for (int i = 0; i < theQuote.getVehicles().count(); i++)
            {
                AutoQuote.Saving sv = aqSavings.add();
                sv.setComeBackPrem(0);
                sv.setMultiCarPrem(0);
            }

            AutoQuote.VehicleCoverages aqVehCovs = theQuote.getCoverages().item(0).getVehicleCoverages();
            for (int i = 0; i < theQuote.getVehicles().count(); i++)
            {
                AutoQuote.VehicleCoverage aqVehCov = theQuote.getCoverages().item(0).getVehicleCoverages().item(i);
                aqVehCov.setCompPrem(0);
                aqVehCov.setCarSeatTest(0);
                aqVehCov.setBiBasePrem(0);
                aqVehCov.setCustCompPrem(0);
                aqVehCov.setUmBiPrem(0);
                aqVehCov.setUimPd100(0);
                aqVehCov.setUmbiSl100(0);
                aqVehCov.setVehMexicoCovTest(0);
                aqVehCov.setVehMexicoCovPrem(0);
                aqVehCov.setRespCollDedPrem(0);
                aqVehCov.setCollPrem(794);
                aqVehCov.setDependentProtTest(0);
                aqVehCov.setSoundReproPrem(0);
                aqVehCov.setSoundRecTranCost(0);
                aqVehCov.setCompBasePrem(0);
                aqVehCov.setTapeCoverageTest(0);
                aqVehCov.setExtdMedPrem(0);
                aqVehCov.setRentalVehCollPrem(0);
                aqVehCov.setLossOfUseTest(0);
                aqVehCov.setUmPd100(250);
                aqVehCov.setMhRentTest(0);
                aqVehCov.setRateLockPrem(0);
                aqVehCov.setAutoLoanCollPrem(0);
                aqVehCov.setRespCollDedTest(0);
                aqVehCov.setPlatinumDisplayPrem(0);
                aqVehCov.setTowAndLaborPrem(0);
                aqVehCov.setRateLockTest(0);
                aqVehCov.setAddlPipPrem(0);
                aqVehCov.setPipBasePrem(0);
                aqVehCov.setTowAndLabor(0);
                aqVehCov.setDependentProtPrem(0);
                aqVehCov.setVehSecIso("11");
                aqVehCov.setMedPayBasePrem(0);
                aqVehCov.setUimbiPerOcc100(0);
                aqVehCov.setAssuranceTest(0);
                aqVehCov.setElectEquipCost(0);
                aqVehCov.setDisappearDedTest(0);
                aqVehCov.setTotalVehPrem(0);
                aqVehCov.setUimbiPerPerson100(0);
                aqVehCov.setAutoDeathPrem(0);
                aqVehCov.setRentalExtdTest(0);
                aqVehCov.setPetTest(0);
                aqVehCov.setWaivedCollTest(0);
                aqVehCov.setUmbiCsl100(0);
                aqVehCov.setUimBiPrem(0);
                aqVehCov.setObelPrem(0);
                aqVehCov.setPetPrem(0);
                aqVehCov.setTripInterruptPrem(0);
                aqVehCov.setVehPriIso("1341");
                aqVehCov.setAccForgiveTest(0);
                aqVehCov.setUmBiBasePrem(0);
                aqVehCov.setCoveredPropDed(0);
                aqVehCov.setCatClaimsPrem(0);
                aqVehCov.setUimbiSl100(0);
                aqVehCov.setUmbiPerOcc100(500);
                aqVehCov.setLiabSubsidyFee(0);
                aqVehCov.setAutoLoanTest(0);
                aqVehCov.setRentalRePrem(0);
                aqVehCov.setSoundRecTranPrem(0);
                aqVehCov.setPpiPrem(0);
                aqVehCov.setUmPdPrem(0);
                aqVehCov.setCarSeatPrem(0);
                aqVehCov.setUimPdBasePrem(0);
                aqVehCov.setWorkLossPrem(0);
                aqVehCov.setCompSubsidyFee(0);
                aqVehCov.setWorkLossBasePrem(0);
                aqVehCov.setCollBasePrem(0);
                aqVehCov.setBrodCollTest(0);
                aqVehCov.setRentalPrem(0);
                aqVehCov.setCollDed(500);
                aqVehCov.setPdSlPrem(0);
                aqVehCov.setRentalVehCompPrem(0);
                aqVehCov.setUimbiCsl100(0);
                aqVehCov.setUimBiBasePrem(0);
                aqVehCov.setTotalDisablePrem(0);
                aqVehCov.setLossOfUsePrem(0);
                aqVehCov.setMedPayPrem(0);
                aqVehCov.setUmbiPerPerson100(250);
                aqVehCov.setCompDed(500);
                aqVehCov.setUmPdDed(250);
                aqVehCov.setPipPrem(0);
                aqVehCov.setRatedDriver(1);
                aqVehCov.setBiPrem(0);
                aqVehCov.setFsgTest(0);
                aqVehCov.setAutoLoanCompPrem(0);
                aqVehCov.setUimPdPrem(0);
                aqVehCov.setCollSubsidyFee(0);
                aqVehCov.setDisappearDedPrem(0);
                aqVehCov.setAddlPipBasePrem(0);
                aqVehCov.setAccForgivePrem(0);
                aqVehCov.setPdBasePrem(0);
                aqVehCov.setAssurancePrem(0);
                aqVehCov.setCustEquipTest(0);
                aqVehCov.setLimitedCollTest(0);
                aqVehCov.setUmPdBasePrem(0);
                aqVehCov.setTripInterruptTest(0);
                aqVehCov.setCovPropertyPrem(0);
                aqVehCov.setRentalVehTest(0);
                aqVehCov.setCustCollPrem(0);

            }
            if (AutoQuoteLibrary.AutoQuoteHelper.Utilities.IsVibeState(theQuote.getCustomer().getAddressStateCode()))
            {
                theQuote.getPolicyInfo().setPaperlessDis(1);
                theQuote.getPolicyInfo().setPrefPayLevel(1);
                theQuote.getCustomer().setSpecialCorresNo(3);
                for (int i = 0; i < theQuote.getVehicles().count(); i++)
                {
                    AutoQuote.Vehicle veh = theQuote.getVehicles().item(i);
                    veh.setWebData(1);
                } 
                for (int i = 0; i < theQuote.getDrivers().count(); i++)
                {
                    AutoQuote.Driver drv = theQuote.getDrivers().item(i);
                    drv.setDdcDiscount(1);
                    drv.setMatureDriverDis(1);
                }                
            }

            theQuote.getPolicyInfo().setTotalPremium(0);
        }

        private void simpleRate(AutoQuote.Autoquote theQuote)
        {
            double multiPolicyDiscountFacter = 1;
            if (theQuote.getPolicyInfo().getHoProduct() == "2")
                multiPolicyDiscountFacter = .95;

            AutoQuote.PolicyCoverage aqPolCov = theQuote.getCoverages().item(0).getPolicyCoverage();

            theQuote.getQuotePolicyControl().setRecalculateTest(1);
            theQuote.getQuotePolicyControl().setCalcTest(1);
            theQuote.getQuotePolicyControl().setRetierTest(1);

            if (theQuote.getQuoteInfo().getQuoteNo0() != "")
            {
                //Amend
                theQuote.getQuoteInfo().setQuoteTransType(1);
            }
            string state = theQuote.getCustomer().getAddressStateCode();

            if (theQuote.getPolicyInfo().getHoProduct() == "1") //home
                theQuote.getPolicyInfo().setMultiPolicyTest(1);
            else if (theQuote.getPolicyInfo().getHoProduct() == "2")
                theQuote.getPolicyInfo().setMultiPolicyTest(2);

            //quote.rate();
            AutoQuote.SixMonthPremiums aqSmPrems = theQuote.getCoverages().item(0).getSixMonthPremiums();
            aqSmPrems.setSmExpenseLoadSurchPrem(92);
            aqSmPrems.setSmTDiscountPrem(8);
            if (theQuote.getVehicles().count() > 1)
                aqSmPrems.setSmTMultiCarPrem(2);
            AutoQuote.Savings aqSavings = theQuote.getCoverages().item(0).getSavings();
            aqSavings.setExpenseLoadSurchPrem(184);
            for (int i = 0; i < aqSavings.count(); i++)
            {
                AutoQuote.Saving sv = aqSavings.item(i);
                if (theQuote.getVehicles().count() > 1) 
                    sv.setMultiCarPrem(1);
            }
            AutoQuote.VehicleCoverages aqVehCovs = theQuote.getCoverages().item(0).getVehicleCoverages();
            double totalVehPrem = 0;
            double totalPdPrem = 0;
            double totalMedPayPrem = 0;
            double totalBiPrem = 0;
            double totalUmpdPrem = 0;
            double totalUmbiPrem = 0;
            double totalCollPrem = 0;
            double totalCompPrem = 0;
            for (int i = 0; i < theQuote.getVehicles().count(); i++)
            {
                AutoQuote.Vehicle theVeh = theQuote.getVehicles().item(i);
                AutoQuote.VehicleCoverage theVehCov = theQuote.getCoverages().item(0).getVehicleCoverages().item(i);
                AutoQuote.SixMonthPremium sm = aqSmPrems.item(i);
                sm.setSmBiPrem(aqPolCov.getBiPerPerson100() * ((double)theVeh.getVehSymbolLiab() / 6) * 1 / (i + 1) * multiPolicyDiscountFacter);
                sm.setSmPdSlPrem((double)aqPolCov.getPdSl100() * ((double)theVeh.getVehSymbolLiab() / 7) * 1 / (i + 1) * multiPolicyDiscountFacter);
                sm.setSmMedPayPrem(aqPolCov.getMedPay10() * ((double)theVeh.getVehSymbolLiab() / 15) * 1 / (i + 1) * multiPolicyDiscountFacter);
                sm.setSmUmBiPrem(theVehCov.getUmbiPerOcc100() * ((double)theVeh.getVehSymbolLiab() / 20) * 1 / (i + 1) * multiPolicyDiscountFacter);
                sm.setSmUmPdPrem((double)theVehCov.getUmPd100() * ((double)theVeh.getVehSymbolLiab() / 10) * 1 / (i + 1) * multiPolicyDiscountFacter);
                sm.setSmCollPrem(theVehCov.getCollDed() * ((double)theVeh.getVehSymbolColl() / 100) * 1 / (i + 1) * multiPolicyDiscountFacter);
                sm.setSmCompPrem(theVehCov.getCompDed() * ((double)theVeh.getVehSymbolComp() / 100) * 1 / (i + 1) * multiPolicyDiscountFacter);
                sm.setSmTotalVehPrem(sm.getSmBiPrem() + sm.getSmPdSlPrem() + sm.getSmMedPayPrem() + sm.getSmUmBiPrem() + sm.getSmUmPdPrem() + sm.getSmCollPrem() + sm.getSmCompPrem());
                if (theQuote.getVehicles().count() > 1)
                    sm.setSmMultiCarPrem(1);
                theVehCov.setBiPrem(sm.getSmBiPrem());
                theVehCov.setPdSlPrem(sm.getSmPdSlPrem());
                theVehCov.setMedPayPrem(sm.getSmMedPayPrem());
                theVehCov.setUmBiPrem(sm.getSmUmBiPrem());
                theVehCov.setUmPdPrem(sm.getSmUmPdPrem());
                theVehCov.setCollPrem(sm.getSmCollPrem());
                theVehCov.setCompPrem(sm.getSmCompPrem());
                theVehCov.setTotalVehPrem(sm.getSmTotalVehPrem());
                totalVehPrem += theVehCov.getTotalVehPrem();
                totalPdPrem += theVehCov.getPdSlPrem();
                totalUmpdPrem += theVehCov.getUmPdPrem();
                totalBiPrem += theVehCov.getBiPrem();
                totalUmbiPrem += theVehCov.getUmBiPrem();
                totalCollPrem += theVehCov.getCollPrem();
                totalCompPrem += theVehCov.getCompPrem();
                totalMedPayPrem += theVehCov.getMedPayPrem();

            }
            theQuote.getPolicyInfo().setTotalPremium(totalVehPrem);
            theQuote.getPolicyInfo().setPaidInFullOptionPrem(totalVehPrem * .93);
            aqSmPrems.setSmTTotalVehPrem(totalVehPrem);
            aqSmPrems.setSmTotalPolPrem(totalVehPrem);
            aqSmPrems.setSmTPdSlPrem(totalPdPrem);
            aqSmPrems.setSmTUmPdBasePrem(totalUmpdPrem);
            aqSmPrems.setSmTBiPrem(totalBiPrem);
            aqSmPrems.setSmTCollPrem(totalCollPrem);
            aqSmPrems.setSmTUmBiPrem(totalUmbiPrem);
            aqSmPrems.setSmTCompPrem(totalCompPrem);
            aqSmPrems.setSmTMedPayPrem(totalMedPayPrem);
            aqSmPrems.setSmTMultiPolicyPrem(totalVehPrem * .05);
            
            theQuote.setDownPayOptions(new AutoQuote.DownPayOptions());
            AutoQuote.DownPayOptions dpos = theQuote.getDownPayOptions();
            AutoQuote.DownPayOption dpo = dpos.add();
            dpo.setDpoTotalPref(theQuote.getPolicyInfo().getPaidInFullOptionPrem());
            dpo.setDpoInstamt(0);
            dpo.setDpoBplanno("030093");
            dpo.setDpoInstfee(6);
            dpo.setDpoRdnamt(theQuote.getPolicyInfo().getPaidInFullOptionPrem());
            dpo.setDpoBplan("PAY IN FULL");
            dpo.setDpoPolicyfee(0);
            dpo.setDpoAnonins(0);
            dpo.setDpoAdnamt(0);
            dpo.setDpoRnonins(0);
            dpo.setDpoDnfee(0);

            AutoQuote.DownPayOption dpo2 = dpos.add();
            dpo2.setDpoTotalPref(totalVehPrem);
            dpo2.setDpoInstamt(totalVehPrem / 2);
            dpo2.setDpoBplanno("040093");
            dpo2.setDpoInstfee(6);
            dpo2.setDpoRdnamt(totalVehPrem / 2);
            dpo2.setDpoBplan("2 PAYMENTS");
            dpo2.setDpoPolicyfee(0);
            dpo2.setDpoAnonins(0);
            dpo2.setDpoAdnamt(0);
            dpo2.setDpoRnonins(0);
            dpo2.setDpoDnfee(0);

            AutoQuote.DownPayOption dpo3 = dpos.add();
            dpo3.setDpoTotalPref(totalVehPrem);
            dpo3.setDpoRdnamt(totalVehPrem / 2.5);
            dpo3.setDpoInstamt((totalVehPrem - (totalVehPrem / 4)) / 2);
            dpo3.setDpoBplanno("070093");
            dpo3.setDpoInstfee(6);
            dpo3.setDpoBplan("3 PAYMENTS");
            dpo3.setDpoPolicyfee(0);
            dpo3.setDpoAnonins(0);
            dpo3.setDpoAdnamt(0);
            dpo3.setDpoRnonins(0);
            dpo3.setDpoDnfee(0);

            AutoQuote.DownPayOption dpo4 = dpos.add();
            dpo4.setDpoTotalPref(totalVehPrem);
            dpo4.setDpoInstamt(totalVehPrem / 4);
            dpo4.setDpoBplanno("050093");
            dpo4.setDpoInstfee(6);
            dpo4.setDpoRdnamt(totalVehPrem / 4);
            dpo4.setDpoBplan("4 PAYMENTS");
            dpo4.setDpoPolicyfee(0);
            dpo4.setDpoAnonins(0);
            dpo4.setDpoAdnamt(0);
            dpo4.setDpoRnonins(0);
            dpo4.setDpoDnfee(0);

            AutoQuote.DownPayOption dpo5 = dpos.add();
            dpo5.setDpoTotalPref(totalVehPrem);
            dpo5.setDpoInstamt(totalVehPrem / 4);
            dpo5.setDpoRdnamt(totalVehPrem / 5);
            dpo5.setDpoBplanno("090393");
            dpo5.setDpoInstfee(6);
            dpo5.setDpoBplan("MONTHLY");
            dpo5.setDpoPolicyfee(0);
            dpo5.setDpoAnonins(0);
            dpo5.setDpoAdnamt(0);
            dpo5.setDpoRnonins(0);
            dpo5.setDpoDnfee(0);

            theQuote.getQuoteInfo().setCompleteCode(1); //incomplete

            if (AutoQuoteLibrary.AutoQuoteHelper.Utilities.IsVibeState(theQuote.getCustomer().getAddressStateCode()))
            {
                theQuote.getPolicyInfo().setPaperlessDis(1);
                aqSmPrems.setSmTPassiveRestraintPrem(0);
                aqSmPrems.setSmTPrefPayrollPrem(0);
                aqSmPrems.setSmTPref1payPrem(0);
                aqSmPrems.setSmTPref2payPrem(0);
                aqSmPrems.setSmTPref4payPrem(0);
                aqSmPrems.setSmTPrefMonthlyAPrem(0);
                aqSmPrems.setSmTPrefMonthlyBPrem(0);
                aqSmPrems.setSmTPrefMonthlyCPrem(0);
                aqSmPrems.setSmTPaperlessPrem(0);
                aqSmPrems.setSmTMingleMatePrem(0);

                aqPolCov.setBundle1Prem(77);
                aqPolCov.setBundle2Prem(216);
                aqPolCov.setBundle3Prem(89);
                aqSmPrems.setSmTBundle1Prem(77);
                aqSmPrems.setSmTBundle2Prem(216);
                aqSmPrems.setSmTBundle3Prem(89);
                
                for (int i = 0; i < theQuote.getVehicles().count(); i++)
                {
                    AutoQuote.SixMonthPremium sm = aqSmPrems.item(i);
                    AutoQuote.Vehicle veh = theQuote.getVehicles().item(i);
                    sm.setSmPaperlessPrem(61);
                    if (veh.getPassiveRestraint() == 2)
                    {
                        sm.setSmPassiveRestraintPrem(131);
                        aqSmPrems.setSmTPassiveRestraintPrem(aqSmPrems.getSmTPassiveRestraintPrem() + sm.getSmPassiveRestraintPrem());
                    }

                    sm.setSmPrefPayrollPrem(82);
                    aqSmPrems.setSmTPrefPayrollPrem(aqSmPrems.getSmTPrefPayrollPrem() + sm.getSmPrefPayrollPrem());
                    sm.setSmPref1payPrem(92);
                    aqSmPrems.setSmTPref1payPrem(aqSmPrems.getSmTPref1payPrem() + sm.getSmPref1payPrem());
                    sm.setSmPref2payPrem(102);
                    aqSmPrems.setSmTPref2payPrem(aqSmPrems.getSmTPref2payPrem() + sm.getSmPref2payPrem());
                    sm.setSmPref4payPrem(112);
                    aqSmPrems.setSmTPref4payPrem(aqSmPrems.getSmTPref4payPrem() + sm.getSmPref4payPrem());
                    sm.setSmPrefMonthlyAPrem(122);
                    aqSmPrems.setSmTPrefMonthlyAPrem(aqSmPrems.getSmTPrefMonthlyAPrem() + sm.getSmPrefMonthlyAPrem());
                    sm.setSmPrefMonthlyBPrem(132);
                    aqSmPrems.setSmTPrefMonthlyBPrem(aqSmPrems.getSmTPrefMonthlyBPrem() + sm.getSmPrefMonthlyBPrem());
                    sm.setSmPrefMonthlyCPrem(142);
                    aqSmPrems.setSmTPrefMonthlyCPrem(aqSmPrems.getSmTPrefMonthlyCPrem() + sm.getSmPrefMonthlyCPrem());
                    if (theQuote.getPolicyInfo().getPaperlessDis() > 0)
                    {
                        sm.setSmPaperlessPrem(0);
                        aqSmPrems.setSmTPaperlessPrem(aqSmPrems.getSmTPaperlessPrem() + sm.getSmPaperlessPrem());
                    }

                    sm.setSmMingleMatePrem(229);
                    aqSmPrems.setSmTMingleMatePrem(aqSmPrems.getSmTMingleMatePrem() + sm.getSmMingleMatePrem());
                }
                for (int i = 0; i < theQuote.getDrivers().count(); i++)
                {
                    AutoQuote.Driver drv = theQuote.getDrivers().item(i);
                    if (drv.getDdcDiscount() == 1)
                        aqSmPrems.setSmTDdcPrem(50);
                    if (drv.getMatureDriverDis() == 1)
                        aqSmPrems.setSmTMatureDrivPrem(39);
                }
            }
        }

        public string BuildErrorInfoTag(Hashtable errorInfo)
        {
            //TODO - How do we structure the ErrorInfo?  Are we going to use it??

            StringBuilder sb = new StringBuilder();
            sb.Append("<ErrorInfo>");
            foreach (string key in errorInfo.Keys)
            {
                sb.Append("<");
                sb.Append(key);
                sb.Append(">");
                sb.Append(errorInfo[key].ToString());
                sb.Append("</");
                sb.Append(key);
                sb.Append(">");
            }
            sb.Append("</ErrorInfo>");
            return sb.ToString();
        }
        public void SaveQuote(AutoQuote.Autoquote quote)
        {
            int iPolicyEffDate = 0;
            int.TryParse(quote.getQuoteInfo().getPolicyEffDate().Year.ToString("0000") + quote.getQuoteInfo().getPolicyEffDate().Month.ToString("00") + quote.getQuoteInfo().getPolicyEffDate().Day.ToString("00"), out iPolicyEffDate);
            int iPolicyExpDate = 0;
            int.TryParse(quote.getQuoteInfo().getPolicyExpDate().Year.ToString("0000") + quote.getQuoteInfo().getPolicyExpDate().Month.ToString("00") + quote.getQuoteInfo().getPolicyExpDate().Day.ToString("00"), out iPolicyExpDate);
            int iEffDate = 0;
            int.TryParse(quote.getPolicyInfo().getEffDate().Year.ToString("0000") + quote.getPolicyInfo().getEffDate().Month.ToString("00") + quote.getPolicyInfo().getEffDate().Day.ToString("00"), out iEffDate);
            int iExpDate = 0;
            int.TryParse(quote.getPolicyInfo().getExpDate().Year.ToString("0000") + quote.getPolicyInfo().getExpDate().Month.ToString("00") + quote.getPolicyInfo().getExpDate().Day.ToString("00"), out iExpDate);
            int iQuoteEffDate = 0;
            int.TryParse(quote.getPolicyInfo().getQuoteEffDate().Year.ToString("0000") + quote.getPolicyInfo().getQuoteEffDate().Month.ToString("00") + quote.getPolicyInfo().getQuoteEffDate().Day.ToString("00"), out iEffDate);
            int iVersionDate = 0;
            int.TryParse(quote.getPolicyInfo().getVersionDate().Year.ToString("0000") + quote.getPolicyInfo().getVersionDate().Month.ToString("00") + quote.getPolicyInfo().getVersionDate().Day.ToString("00"), out iEffDate);
            int iCreditScoreEffDate = 0;
            int.TryParse(quote.getPolicyInfo().getCreditScoreEffDate().Year.ToString("0000") + quote.getPolicyInfo().getCreditScoreEffDate().Month.ToString("00") + quote.getPolicyInfo().getCreditScoreEffDate().Day.ToString("00"), out iEffDate);
            int iIssueDate = 0;
            int.TryParse(quote.getPolicyInfo().getIssueDate().Year.ToString("0000") + quote.getPolicyInfo().getIssueDate().Month.ToString("00") + quote.getPolicyInfo().getIssueDate().Day.ToString("00"), out iEffDate);
            int iOrigQuoteDate = 0;
            int.TryParse(quote.getQuoteInfo().getOrigQuoteDate().Year.ToString("0000") + quote.getQuoteInfo().getOrigQuoteDate().Month.ToString("00") + quote.getQuoteInfo().getOrigQuoteDate().Day.ToString("00"), out iEffDate);
            int iOrigCompleteDate = 0;
            int.TryParse(quote.getQuoteInfo().getOrigCompleteDate().Year.ToString("0000") + quote.getQuoteInfo().getOrigCompleteDate().Month.ToString("00") + quote.getQuoteInfo().getOrigCompleteDate().Day.ToString("00"), out iEffDate);
            int iRateVersionDate = 0;
            int.TryParse(quote.getPolicyInfo().getRateVersionDate().Year.ToString("0000") + quote.getPolicyInfo().getRateVersionDate().Month.ToString("00") + quote.getPolicyInfo().getRateVersionDate().Day.ToString("00"), out iEffDate);
            int iRequoteDate = 0;
            int.TryParse(quote.getQuoteInfo().getRequoteDate().Year.ToString("0000") + quote.getQuoteInfo().getRequoteDate().Month.ToString("00") + quote.getQuoteInfo().getRequoteDate().Day.ToString("00"), out iEffDate);
            int iQuoteConvDate = 0;
            int.TryParse(quote.getQuoteInfo().getQuoteConvDate().Year.ToString("0000") + quote.getQuoteInfo().getQuoteConvDate().Month.ToString("00") + quote.getQuoteInfo().getQuoteConvDate().Day.ToString("00"), out iEffDate);
            int iRatemakerVersDate = 0;
            int.TryParse(quote.getPolicyInfo().getRatemakerVersDate().Year.ToString("0000") + quote.getPolicyInfo().getRatemakerVersDate().Month.ToString("00") + quote.getPolicyInfo().getRatemakerVersDate().Day.ToString("00"), out iEffDate);
                
            using (var context = new AutoQuoteEntitie7())
            {
                string quoteNo = quote.getQuoteInfo().getQuoteNo0();
                var existingQuote = from q in context.Quotes
                                     where q.QM_QUOTE_NO_0.Equals(quoteNo)
                                     select q;
                if (existingQuote.Count() == 0)
                {
                    context.Quotes.Add(new AutoQuoteLibrary.Quote
                    {
                        QM_QUOTE_NO_0 = quote.getQuoteInfo().getQuoteNo0(),
                        QM_FIRST_NAME_OF_CUSTOMER = quote.getCustomer().getFirstNameOfCustomer(),
                        QM_LAST_NAME_OF_CUSTOMER = quote.getCustomer().getLastNameOfCustomer(),
                        QM_MIDDLE_INITIAL = quote.getCustomer().getMiddleInitial(),
                        QM_ADDRESS_CITY = quote.getCustomer().getAddressCity(),
                        QM_ADDRESS_LINE_1 = quote.getCustomer().getAddressLine1(),
                        QM_ADDRESS_LINE_2 = quote.getCustomer().getAddressLine2(),
                        QM_ADDRESS_STATE_CODE = quote.getCustomer().getAddressStateCode(),
                        QM_ADDRESS_STATE_NO = (short)quote.getCustomer().getAddressStateNo(),
                        QM_MASTER_STATE_NO = (short)quote.getCustomer().getMasterStateNo(),
                        QM_ZIP_CODE_1 = (int)quote.getCustomer().getZipCode1(),
                        QM_ZIP_CODE_2 = (int)quote.getCustomer().getZipCode2(),
                        QM_ADDRESS_TERR = (short)quote.getCustomer().getAddressTerr(),
                        QM_ADDTL_CUSTOMER_TITLE = quote.getCustomer().getAddtlCustomerTitle(),
                        QM_ADDTL_FIRST_NAME_OF_CUST = quote.getCustomer().getAddtlFirstNameOfCust(),
                        QM_ADDTL_LAST_NAME_OF_CUST = quote.getCustomer().getAddtlLastNameOfCust(),
                        QM_ADDTL_MIDDLE_INITIAL = quote.getCustomer().getAddtlMiddleInitial(),
                        QM_ADDRESS_VERIFICATION_TEST = (short)quote.getCustomer().getAddressVerificationTest(),
                        QM_CUSTOMER_TITLE = quote.getCustomer().getCustomerTitle(),
                        QM_DAY_AREA_CODE = (short)quote.getCustomer().getDayAreaCode(),
                        QM_DAY_PHONE1 = (short)quote.getCustomer().getDayPhone1(),
                        QM_DAY_PHONE2 = (short)quote.getCustomer().getDayPhone2(),
                        QM_DAY_EXT = (short)quote.getCustomer().getDayExt(),
                        QM_DAY_PHONE_TYPE = (short)quote.getCustomer().getDayPhoneType(),
                        QM_EVE_AREA_CODE = (short)quote.getCustomer().getEveAreaCode(),
                        QM_EVE_PHONE1 = (short)quote.getCustomer().getEvePhone1(),
                        QM_EVE_PHONE2 = (short)quote.getCustomer().getEvePhone2(),
                        QM_EVE_EXT = (short)quote.getCustomer().getEveExt(),
                        QM_EVE_PHONE_TYPE = (short)quote.getCustomer().getEvePhoneType(),
                        QM_E_MAIL_ADDRESS = quote.getCustomer().getEMailAddress(),
                        QM_POLICY_EFF_DATE = iPolicyEffDate,
                        QM_POLICY_EXP_DATE = iPolicyExpDate,
                        QM_EFF_DATE = iEffDate,
                        QM_EXP_DATE = iExpDate,
                        QM_QUOTE_EFF_DATE = iQuoteEffDate,
                        QM_EMAIL_ADDRESS_TEST = (short)quote.getPolicyInfo().getEmailAddressTest(),
                        QM_RISK_ADDRESS_LINE_1 = quote.getCustomer().getRiskAddressLine1(),
                        QM_RISK_ADDRESS_LINE_2 = quote.getCustomer().getRiskAddressLine2(),
                        QM_RISK_ADDRESS_CITY = quote.getCustomer().getRiskAddressCity(),
                        QM_RISK_STATE = (short)quote.getCustomer().getRiskState(),
                        QM_RISK_ZIP_CODE_1 = (short)quote.getCustomer().getRiskZipCode1(),
                        QM_RISK_ZIP_CODE_2 = (short)quote.getCustomer().getRiskZipCode2(),
                        QM_RISK_TERR = (short)quote.getCustomer().getRiskTerr(),
                        qm_market_brand = (short)quote.getPolicyInfo().getMarketBrand(),
                        QM_MARKET_KEY_CODE = quote.getCustomer().getMarketKeyCode(),
                        QM_MARKET_SOURCE_ADQ = (short)quote.getCustomer().getMarketSourceAdq(),
                        QM_PRODUCT_CODE = (short)quote.getPolicyInfo().getProductCode(),
                        QM_VERSION_NO = (short)quote.getPolicyInfo().getVersionNo(),
                        QM_VERSION_DATE = iVersionDate,
                        QM_BILLING_METHOD = (short)quote.getPolicyInfo().getBillingMethod(),
                        QM_CREDIT_MODEL = quote.getCustomer().getCreditModel(),
                        QM_CREDIT_SCORE = quote.getCustomer().getCreditScore(),
                        QM_CREDIT_SCORE_EFF_DATE = iCreditScoreEffDate,
                        QM_CREDIT_SCORE_TYPE = (short)quote.getPolicyInfo().getCreditScoreType(),
                        QM_CREDIT_SOURCE = (short)quote.getPolicyInfo().getCreditSource(),
                        QM_CREDIT_VENDOR = (short)quote.getPolicyInfo().getCreditVendor(),
                        QM_DNQ_NO = (short)quote.getQuoteInfo().getDnqNo(),
                        QM_CURRENT_CARRIER_NO = (short)quote.getCustomer().getCurrentCarrierNo(),
                        QM_CURRENT_CARRIER_PREM = (short)quote.getPolicyInfo().getCurrentCarrierPrem(),
                        QM_CURRENT_CARRIER_TYPE = (short)quote.getCustomer().getCurrentCarrierType(),
                        QM_CURRENT_LIMITS = (short)quote.getCustomer().getCurrentLimits(),
                        QM_CURRENT_CARRIER_TEST = (short)quote.getPolicyInfo().getCurrentCarrierTest(),
                        QM_COMPLETE_CODE = (short)quote.getQuoteInfo().getCompleteCode(),
                        QM_AMF_ACCOUNT_NO = (int)quote.getPolicyInfo().getAmfAccountNo(),
                        QM_UNDERWRITING_CO_NO = (int)quote.getPolicyInfo().getUnderwritingCoNo(),
                        QM_TOTAL_PREMIUM = (int)quote.getPolicyInfo().getTotalPremium(),
                        QM_TERM_FACTOR = (int)quote.getPolicyInfo().getTermFactor(),
                        QM_NO_OF_DRIVERS = (short)quote.getDrivers().count(),
                        QM_NO_OF_VEHICLES = (short)quote.getVehicles().count(),
                        QM_NO_OF_VIOLATIONS = (short)quote.getViolations().count(),
                        QM_NO_OF_ACC_COMP = (short)quote.getAccidents().count(),
                        QM_NO_OF_ADD_3_YRS = (short)quote.getCustomer().getNoOfAdd3Yrs(),
                        QM_NO_OF_DAYS_LAPSED = (short)quote.getPolicyInfo().getNoOfDaysLapsed(),
                        QM_NO_OF_YOUTHFULS = (short)quote.getPolicyInfo().getNoOfYouthfuls(),
                        QM_MULTI_POLICY_TEST = (short)quote.getPolicyInfo().getMultiPolicyTest(),
                        QM_MULTI_STATE_TEST = (short)quote.getCustomer().getMultiStateTest(),
                        QM_MASTER_CO_NO = (short)quote.getCustomer().getMasterCoNo(),
                        QM_NO_OF_EMP_3_YRS = (short)quote.getCustomer().getNoOfEmp3Yrs(),
                        QM_ALT_QUOTE_TEST = (short)quote.getQuoteInfo().getAltQuoteTest(),
                        qm_channel_method = (short)quote.getPolicyInfo().getChannelMethod(),
                        QM_USER_ID_NO = (short)quote.getPolicyInfo().getUserIdNo(),
                        QM_LOCATION_NO = (short)quote.getPolicyInfo().getLocationNo(),
                        QM_ISSUE_DATE = iIssueDate,
                        QM_METHOD_CV_FORMS = (short)quote.getPolicyInfo().getMethodCvForms(),
                        QM_SPECIAL_CORRES_NO = (short)quote.getCustomer().getSpecialCorresNo(),
                        QM_PRODUCT_VERSION = (short)quote.getCustomer().getProductVersion(),
                        QM_QUOTE_PRINT_TEST = (short)quote.getQuoteInfo().getQuotePrintTest(),
                        QM_RESPONSE_NO = (short)quote.getQuoteInfo().getResponseNo(),
                        QM_QUOTE_TRANS_TYPE = (short)quote.getQuoteInfo().getQuoteTransType(),
                        QM_ORIG_QUOTE_DATE = iOrigQuoteDate,
                        QM_ORIG_COMPLETE_DATE = iOrigCompleteDate,
                        QM_RATE_ADJ_TERM = (short)quote.getPolicyInfo().getRateAdjTerm(),
                        QM_RATE_ADJ_FACTOR = (short)quote.getPolicyInfo().getRateAdjFactor(),
                        QM_RATE_CALC_TYPE = (short)quote.getPolicyInfo().getRateCalcType(),
                        QM_RATE_ADJ_FACTOR_INCRMNT = (short)quote.getPolicyInfo().getRateAdjFactorIncrmnt(),
                        QM_RATE_VERSION_DATE = iRateVersionDate,
                        QM_CONTACT_TYPE_NO = (short)quote.getCustomer().getContactTypeNo(),
                        QM_EFT_TEST = (short)quote.getPolicyInfo().getEftTest(),
                        QM_DEPT_NO = (short)quote.getPolicyInfo().getDeptNo(),
                        QM_ENDORSER_USER_ID_NO = (short)quote.getPolicyInfo().getEndorserUserIdNo(),
                        QM_DRIV_EXCLUSION_TEST = (short)quote.getCustomer().getDrivExclusionTest(),
                        QM_ERMF_FACTOR = (short)quote.getPolicyInfo().getErmfFactor(),
                        QM_CONVERTED_UW_COMPANY = (short)quote.getPolicyInfo().getUnderwritingCoNo(),
                        QM_ASSIST_SCORE = 0,
                        QM_DOCUMENT_NO = (short)quote.getPolicyInfo().getDocumentNo(),
                        QM_CUST_PROFILE_NO = quote.getCustomer().getCustProfileNo(),
                        QM_CONTACT_DATE_END = 0,
                        QM_CONTACT_DATE_START = 0,
                        QM_CONTACT_RECV_DATE = 0,
                        QM_CONTACT_TIME_END = 0,
                        QM_CONTACT_TIME_START = 0,
                        QM_EXCLUDE_AUTO_CALL = (short)quote.getCustomer().getExcludeAutoCall(),
                        QM_DNQ_BY_UW_CO_TEST = 0,
                        QM_HOME_OFFICE_NO = (short)quote.getPolicyInfo().getHomeOfficeNo(),
                        QM_HOMEOWNER_VERIFY_TEST = (short)quote.getCustomer().getHomeownerVerifyTest(),
                        QM_BEST_TIME_TO_CALL = 0,
                        qm_e_document_level = 0,
                        QM_LEVEL_I_NO = (short)quote.getPolicyInfo().getLevelIiNo(),
                        qm_level_i_comm_rate = (short)quote.getPolicyInfo().getLevelIiCommRate(),
                        qm_level_ii_comm_rate = (short)quote.getPolicyInfo().getLevelIiiCommRate(),
                        QM_LEVEL_III_NO = (short)quote.getPolicyInfo().getLevelIiiNo(),
                        qm_level_iii_comm_rate = (short)quote.getPolicyInfo().getLevelIiiCommRate(),
                        QM_LEVEL_II_NO = (short)quote.getPolicyInfo().getLevelIiNo(),
                        QM_NEXT_VEHICLE_NO = (short)quote.getPolicyInfo().getNextVehNo(),
                        QM_NEXT_DRIVER_NO = (short)quote.getPolicyInfo().getNextDriverNo(),
                        qm_no_of_hh_drivers = (short)quote.getPolicyInfo().getNoOfHhDrivers(),
                        QM_NO_OF_REQUOTES = (short)quote.getQuoteInfo().getNoOfRequotes(),
                        QM_MEMBER_NO = quote.getPolicyInfo().getMemberNo2(),
                        QM_OPTION_FEE_1 = (short)quote.getPolicyInfo().getOptionFee1(),
                        QM_OPTION_FEE_2 = (short)quote.getPolicyInfo().getOptionFee2(),
                        QM_OPTION_FEE_3 = (short)quote.getPolicyInfo().getOptionFee3(),
                        QM_OPTION_FEE_4 = (short)quote.getPolicyInfo().getOptionFee4(),
                        QM_OPTION_FEE_1_DIFF = (short)quote.getPolicyInfo().getOptionFee1Diff(),
                        QM_OPTION_FEE_2_DIFF = (short)quote.getPolicyInfo().getOptionFee2Diff(),
                        QM_OPTION_FEE_3_DIFF = (short)quote.getPolicyInfo().getOptionFee3Diff(),
                        QM_OPTION_FEE_4_DIFF = (short)quote.getPolicyInfo().getOptionFee4Diff(),
                        QM_MESSAGE_LINE1 = quote.getCustomer().getMessageLine1(),
                        QM_MESSAGE_LINE2 = quote.getCustomer().getMessageLine2(),
                        QM_OPTION_FEE1_TFA = (short)quote.getPolicyInfo().getOptionFee1Tfa(),
                        QM_OPTION_FEE2_TFA = (short)quote.getPolicyInfo().getOptionFee2Tfa(),
                        QM_OPTION_FEE3_TFA = (short)quote.getPolicyInfo().getOptionFee3Tfa(),
                        QM_OPTION_FEE4_TFA = (short)quote.getPolicyInfo().getOptionFee4Tfa(),
                        QM_ORIGIN_CODE_NO = 0,
                        QM_PAID_IN_FULL_TEST = (short)quote.getPolicyInfo().getPaidInFullTest(),
                        QM_PAID_IN_FULL_OPTION_PREM = (int)quote.getPolicyInfo().getPaidInFullOptionPrem(),
                        QM_POLICY_FEE = (short)quote.getPolicyInfo().getPolicyFee(),
                        qm_no_of_can_notice_12_month = (short)quote.getPolicyInfo().getNoOfCanNotice12Month(),
                        QM_PHONE_QUOTE_TEST = 0,
                        QM_MASTER_TERR_NO = (short)quote.getCustomer().getMasterTerrNo(),
                        qm_policy_form = (short)quote.getPolicyInfo().getPolicyForm(),
                        QM_QUOTE_TEST = (short)quote.getQuoteInfo().getQuoteTest(),
                        QM_REFERENCE_NO = quote.getCustomer().getReferenceNo(),
                        QM_RENT_OWN_TEST = (short)quote.getCustomer().getRentOwnTest(),
                        QM_REG_OWNER_TEST = (short)quote.getCustomer().getRegOwnerTest(),
                        QM_REGUARANTEE_QUOTE_TEST = (short)quote.getQuoteInfo().getReguaranteeQuoteTest(),
                        QM_REQUOTE_DATE = iRequoteDate,
                        QM_SOCIAL_SECURITY_NO = quote.getCustomer().getSocialSecurityNo(),
                        QM_SERVICING_OFFICE = 0,
                        QM_QUOTE_TAX = 0,
                        QM_QUASI_BIND_TEST = 0,
                        QM_QUOTE_CONV_DATE = iQuoteConvDate,
                        QM_SOLICIT_ID = quote.getCustomer().getSolicitId(),
                        QM_PREMIUM_TERM = (short)quote.getPolicyInfo().getPremiumTerm(),
                        QM_QUOTE_CANCEL_DATE = 0,
                        //qm_social_security_no_enc = (byte)0,
                        QM_WEB_DISCOUNT = 1,
                        QM_SR22_APPLIED_TEST = (short)quote.getCustomer().getSr22AppliedTest(),
                        QM_SR22_FILING_FEE = (short)quote.getPolicyInfo().getSr22FilingFee(),
                        QM_SUB_PRODUCT_CODE = 0,
                        QM_TELE_CONTACT_DATE = 0,
                        QM_RISK_STNO_LOCK_TEST = 0,
                        QM_RISK_ZIP_LOCK_TEST = 0,
                        QM_TIER_LOCK_TEST = (short)quote.getQuoteInfo().getTierLockTest(),
                        QM_STATE_CO_LOCK_TEST = 0,
                        QM_TORT_PREMIUM = 0,
                        QM_QUOTE_PRINT_POINTER = 0,
                        QM_POST_MARK_DATE = 0,
                        QM_PRIOR_CONT_INS_TEST = (short)quote.getPolicyInfo().getPriorContInsTest(),
                        QM_USE_NEW_BILLING_FLAG = (short)quote.getPolicyInfo().getUseNewBillingFlag(),
                        qm_ratemaker_vers_date = iRatemakerVersDate,
                        QM_ALTERNATE_CONTACT = "",
                        QM_ANNUAL_INCOME = (short)quote.getCustomer().getAnnualIncome(),
                        QM_ACCTNUM_LOCK_TEST = 0,
                        QM_LAST_REQUOTE_TRANS = 0,
                        QM_MAIL_ID = "",
                        QM_MAIL_STNO_LOCK_TEST = 0,
                        QM_MARKET_KEY_RESERVE = "",
                        QM_MAIL_ZIP_LOCK_TEST = 0,
                        QM_LL_3_APPLIED_TEST = 0,
                        QM_PHONE_EXTENSION = (short)quote.getCustomer().getPhoneExtension(),
                        QM_PREMIUM_FINANCE_TEST = 0,
                        QM_PREMIUM_FINANCE_CO_NO = 0,
                        QM_INSP_CONT_COV_SINCE_DATE = 0,
                        QM_INSP_PRI_INS_WAIVER_TEST = 0,
                        QM_LAST_ENDORSEMENT_DATE = 0,
                        QM_MASTER_TERR_ADJ_TYPE = 0,
                        QM_MASTER_TERR_ADQ_TYPE = 0,
                        QM_PERSONAL_MESS_NO = 0,
                        QM_POLICY_REFERENCE_NO = ""
                    });
                    context.SaveChanges();
                    int quotePrimeKey = (from q in context.Quotes
                                         where q.QM_QUOTE_NO_0.Equals(quoteNo)
                                         select q).First().PRIME_KEY;
                    for (int i = 0; i < quote.getVehicles().count(); i++)
                    {
                        context.QuoteVehicles.Add(new QuoteVehicle
                        {
                            PRIME_KEY_V = quotePrimeKey,
                            QM_VEH_YEAR = (short)quote.getVehicles().item(i).getVehYear(),
                            QM_VEH_MAKE = quote.getVehicles().item(i).getVehMake().Length > 10 ? quote.getVehicles().item(i).getVehMake().Substring(0, 10).Trim() : quote.getVehicles().item(i).getVehMake(),
                            QM_MAKE_NUMBER = (short)quote.getVehicles().item(i).getMakeNumber(),
                            QM_VEH_MODEL = quote.getVehicles().item(i).getVehModel().Length > 10 ? quote.getVehicles().item(i).getVehModel().Substring(0,10).Trim() : quote.getVehicles().item(i).getVehModel(),
                            QM_MODEL_NUMBER = (short)quote.getVehicles().item(i).getModelNumber(),
                            QM_VEH_BODY_STYLE = quote.getVehicles().item(i).getVehBodyStyle().Length > 8 ? quote.getVehicles().item(i).getVehBodyStyle().Substring(0, 8).Trim() : quote.getVehicles().item(i).getVehBodyStyle(),
                            QM_VEH_WEB_MODEL = quote.getVehicles().item(i).getVehWebModel().Trim(),
                            QM_VEH_VIN_NUMBER = quote.getVehicles().item(i).getVehVinNumber().Trim(),
                            QM_VEH_USE = (short)quote.getVehicles().item(i).getVehUse(),
                            QM_VEH_SYMBOL = (short)quote.getVehicles().item(i).getVehSymbol(),
                            QM_VEH_ALT_TERR = (short)quote.getVehicles().item(i).getVehAltTerr(),
                            QM_VEH_EXPOSURE = (short)quote.getVehicles().item(i).getVehExposure(),
                            QM_VEH_HI_PERF_IND = (short)quote.getVehicles().item(i).getVehHiPerfInd(),
                            QM_VEH_ISO_COLL = 0,
                            QM_VEH_ISO_COMP = 0,
                            QM_VEH_SYMBOL_COLL = (short)quote.getVehicles().item(i).getVehSymbolIsoColl(),
                            QM_VEH_SYMBOL_COMP = (short)quote.getVehicles().item(i).getVehSymbolComp(),
                            QM_VEH_SYMBOL_LIAB = 0,
                            QM_VEH_SYMBOL_PIP = 0,
                            QM_VEH_SYMBOL_YEAR_ADJ = 0,
                            QM_VEH_TERR_BI = 0,
                            QM_VEH_TERR_COLL = 0,
                            QM_VEH_TERR_COMP = 0,
                            QM_VEH_TERR_PD = 0,
                            QM_VEH_TERR_PIP = 0,
                            QM_VEH_TERR_UM = 0,
                            QM_VEH_TYPE = 0,
                            QM_VEHICLE_ADDED_DATE = 0,
                            QM_VEHICLE_NO_V = "",
                            QM_VEHICLE_RECOVERY_TEST = 0,
                            QM_ADDL_INSURED_NO = 0,
                            QM_ADJUST_TO_MAKE_MODEL = quote.getVehicles().item(i).getAdjustToMakeModel(),
                            QM_AIR_BAG_TEST = 0,
                            QM_ALT_GAR_ZIP_CODE_1 = 0,
                            QM_ALT_GAR_ZIP_CODE_2 = 0,
                            QM_ALT_GARAGE_CITY = "",
                            QM_ALT_ZIP_LOCK_TEST = 0,
                            QM_ANNUAL_MILEAGE = 0,
                            QM_ANTI_LOCK_BRAKE_TEST = 0,
                            QM_AUTO_SURCHARGE = 0,
                            QM_CAMPER_TOP_TEST = 0,
                            QM_CITY_CODE = 0,
                            QM_COST_NEW = 0,
                            QM_COST_OF_CAMPER_TOP = 0,
                            QM_COUNTY_CODE = 0,
                            QM_COVERED_PROP_AMT = 0,
                            QM_CURR_ODOM_DATE = 0,
                            QM_CURRENT_ODOM = 0,
                            QM_CUST_EST_PLEASURE_MILES = 0,
                            QM_CUSTOM_EQUIP_COST = 0,
                            QM_DAYS_PER_WEEK = 0,
                            QM_DAYTIME_LIGHTS_TEST = 0,
                            QM_DISABLING_DEVICE = 0,
                            QM_DISTANCE = 0,
                            QM_DRIVE_TYPE_TEST = 0,
                            QM_EFT_DIS = 0,
                            QM_ENTER_VEH_DATE = "",
                            QM_EX_LIAB_TEST = 0,
                            QM_FACTORY_CUST_TEST = 0,
                            QM_FL_COUNTY_TEST = 0,
                            QM_HOMEOWNER_DIS = 0,
                            QM_INSPECTION_REQ_TEST = 0,
                            QM_LEASE_AI_TEST = 0,
                            QM_LOSS_PAYEE_NO = 0,
                            QM_MANUAL_OVERRIDE_TEST = 0,
                            QM_MULTI_CAR_DIS = 0,
                            QM_MULTI_POLICY_DIS = 0,
                            QM_MUNI_RATE = 0,
                            QM_MUNI_SURCHARGE = 0,
                            QM_ODOM_2 = 0,
                            QM_ODOM_DATE_2 = 0,
                            QM_ODOM_NEEDS_UPD_FLAG = 0,
                            QM_ORIG_VEHICLE_NO = 0,
                            QM_PARKING_TYPE = 0,
                            QM_PASSIVE_RESTRAINT = 0,
                            QM_PD_IN_FULL_DIS = 0,
                            QM_PERCENT_BUSINESS = 0,
                            QM_PERFORMANCE = 0,
                            QM_PREMIER_DISCOUNT_DATA = 0,
                            Qm_prior_annual_mileage = 0,
                            QM_PRIOR_ODOM = 0,
                            QM_PRIOR_ODOM_DATE = 0,
                            QM_PURCH_VEH_DATE = 0,
                            QM_PURCHASE_ODOM = 0,
                            QM_REG_TO_NON_RELATIVES_TEST = 0,
                            QM_RENEWAL_CREDIT_DATA = 0,
                            QM_REPLACEMENT_VEHICLE_FLAG = 0,
                            QM_RETIRED_DRIV_AGE_CAT = 0,
                            QM_RETIRED_DRIV_DIS = 0,
                            QM_RO_ADDL_FIRST_NAME = "",
                            QM_RO_ADDL_LAST_NAME = "",
                            QM_RO_ADDL_MIDDLE_INIT = "",
                            QM_RO_ADDRESS_CITY = "",
                            QM_RO_ADDRESS_LINE_1 = "",
                            QM_RO_ADDRESS_LINE_2 = "",
                            QM_RO_FIRST_NAME = "",
                            QM_RO_LAST_NAME = "",
                            QM_RO_MIDDLE_INIT = "",
                            QM_RO_STATE_CODE = "",
                            QM_RO_ZIP_CODE_1 = "",
                            QM_RO_ZIP_CODE_2 = "",
                            QM_SAFE_VEH = 0,
                            QM_SAFE_VEH_DIS = 0,
                            QM_STORED_VEH_DATE = 0,
                            QM_STORED_VEH_TEST = 0,
                            QM_TAX_CODE = 0,
                            QM_TERR_ADJ_TYPE = 0,
                            QM_TERR_ADQ_TYPE = 0,
                            QM_TONAGE_TEST = 0,
                            QM_UPD_ANNUAL_MILEAGE = 0,
                            QM_UPD_CURRENT_ODOM = 0,
                            QM_UPD_ODOM_DATE = 0,
                            QM_WEB_DATA = 0,
                            QM_WEEKS_PER_MONTH = 0,
                            QM_WINDOW_ETCHING_TEST = 0,
                            QM_YEAR_MAKE_MODEL = ""
                        });
                    }
                    for (int i = 0; i < quote.getDrivers().count(); i++)
                    {
                        context.QuoteDrivers.Add(new QuoteDriver
                        {
                            PRIME_KEY_D = quotePrimeKey,
                            QM_DRIV_FIRST = quote.getDrivers().item(i).getDrivFirst().Trim(),
                            QM_DRIV_LAST = quote.getDrivers().item(i).getDrivLast().Trim()
                        });
                    }
                }
                else
                {
                    var updateQuote = existingQuote.First();
                    updateQuote.QM_QUOTE_NO_0 = quote.getQuoteInfo().getQuoteNo0();
                    updateQuote.QM_FIRST_NAME_OF_CUSTOMER = quote.getCustomer().getFirstNameOfCustomer();
                    updateQuote.QM_LAST_NAME_OF_CUSTOMER = quote.getCustomer().getLastNameOfCustomer();
                    updateQuote.QM_MIDDLE_INITIAL = quote.getCustomer().getMiddleInitial();
                    updateQuote.QM_ADDRESS_CITY = quote.getCustomer().getAddressCity();
                    updateQuote.QM_ADDRESS_LINE_1 = quote.getCustomer().getAddressLine1();
                    updateQuote.QM_ADDRESS_LINE_2 = quote.getCustomer().getAddressLine2();
                    updateQuote.QM_ADDRESS_STATE_CODE = quote.getCustomer().getAddressStateCode();
                    updateQuote.QM_ADDRESS_STATE_NO = (short)quote.getCustomer().getAddressStateNo();
                    updateQuote.QM_MASTER_STATE_NO = (short)quote.getCustomer().getMasterStateNo();
                    updateQuote.QM_ZIP_CODE_1 = (int)quote.getCustomer().getZipCode1();
                    updateQuote.QM_ZIP_CODE_2 = (int)quote.getCustomer().getZipCode2();
                    updateQuote.QM_ADDRESS_TERR = (short)quote.getCustomer().getAddressTerr();
                    updateQuote.QM_ADDTL_CUSTOMER_TITLE = quote.getCustomer().getAddtlCustomerTitle();
                    updateQuote.QM_ADDTL_FIRST_NAME_OF_CUST = quote.getCustomer().getAddtlFirstNameOfCust();
                    updateQuote.QM_ADDTL_LAST_NAME_OF_CUST = quote.getCustomer().getAddtlLastNameOfCust();
                    updateQuote.QM_ADDTL_MIDDLE_INITIAL = quote.getCustomer().getAddtlMiddleInitial();
                    updateQuote.QM_ADDRESS_VERIFICATION_TEST = (short)quote.getCustomer().getAddressVerificationTest();
                    updateQuote.QM_CUSTOMER_TITLE = quote.getCustomer().getCustomerTitle();
                    updateQuote.QM_DAY_AREA_CODE = (short)quote.getCustomer().getDayAreaCode();
                    updateQuote.QM_DAY_PHONE1 = (short)quote.getCustomer().getDayPhone1();
                    updateQuote.QM_DAY_PHONE2 = (short)quote.getCustomer().getDayPhone2();
                    updateQuote.QM_DAY_EXT = (short)quote.getCustomer().getDayExt();
                    updateQuote.QM_DAY_PHONE_TYPE = (short)quote.getCustomer().getDayPhoneType();
                    updateQuote.QM_EVE_AREA_CODE = (short)quote.getCustomer().getEveAreaCode();
                    updateQuote.QM_EVE_PHONE1 = (short)quote.getCustomer().getEvePhone1();
                    updateQuote.QM_EVE_PHONE2 = (short)quote.getCustomer().getEvePhone2();
                    updateQuote.QM_EVE_EXT = (short)quote.getCustomer().getEveExt();
                    updateQuote.QM_EVE_PHONE_TYPE = (short)quote.getCustomer().getEvePhoneType();
                    updateQuote.QM_E_MAIL_ADDRESS = quote.getCustomer().getEMailAddress();
                    updateQuote.QM_POLICY_EFF_DATE = iPolicyEffDate;
                    updateQuote.QM_POLICY_EXP_DATE = iPolicyExpDate;
                    updateQuote.QM_EFF_DATE = iEffDate;
                    updateQuote.QM_EXP_DATE = iExpDate;
                    updateQuote.QM_QUOTE_EFF_DATE = iQuoteEffDate;
                    updateQuote.QM_EMAIL_ADDRESS_TEST = (short)quote.getPolicyInfo().getEmailAddressTest();
                    updateQuote.QM_RISK_ADDRESS_LINE_1 = quote.getCustomer().getRiskAddressLine1();
                    updateQuote.QM_RISK_ADDRESS_LINE_2 = quote.getCustomer().getRiskAddressLine2();
                    updateQuote.QM_RISK_ADDRESS_CITY = quote.getCustomer().getRiskAddressCity();
                    updateQuote.QM_RISK_STATE = (short)quote.getCustomer().getRiskState();
                    updateQuote.QM_RISK_ZIP_CODE_1 = (short)quote.getCustomer().getRiskZipCode1();
                    updateQuote.QM_RISK_ZIP_CODE_2 = (short)quote.getCustomer().getRiskZipCode2();
                    updateQuote.QM_RISK_TERR = (short)quote.getCustomer().getRiskTerr();
                    updateQuote.qm_market_brand = (short)quote.getPolicyInfo().getMarketBrand();
                    updateQuote.QM_MARKET_KEY_CODE = quote.getCustomer().getMarketKeyCode();
                    updateQuote.QM_MARKET_SOURCE_ADQ = (short)quote.getCustomer().getMarketSourceAdq();
                    updateQuote.QM_PRODUCT_CODE = (short)quote.getPolicyInfo().getProductCode();
                    updateQuote.QM_VERSION_NO = (short)quote.getPolicyInfo().getVersionNo();
                    updateQuote.QM_VERSION_DATE = iVersionDate;
                    updateQuote.QM_BILLING_METHOD = (short)quote.getPolicyInfo().getBillingMethod();
                    updateQuote.QM_CREDIT_MODEL = quote.getCustomer().getCreditModel();
                    updateQuote.QM_CREDIT_SCORE = quote.getCustomer().getCreditScore();
                    updateQuote.QM_CREDIT_SCORE_EFF_DATE = iCreditScoreEffDate;
                    updateQuote.QM_CREDIT_SCORE_TYPE = (short)quote.getPolicyInfo().getCreditScoreType();
                    updateQuote.QM_CREDIT_SOURCE = (short)quote.getPolicyInfo().getCreditSource();
                    updateQuote.QM_CREDIT_VENDOR = (short)quote.getPolicyInfo().getCreditVendor();
                    updateQuote.QM_DNQ_NO = (short)quote.getQuoteInfo().getDnqNo();
                    updateQuote.QM_CURRENT_CARRIER_NO = (short)quote.getCustomer().getCurrentCarrierNo();
                    updateQuote.QM_CURRENT_CARRIER_PREM = (short)quote.getPolicyInfo().getCurrentCarrierPrem();
                    updateQuote.QM_CURRENT_CARRIER_TYPE = (short)quote.getCustomer().getCurrentCarrierType();
                    updateQuote.QM_CURRENT_LIMITS = (short)quote.getCustomer().getCurrentLimits();
                    updateQuote.QM_CURRENT_CARRIER_TEST = (short)quote.getPolicyInfo().getCurrentCarrierTest();
                    updateQuote.QM_COMPLETE_CODE = (short)quote.getQuoteInfo().getCompleteCode();
                    updateQuote.QM_AMF_ACCOUNT_NO = (int)quote.getPolicyInfo().getAmfAccountNo();
                    updateQuote.QM_UNDERWRITING_CO_NO = (int)quote.getPolicyInfo().getUnderwritingCoNo();
                    updateQuote.QM_TOTAL_PREMIUM = (int)quote.getPolicyInfo().getTotalPremium();
                    updateQuote.QM_TERM_FACTOR = (int)quote.getPolicyInfo().getTermFactor();
                    updateQuote.QM_NO_OF_DRIVERS = (short)quote.getDrivers().count();
                    updateQuote.QM_NO_OF_VEHICLES = (short)quote.getVehicles().count();
                    updateQuote.QM_NO_OF_VIOLATIONS = (short)quote.getViolations().count();
                    updateQuote.QM_NO_OF_ACC_COMP = (short)quote.getAccidents().count();
                    updateQuote.QM_NO_OF_ADD_3_YRS = (short)quote.getCustomer().getNoOfAdd3Yrs();
                    updateQuote.QM_NO_OF_DAYS_LAPSED = (short)quote.getPolicyInfo().getNoOfDaysLapsed();
                    updateQuote.QM_NO_OF_YOUTHFULS = (short)quote.getPolicyInfo().getNoOfYouthfuls();
                    updateQuote.QM_MULTI_POLICY_TEST = (short)quote.getPolicyInfo().getMultiPolicyTest();
                    updateQuote.QM_MULTI_STATE_TEST = (short)quote.getCustomer().getMultiStateTest();
                    updateQuote.QM_MASTER_CO_NO = (short)quote.getCustomer().getMasterCoNo();
                    updateQuote.QM_NO_OF_EMP_3_YRS = (short)quote.getCustomer().getNoOfEmp3Yrs();
                    updateQuote.QM_ALT_QUOTE_TEST = (short)quote.getQuoteInfo().getAltQuoteTest();
                    updateQuote.qm_channel_method = (short)quote.getPolicyInfo().getChannelMethod();
                    updateQuote.QM_USER_ID_NO = (short)quote.getPolicyInfo().getUserIdNo();
                    updateQuote.QM_LOCATION_NO = (short)quote.getPolicyInfo().getLocationNo();
                    updateQuote.QM_ISSUE_DATE = iIssueDate;
                    updateQuote.QM_METHOD_CV_FORMS = (short)quote.getPolicyInfo().getMethodCvForms();
                    updateQuote.QM_SPECIAL_CORRES_NO = (short)quote.getCustomer().getSpecialCorresNo();
                    updateQuote.QM_PRODUCT_VERSION = (short)quote.getCustomer().getProductVersion();
                    updateQuote.QM_QUOTE_PRINT_TEST = (short)quote.getQuoteInfo().getQuotePrintTest();
                    updateQuote.QM_RESPONSE_NO = (short)quote.getQuoteInfo().getResponseNo();
                    updateQuote.QM_QUOTE_TRANS_TYPE = (short)quote.getQuoteInfo().getQuoteTransType();
                    updateQuote.QM_ORIG_QUOTE_DATE = iOrigQuoteDate;
                    updateQuote.QM_ORIG_COMPLETE_DATE = iOrigCompleteDate;
                    updateQuote.QM_RATE_ADJ_TERM = (short)quote.getPolicyInfo().getRateAdjTerm();
                    updateQuote.QM_RATE_ADJ_FACTOR = (short)quote.getPolicyInfo().getRateAdjFactor();
                    updateQuote.QM_RATE_CALC_TYPE = (short)quote.getPolicyInfo().getRateCalcType();
                    updateQuote.QM_RATE_ADJ_FACTOR_INCRMNT = (short)quote.getPolicyInfo().getRateAdjFactorIncrmnt();
                    updateQuote.QM_RATE_VERSION_DATE = iRateVersionDate;
                    updateQuote.QM_CONTACT_TYPE_NO = (short)quote.getCustomer().getContactTypeNo();
                    updateQuote.QM_EFT_TEST = (short)quote.getPolicyInfo().getEftTest();
                    updateQuote.QM_DEPT_NO = (short)quote.getPolicyInfo().getDeptNo();
                    updateQuote.QM_ENDORSER_USER_ID_NO = (short)quote.getPolicyInfo().getEndorserUserIdNo();
                    updateQuote.QM_DRIV_EXCLUSION_TEST = (short)quote.getCustomer().getDrivExclusionTest();
                    updateQuote.QM_ERMF_FACTOR = (short)quote.getPolicyInfo().getErmfFactor();
                    updateQuote.QM_CONVERTED_UW_COMPANY = (short)quote.getPolicyInfo().getUnderwritingCoNo();
                    updateQuote.QM_ASSIST_SCORE = 0;
                    updateQuote.QM_DOCUMENT_NO = (short)quote.getPolicyInfo().getDocumentNo();
                    updateQuote.QM_CUST_PROFILE_NO = quote.getCustomer().getCustProfileNo();
                    updateQuote.QM_CONTACT_DATE_END = 0;
                    updateQuote.QM_CONTACT_DATE_START = 0;
                    updateQuote.QM_CONTACT_RECV_DATE = 0;
                    updateQuote.QM_CONTACT_TIME_END = 0;
                    updateQuote.QM_CONTACT_TIME_START = 0;
                    updateQuote.QM_EXCLUDE_AUTO_CALL = (short)quote.getCustomer().getExcludeAutoCall();
                    updateQuote.QM_DNQ_BY_UW_CO_TEST = 0;
                    updateQuote.QM_HOME_OFFICE_NO = (short)quote.getPolicyInfo().getHomeOfficeNo();
                    updateQuote.QM_HOMEOWNER_VERIFY_TEST = (short)quote.getCustomer().getHomeownerVerifyTest();
                    updateQuote.QM_BEST_TIME_TO_CALL = 0;
                    updateQuote.qm_e_document_level = 0;
                    updateQuote.QM_LEVEL_I_NO = (short)quote.getPolicyInfo().getLevelIiNo();
                    updateQuote.qm_level_i_comm_rate = (short)quote.getPolicyInfo().getLevelIiCommRate();
                    updateQuote.qm_level_ii_comm_rate = (short)quote.getPolicyInfo().getLevelIiiCommRate();
                    updateQuote.QM_LEVEL_III_NO = (short)quote.getPolicyInfo().getLevelIiiNo();
                    updateQuote.qm_level_iii_comm_rate = (short)quote.getPolicyInfo().getLevelIiiCommRate();
                    updateQuote.QM_LEVEL_II_NO = (short)quote.getPolicyInfo().getLevelIiNo();
                    updateQuote.QM_NEXT_VEHICLE_NO = (short)quote.getPolicyInfo().getNextVehNo();
                    updateQuote.QM_NEXT_DRIVER_NO = (short)quote.getPolicyInfo().getNextDriverNo();
                    updateQuote.qm_no_of_hh_drivers = (short)quote.getPolicyInfo().getNoOfHhDrivers();
                    updateQuote.QM_NO_OF_REQUOTES = (short)quote.getQuoteInfo().getNoOfRequotes();
                    updateQuote.QM_MEMBER_NO = quote.getPolicyInfo().getMemberNo2();
                    updateQuote.QM_OPTION_FEE_1 = (short)quote.getPolicyInfo().getOptionFee1();
                    updateQuote.QM_OPTION_FEE_2 = (short)quote.getPolicyInfo().getOptionFee2();
                    updateQuote.QM_OPTION_FEE_3 = (short)quote.getPolicyInfo().getOptionFee3();
                    updateQuote.QM_OPTION_FEE_4 = (short)quote.getPolicyInfo().getOptionFee4();
                    updateQuote.QM_OPTION_FEE_1_DIFF = (short)quote.getPolicyInfo().getOptionFee1Diff();
                    updateQuote.QM_OPTION_FEE_2_DIFF = (short)quote.getPolicyInfo().getOptionFee2Diff();
                    updateQuote.QM_OPTION_FEE_3_DIFF = (short)quote.getPolicyInfo().getOptionFee3Diff();
                    updateQuote.QM_OPTION_FEE_4_DIFF = (short)quote.getPolicyInfo().getOptionFee4Diff();
                    updateQuote.QM_MESSAGE_LINE1 = quote.getCustomer().getMessageLine1();
                    updateQuote.QM_MESSAGE_LINE2 = quote.getCustomer().getMessageLine2();
                    updateQuote.QM_OPTION_FEE1_TFA = (short)quote.getPolicyInfo().getOptionFee1Tfa();
                    updateQuote.QM_OPTION_FEE2_TFA = (short)quote.getPolicyInfo().getOptionFee2Tfa();
                    updateQuote.QM_OPTION_FEE3_TFA = (short)quote.getPolicyInfo().getOptionFee3Tfa();
                    updateQuote.QM_OPTION_FEE4_TFA = (short)quote.getPolicyInfo().getOptionFee4Tfa();
                    updateQuote.QM_ORIGIN_CODE_NO = 0;
                    updateQuote.QM_PAID_IN_FULL_TEST = (short)quote.getPolicyInfo().getPaidInFullTest();
                    updateQuote.QM_PAID_IN_FULL_OPTION_PREM = (int)quote.getPolicyInfo().getPaidInFullOptionPrem();
                    updateQuote.QM_POLICY_FEE = (short)quote.getPolicyInfo().getPolicyFee();
                    updateQuote.qm_no_of_can_notice_12_month = (short)quote.getPolicyInfo().getNoOfCanNotice12Month();
                    updateQuote.QM_PHONE_QUOTE_TEST = 0;
                    updateQuote.QM_MASTER_TERR_NO = (short)quote.getCustomer().getMasterTerrNo();
                    updateQuote.qm_policy_form = (short)quote.getPolicyInfo().getPolicyForm();
                    updateQuote.QM_QUOTE_TEST = (short)quote.getQuoteInfo().getQuoteTest();
                    updateQuote.QM_REFERENCE_NO = quote.getCustomer().getReferenceNo();
                    updateQuote.QM_RENT_OWN_TEST = (short)quote.getCustomer().getRentOwnTest();
                    updateQuote.QM_REG_OWNER_TEST = (short)quote.getCustomer().getRegOwnerTest();
                    updateQuote.QM_REGUARANTEE_QUOTE_TEST = (short)quote.getQuoteInfo().getReguaranteeQuoteTest();
                    updateQuote.QM_REQUOTE_DATE = iRequoteDate;
                    updateQuote.QM_SOCIAL_SECURITY_NO = quote.getCustomer().getSocialSecurityNo();
                    updateQuote.QM_SERVICING_OFFICE = 0;
                    updateQuote.QM_QUOTE_TAX = 0;
                    updateQuote.QM_QUASI_BIND_TEST = 0;
                    updateQuote.QM_QUOTE_CONV_DATE = iQuoteConvDate;
                    updateQuote.QM_SOLICIT_ID = quote.getCustomer().getSolicitId();
                    updateQuote.QM_PREMIUM_TERM = (short)quote.getPolicyInfo().getPremiumTerm();
                    updateQuote.QM_QUOTE_CANCEL_DATE = 0;
                    //updateQuote.QM_social_security_no_enc = (byte)0;
                    updateQuote.QM_WEB_DISCOUNT = 1;
                    updateQuote.QM_SR22_APPLIED_TEST = (short)quote.getCustomer().getSr22AppliedTest();
                    updateQuote.QM_SR22_FILING_FEE = (short)quote.getPolicyInfo().getSr22FilingFee();
                    updateQuote.QM_SUB_PRODUCT_CODE = 0;
                    updateQuote.QM_TELE_CONTACT_DATE = 0;
                    updateQuote.QM_RISK_STNO_LOCK_TEST = 0;
                    updateQuote.QM_RISK_ZIP_LOCK_TEST = 0;
                    updateQuote.QM_TIER_LOCK_TEST = (short)quote.getQuoteInfo().getTierLockTest();
                    updateQuote.QM_STATE_CO_LOCK_TEST = 0;
                    updateQuote.QM_TORT_PREMIUM = 0;
                    updateQuote.QM_QUOTE_PRINT_POINTER = 0;
                    updateQuote.QM_POST_MARK_DATE = 0;
                    updateQuote.QM_PRIOR_CONT_INS_TEST = (short)quote.getPolicyInfo().getPriorContInsTest();
                    updateQuote.QM_USE_NEW_BILLING_FLAG = (short)quote.getPolicyInfo().getUseNewBillingFlag();
                    updateQuote.qm_ratemaker_vers_date = iRatemakerVersDate;
                    updateQuote.QM_ALTERNATE_CONTACT = "";
                    updateQuote.QM_ANNUAL_INCOME = (short)quote.getCustomer().getAnnualIncome();
                    updateQuote.QM_ACCTNUM_LOCK_TEST = 0;
                    updateQuote.QM_LAST_REQUOTE_TRANS = 0;
                    updateQuote.QM_MAIL_ID = "";
                    updateQuote.QM_MAIL_STNO_LOCK_TEST = 0;
                    updateQuote.QM_MARKET_KEY_RESERVE = "";
                    updateQuote.QM_MAIL_ZIP_LOCK_TEST = 0;
                    updateQuote.QM_LL_3_APPLIED_TEST = 0;
                    updateQuote.QM_PHONE_EXTENSION = (short)quote.getCustomer().getPhoneExtension();
                    updateQuote.QM_PREMIUM_FINANCE_TEST = 0;
                    updateQuote.QM_PREMIUM_FINANCE_CO_NO = 0;
                    updateQuote.QM_INSP_CONT_COV_SINCE_DATE = 0;
                    updateQuote.QM_INSP_PRI_INS_WAIVER_TEST = 0;
                    updateQuote.QM_LAST_ENDORSEMENT_DATE = 0;
                    updateQuote.QM_MASTER_TERR_ADJ_TYPE = 0;
                    updateQuote.QM_MASTER_TERR_ADQ_TYPE = 0;
                    updateQuote.QM_PERSONAL_MESS_NO = 0;
                    updateQuote.QM_POLICY_REFERENCE_NO = "";

                    int vehPrimeKey = updateQuote.PRIME_KEY;
                    var existingVehicles = from v in context.QuoteVehicles
                                         where v.PRIME_KEY_V.Equals(vehPrimeKey)
                                         select v;
                    int i = 0;
                    foreach (QuoteVehicle updateVehicle in existingVehicles)
                    {
                        updateVehicle.QM_VEH_YEAR = (short)quote.getVehicles().item(i).getVehYear();
                        updateVehicle.QM_VEH_MAKE = quote.getVehicles().item(i).getVehMake().Length > 10 ? quote.getVehicles().item(i).getVehMake().Substring(0,10).Trim() : quote.getVehicles().item(i).getVehMake();
                        updateVehicle.QM_MAKE_NUMBER = (short)quote.getVehicles().item(i).getMakeNumber();
                        updateVehicle.QM_VEH_MODEL = quote.getVehicles().item(i).getVehModel().Length > 10 ? quote.getVehicles().item(i).getVehModel().Substring(0, 10).Trim() : quote.getVehicles().item(i).getVehModel();
                        updateVehicle.QM_MODEL_NUMBER = (short)quote.getVehicles().item(i).getModelNumber();
                        updateVehicle.QM_VEH_BODY_STYLE = quote.getVehicles().item(i).getVehBodyStyle().Length > 8 ? quote.getVehicles().item(i).getVehBodyStyle().Substring(0, 8).Trim() : quote.getVehicles().item(i).getVehBodyStyle();
                        updateVehicle.QM_VEH_WEB_MODEL = quote.getVehicles().item(i).getVehWebModel().Trim();
                        updateVehicle.QM_VEH_VIN_NUMBER = quote.getVehicles().item(i).getVehVinNumber();
                        updateVehicle.QM_VEH_USE = (short)quote.getVehicles().item(i).getVehUse();
                        updateVehicle.QM_VEH_SYMBOL = (short)quote.getVehicles().item(i).getVehSymbol();
                        updateVehicle.QM_VEH_ALT_TERR = (short)quote.getVehicles().item(i).getVehAltTerr();
                        updateVehicle.QM_VEH_EXPOSURE = (short)quote.getVehicles().item(i).getVehExposure();
                        updateVehicle.QM_VEH_HI_PERF_IND = (short)quote.getVehicles().item(i).getVehHiPerfInd();
                        updateVehicle.QM_VEH_ISO_COLL = 0;
                        updateVehicle.QM_VEH_ISO_COMP = 0;
                        updateVehicle.QM_VEH_SYMBOL_COLL = (short)quote.getVehicles().item(i).getVehSymbolIsoColl();
                        updateVehicle.QM_VEH_SYMBOL_COMP = (short)quote.getVehicles().item(i).getVehSymbolComp();
                        updateVehicle.QM_VEH_SYMBOL_LIAB = 0;
                        updateVehicle.QM_VEH_SYMBOL_PIP = 0;
                        updateVehicle.QM_VEH_SYMBOL_YEAR_ADJ = 0;
                        updateVehicle.QM_VEH_TERR_BI = 0;
                        updateVehicle.QM_VEH_TERR_COLL = 0;
                        updateVehicle.QM_VEH_TERR_COMP = 0;
                        updateVehicle.QM_VEH_TERR_PD = 0;
                        updateVehicle.QM_VEH_TERR_PIP = 0;
                        updateVehicle.QM_VEH_TERR_UM = 0;
                        updateVehicle.QM_VEH_TYPE = 0;
                        updateVehicle.QM_VEHICLE_ADDED_DATE = 0;
                        updateVehicle.QM_VEHICLE_NO_V = "";
                        updateVehicle.QM_VEHICLE_RECOVERY_TEST = 0;
                        updateVehicle.QM_ADDL_INSURED_NO = 0;
                        updateVehicle.QM_ADJUST_TO_MAKE_MODEL = quote.getVehicles().item(i).getAdjustToMakeModel();
                        updateVehicle.QM_AIR_BAG_TEST = 0;
                        updateVehicle.QM_ALT_GAR_ZIP_CODE_1 = 0;
                        updateVehicle.QM_ALT_GAR_ZIP_CODE_2 = 0;
                        updateVehicle.QM_ALT_GARAGE_CITY = "";
                        updateVehicle.QM_ALT_ZIP_LOCK_TEST = 0;
                        updateVehicle.QM_ANNUAL_MILEAGE = 0;
                        updateVehicle.QM_ANTI_LOCK_BRAKE_TEST = 0;
                        updateVehicle.QM_AUTO_SURCHARGE = 0;
                        updateVehicle.QM_CAMPER_TOP_TEST = 0;
                        updateVehicle.QM_CITY_CODE = 0;
                        updateVehicle.QM_COST_NEW = 0;
                        updateVehicle.QM_COST_OF_CAMPER_TOP = 0;
                        updateVehicle.QM_COUNTY_CODE = 0;
                        updateVehicle.QM_COVERED_PROP_AMT = 0;
                        updateVehicle.QM_CURR_ODOM_DATE = 0;
                        updateVehicle.QM_CURRENT_ODOM = 0;
                        updateVehicle.QM_CUST_EST_PLEASURE_MILES = 0;
                        updateVehicle.QM_CUSTOM_EQUIP_COST = 0;
                        updateVehicle.QM_DAYS_PER_WEEK = 0;
                        updateVehicle.QM_DAYTIME_LIGHTS_TEST = 0;
                        updateVehicle.QM_DISABLING_DEVICE = 0;
                        updateVehicle.QM_DISTANCE = 0;
                        updateVehicle.QM_DRIVE_TYPE_TEST = 0;
                        updateVehicle.QM_EFT_DIS = 0;
                        updateVehicle.QM_ENTER_VEH_DATE = "";
                        updateVehicle.QM_EX_LIAB_TEST = 0;
                        updateVehicle.QM_FACTORY_CUST_TEST = 0;
                        updateVehicle.QM_FL_COUNTY_TEST = 0;
                        updateVehicle.QM_HOMEOWNER_DIS = 0;
                        updateVehicle.QM_INSPECTION_REQ_TEST = 0;
                        updateVehicle.QM_LEASE_AI_TEST = 0;
                        updateVehicle.QM_LOSS_PAYEE_NO = 0;
                        updateVehicle.QM_MANUAL_OVERRIDE_TEST = 0;
                        updateVehicle.QM_MULTI_CAR_DIS = 0;
                        updateVehicle.QM_MULTI_POLICY_DIS = 0;
                        updateVehicle.QM_MUNI_RATE = 0;
                        updateVehicle.QM_MUNI_SURCHARGE = 0;
                        updateVehicle.QM_ODOM_2 = 0;
                        updateVehicle.QM_ODOM_DATE_2 = 0;
                        updateVehicle.QM_ODOM_NEEDS_UPD_FLAG = 0;
                        updateVehicle.QM_ORIG_VEHICLE_NO = 0;
                        updateVehicle.QM_PARKING_TYPE = 0;
                        updateVehicle.QM_PASSIVE_RESTRAINT = 0;
                        updateVehicle.QM_PD_IN_FULL_DIS = 0;
                        updateVehicle.QM_PERCENT_BUSINESS = 0;
                        updateVehicle.QM_PERFORMANCE = 0;
                        updateVehicle.QM_PREMIER_DISCOUNT_DATA = 0;
                        updateVehicle.Qm_prior_annual_mileage = 0;
                        updateVehicle.QM_PRIOR_ODOM = 0;
                        updateVehicle.QM_PRIOR_ODOM_DATE = 0;
                        updateVehicle.QM_PURCH_VEH_DATE = 0;
                        updateVehicle.QM_PURCHASE_ODOM = 0;
                        updateVehicle.QM_REG_TO_NON_RELATIVES_TEST = 0;
                        updateVehicle.QM_RENEWAL_CREDIT_DATA = 0;
                        updateVehicle.QM_REPLACEMENT_VEHICLE_FLAG = 0;
                        updateVehicle.QM_RETIRED_DRIV_AGE_CAT = 0;
                        updateVehicle.QM_RETIRED_DRIV_DIS = 0;
                        updateVehicle.QM_RO_ADDL_FIRST_NAME = "";
                        updateVehicle.QM_RO_ADDL_LAST_NAME = "";
                        updateVehicle.QM_RO_ADDL_MIDDLE_INIT = "";
                        updateVehicle.QM_RO_ADDRESS_CITY = "";
                        updateVehicle.QM_RO_ADDRESS_LINE_1 = "";
                        updateVehicle.QM_RO_ADDRESS_LINE_2 = "";
                        updateVehicle.QM_RO_FIRST_NAME = "";
                        updateVehicle.QM_RO_LAST_NAME = "";
                        updateVehicle.QM_RO_MIDDLE_INIT = "";
                        updateVehicle.QM_RO_STATE_CODE = "";
                        updateVehicle.QM_RO_ZIP_CODE_1 = "";
                        updateVehicle.QM_RO_ZIP_CODE_2 = "";
                        updateVehicle.QM_SAFE_VEH = 0;
                        updateVehicle.QM_SAFE_VEH_DIS = 0;
                        updateVehicle.QM_STORED_VEH_DATE = 0;
                        updateVehicle.QM_STORED_VEH_TEST = 0;
                        updateVehicle.QM_TAX_CODE = 0;
                        updateVehicle.QM_TERR_ADJ_TYPE = 0;
                        updateVehicle.QM_TERR_ADQ_TYPE = 0;
                        updateVehicle.QM_TONAGE_TEST = 0;
                        updateVehicle.QM_UPD_ANNUAL_MILEAGE = 0;
                        updateVehicle.QM_UPD_CURRENT_ODOM = 0;
                        updateVehicle.QM_UPD_ODOM_DATE = 0;
                        updateVehicle.QM_WEB_DATA = 0;
                        updateVehicle.QM_WEEKS_PER_MONTH = 0;
                        updateVehicle.QM_WINDOW_ETCHING_TEST = 0;
                        updateVehicle.QM_YEAR_MAKE_MODEL = "";
                        i++;
                    }
                    int drvPrimeKey = updateQuote.PRIME_KEY;
                    var existingDrivers = from d in context.QuoteDrivers
                                         where d.PRIME_KEY_D.Equals(drvPrimeKey)
                                         select d;
                    i=0;
                    foreach (QuoteDriver updateDriver in existingDrivers)
                    {
                        updateDriver.PRIME_KEY_D = drvPrimeKey;
                        updateDriver.QM_DRIV_FIRST = quote.getDrivers().item(i).getDrivFirst().Trim();
                        updateDriver.QM_DRIV_LAST = quote.getDrivers().item(i).getDrivLast().Trim();
                        i++;
                    }
                }
                context.SaveChanges();
            }
        }
        public string GetNewQuoteNo()
        {
            using (var context = new AutoQuoteEntitie7())
            {
                var count = context.Quotes.Count();
                if (count == 0)
                    return " 1000001";
                var lastQuote = context.Quotes.Max(x => x.QM_QUOTE_NO_0);
                int nextQuote = 0;
                int.TryParse(lastQuote, out nextQuote);
                nextQuote++;
                return " " + nextQuote.ToString();
            }
        }

        private string GetCoverageXML(AutoQuote.Autoquote quote, XmlDocument xml, string state)
        {
            //Coverages XML
            //string CoveragesXML = covs.serialize();
            //xml.LoadXml(CoveragesXML);

            string PolicyPremiums = GetPolicyPremiums(quote);
            string VehiclePremiums = GetVehicleDetailsAndOrPremiums(quote);
            string CalculatedPremiums = GetCalculatedPremiums(quote, ref PolicyPremiums);
            string EnhanceCoverages = GetEnhancedCovPremiums(quote);
            //string enhancePremiums = enhanceCov.GetAllVehiclesWEhanceCoverages(quote, covs, xml, true, true);
            //string enhancePremiums = GetAllVehiclesWEhanceCoverages(quote, true);

            //Derive and format (translate) Discounts don't need here to call GetDiscounts
            //**** String Discounts = GetDiscounts(ref quote, ref state, ref isAffinity, true);

            //This is a layer to modify the xml and make it suitable for the calling app
           // TransformXml(xml, ConfigurationManager.AppSettings["PluginsPath"] + "\\xsl\\CleanQuoteForWeb.xslt");
            // CoveragesXML = xml.OuterXml.ToString();
            string CoveragesXML = GetCoverages(quote);
            //ysang 6/17/2011 7246 add here
            //string sCovOption = "";

            //if (state == "NJ")
            //{
            //    sCovOption = CoverageOptionsSelect.GetCoverageOption(state);
            //}
            CoveragesXML = CoveragesXML.Replace("</Coverages>", PolicyPremiums + VehiclePremiums + EnhanceCoverages + CalculatedPremiums + "</Coverages>");


            return CoveragesXML;
        }

        public string GetCoverages(AutoQuote.Autoquote quote)
        {
            string result = "<Coverages>";
            result += "<PolicyCoverages>";
            
            var polCovs = new []{
	            new {
                    CoverageCode = "Bi", 
                    QuoteProperty = "BiPerPerson100~BiPerOcc100",
                    WebQuestionID = "POLCOV_BI_LABEL", 
                    SuppressRendering = "False", 
                    VehIndex = -1,
                    Invalid = "False",
                    Offered = "True",
                    Abbrev = "BI",
                    Desc = "Bodily Injury Liability",
                    CovOption = "False",
                    CovInputType = "combo",
                    Limits = new []{
                        new {
                            Value = "250~500", //setBiPerOcc100
                            Abbrev = "25/50",
                            Caption = "$25,000 per person / $50,000 per accident",
                            SortOrder = "2",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "500~1000",
                            Abbrev = "50/100",
                            Caption = "$50,000 per person / $100,000 per accident",
                            SortOrder = "3",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "1000~3000",
                            Abbrev = "100/300",
                            Caption = "$100,000 per person / $300,000 per accident",
                            SortOrder = "4",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "2500~5000",
                            Abbrev = "250/500",
                            Caption = "$250,000 per person / $500,000 per accident",
                            SortOrder = "5",
                            IsNoCov = "False",
                            Tag = ""
                        }
                    },
                    SelectedLimitValue = quote.getCoverages().item(0).getPolicyCoverage().getBiPerPerson100().ToString() + "~" + quote.getCoverages().item(0).getPolicyCoverage().getBiPerOcc100().ToString(),
                    FAQText = "Pays for financial loss when you are held legally responsible for an accident that causes injury or loss of life to other individuals.  This protection also provides for your defense against a claim or lawsuit.",
                    IsForceEditOnChange = "False"
                },
                 new {
                    CoverageCode = "Pd", 
                    QuoteProperty = "PdSl100",
                    WebQuestionID = "POLCOV_PD", 
                    SuppressRendering = "False", 
                    VehIndex = -1,
                    Invalid = "False",
                    Offered = "True",
                    Abbrev = "Pd",
                    Desc = "Property Damage Liability",
                    CovOption = "False",
                    CovInputType = "combo",
                    Limits = new []{
                        new {
                            Value = "250",
                            Abbrev = "25K",
                            Caption = "$25,000",
                            SortOrder = "2",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "500",
                            Abbrev = "50K",
                            Caption = "$50,000",
                            SortOrder = "3",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "1000",
                            Abbrev = "100K",
                            Caption = "$100,000",
                            SortOrder = "4",
                            IsNoCov = "False",
                            Tag = ""
                        }
                    },
                    SelectedLimitValue = quote.getCoverages().item(0).getPolicyCoverage().getPdSl100().ToString(),
                    FAQText = " Pays for financial loss when you are held legally responsible for an auto accident that causes damage to other people's property, including the vehicles of other drivers.",
                    IsForceEditOnChange = "False"
                },
                 new {
                    CoverageCode = "MedPay", 
                    QuoteProperty = "MedPay10",
                    WebQuestionID = "POLCOV_MEDPAY_LABEL", 
                    SuppressRendering = "False", 
                    VehIndex = -1,
                    Invalid = "False",
                    Offered = "True",
                    Abbrev = "MedPay",
                    Desc = "Medical Payments",
                    CovOption = "False",
                    CovInputType = "combo",
                    Limits = new []{
                        new {
                            Value = "50",
                            Abbrev = "500",
                            Caption = "$500",
                            SortOrder = "1",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "100",
                            Abbrev = "1K",
                            Caption = "$1,000",
                            SortOrder = "2",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "200",
                            Abbrev = "2K",
                            Caption = "$2,000",
                            SortOrder = "3",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "500",
                            Abbrev = "5K",
                            Caption = "$5,000",
                            SortOrder = "4",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "1000",
                            Abbrev = "10K",
                            Caption = "$10,000",
                            SortOrder = "5",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "2500",
                            Abbrev = "25K",
                            Caption = "$25,000",
                            SortOrder = "6",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "0",
                            Abbrev = "No Cov",
                            Caption = "No Coverage",
                            SortOrder = "7",
                            IsNoCov = "True",
                            Tag = ""
                        }
                    },
                    SelectedLimitValue = quote.getCoverages().item(0).getPolicyCoverage().getMedPay10().ToString(),
                    FAQText = "Pays reasonable expenses for you and your passengers for medical services which result from an automobile accident. ",
                    IsForceEditOnChange = "False"
                },
                new {
                    CoverageCode = "Umbi", 
                    QuoteProperty = "UmbiPerPerson100~UmbiPerOcc100",
                    WebQuestionID = "POLCOV_UMBI_A_LABEL", 
                    SuppressRendering = "False", 
                    VehIndex = -1,
                    Invalid = "False",
                    Offered = "True",
                    Abbrev = "Umbi",
                    Desc = "Uninsured Motorists Bodily Liability",
                    CovOption = "False",
                    CovInputType = "combo",
                    Limits = new []{
                        new {
                            Value = "250~500",
                            Abbrev = "25/50",
                            Caption = "$25,000 per person / $50,000 per accident",
                            SortOrder = "2",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "500~1000",
                            Abbrev = "50/100",
                            Caption = "$50,000 per person / $100,000 per accident",
                            SortOrder = "3",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "1000~3000",
                            Abbrev = "100/300",
                            Caption = "$100,000 per person / $300,000 per accident",
                            SortOrder = "4",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "2500~5000",
                            Abbrev = "250/500",
                            Caption = "$250,000 per person / $500,000 per accident",
                            SortOrder = "5",
                            IsNoCov = "False",
                            Tag = ""
                        }
                    },
                    SelectedLimitValue = quote.getCoverages().item(0).getVehicleCoverages().item(0).getUmbiPerPerson100().ToString() + "~" + quote.getCoverages().item(0).getVehicleCoverages().item(0).getUmbiPerOcc100().ToString(),
                    FAQText = "Coverage designed to provide protection for you, your family members and others in your auto for financial loss resulting from bodily injury or death caused by someone who did not buy insurance or if you are involved in a hit-and-run accident.",
                    IsForceEditOnChange = "False"
                },
                 new {
                    CoverageCode = "Umpd", 
                    QuoteProperty = "UmPd100",
                    WebQuestionID = "POLCOV_UMPD", 
                    SuppressRendering = "False", 
                    VehIndex = -1,
                    Invalid = "False",
                    Offered = "True",
                    Abbrev = "Umpd",
                    Desc = "Uninsured Motorists Property Damage",
                    CovOption = "False",
                    CovInputType = "combo",
                    Limits = new []{
                        new {
                            Value = "250",
                            Abbrev = "25K",
                            Caption = "$25,000",
                            SortOrder = "2",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "500",
                            Abbrev = "50K",
                            Caption = "$50,000",
                            SortOrder = "3",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "1000",
                            Abbrev = "100K",
                            Caption = "$100,000",
                            SortOrder = "4",
                            IsNoCov = "False",
                            Tag = ""
                        }
                    },
                    SelectedLimitValue = quote.getCoverages().item(0).getVehicleCoverages().item(0).getUmPd100().ToString(),
                    FAQText = "Pays for loss or damage to your vehicle that you are legally entitled to receive from the owner or driver of an underinsured vehicle.",
                    IsForceEditOnChange = "False"
                },
                 new {
                    CoverageCode = "UmpdOption", 
                    QuoteProperty = "UmPdDed",
                    WebQuestionID = "POLCOV_UMPDDED", 
                    SuppressRendering = "False", 
                    VehIndex = -1,
                    Invalid = "False",
                    Offered = "True",
                    Abbrev = "Umpd Ded",
                    Desc = "Uninsured Motorists Property Damage Deductible",
                    CovOption = "False",
                    CovInputType = "combo",
                    Limits = new []{
                        new {
                            Value = "250",
                            Abbrev = "25K",
                            Caption = "$250",
                            SortOrder = "2",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "500",
                            Abbrev = "50K",
                            Caption = "$500",
                            SortOrder = "3",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "1000",
                            Abbrev = "100K",
                            Caption = "$1000",
                            SortOrder = "4",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "0",
                            Abbrev = "n/a",
                            Caption = "n/a",
                            SortOrder = "5",
                            IsNoCov = "False",
                            Tag = ""
                        }
                    },
                    SelectedLimitValue = quote.getCoverages().item(0).getVehicleCoverages().item(0).getUmPdDed().ToString(),
                    FAQText = "Pays for loss or damage to your vehicle that you are legally entitled to receive from the owner or driver of an underinsured vehicle.",
                    IsForceEditOnChange = "False"
                },
                 new {
                    CoverageCode = "UmType", 
                    QuoteProperty = "UmType",
                    WebQuestionID = "POLCOV_UMTYPE", 
                    SuppressRendering = "False", 
                    VehIndex = -1,
                    Invalid = "False",
                    Offered = "True",
                    Abbrev = "UMType",
                    Desc = "Uninsured Motorists Coverage Type",
                    CovOption = "False",
                    CovInputType = "combo",
                    Limits = new []{
                        new {
                            Value = "0",
                            Abbrev = "Added on",
                            Caption = "Added on to At-Fault Liability Limits",
                            SortOrder = "1",
                            IsNoCov = "False",
                            Tag = ""
                        },
                        new {
                            Value = "6",
                            Abbrev = "Reduced by",
                            Caption = "Reduced by At-Fault Liability Limits",
                            SortOrder = "6",
                            IsNoCov = "False",
                            Tag = ""
                        }
                    },
                    SelectedLimitValue = quote.getCoverages().item(0).getPolicyCoverage().getUmType().ToString(),
                    FAQText = "This coverage will only pay up to the difference between the at-fault driver's Liability Coverage and your Uninsured Motorists Coverage limits.  This means that this coverage will allow you to collect from the at-fault driver's and your Uninsured Motorists Coverage, combined, up to the same limit of Uninsured Motorists Coverage you have purchased.  In other words, the maximum payment available from this coverage for a claim would be equal to your Uninsured Motorists Coverage limit regardless of the at-fault driver's Liability Coverage limit.&lt;/P&gt;&lt;P&gt;The two Uninsured Motorists Coverage choices do not differ if the at-fault driver responsible for your injuries or property damage does not have any Liability Coverage.  In such cases, both types of Uninsured Motorists Coverage will pay up to the amount of Uninsured Motorist Coverage purchased.",
                    IsForceEditOnChange = "False"
                }
            };

            foreach (var cov in polCovs)
            {
                result += "<Coverage>";
                result += "<CovCode>" + cov.CoverageCode + "</CovCode>";
                result += "<QuoteProperty>" + cov.QuoteProperty + "</QuoteProperty>";
                result += "<WebQuestionID>" + cov.WebQuestionID + "</WebQuestionID>";
                result += "<SuppressRendering>" + cov.SuppressRendering + "</SuppressRendering>";
                result += "<Invalid>" + cov.Invalid + "</Invalid>";
                result += "<Offered>" + cov.Offered + "</Offered>";
                result += "<Abbrev>" + cov.Abbrev + "</Abbrev>";
                result += "<Desc>" + cov.Desc + "</Desc>";
                result += "<CovOption>" + cov.CovOption + "</CovOption>";
                result += "<CovInputType>" + cov.CovInputType + "</CovInputType>";
                result += "<Limits>";
                foreach (var lim in cov.Limits)
                {
                    result += "<Limit>";
                    result += "<Value>" + lim.Value + "</Value>";
                    result += "<Abbrev>" + lim.Abbrev + "</Abbrev>";
                    result += "<Caption>" + lim.Caption + "</Caption>";
                    result += "<SortOrder>" + lim.SortOrder + "</SortOrder>";
                    result += "<IsNoCov>" + lim.IsNoCov + "</IsNoCov>";
                    result += "<Tag>" + lim.Tag + "</Tag>";
                    result += "</Limit>";
                }
                result += "<SelectedLimitValue>" + cov.SelectedLimitValue + "</SelectedLimitValue>";
                result += "</Limits>";
                result += "<FAQText>" + System.Web.HttpUtility.HtmlEncode(cov.FAQText) + "</FAQText>";
                result += "<IsForceEditOnChange>" + cov.IsForceEditOnChange + "</IsForceEditOnChange>";
                result += "</Coverage>";
            }
            result += "</PolicyCoverages>";
            result += "<VehicleCoverages>";
            
            for (int i=0; i< quote.getCoverages().item(0).getVehicleCoverages().count();i++)
            {
                result += "<Vehicle>";
                result += "<VehIndex>" + i.ToString() + "</VehIndex>";

                var vehCovs = new []{
	                new {
                        CoverageCode = "OtcDed", 
                        QuoteProperty = "CompDed",
                        WebQuestionID = "VEHCOV_COMPDED_O_LABEL", 
                        SuppressRendering = "False", 
                        VehIndex = i,
                        Invalid = "False",
                        Offered = "True",
                        Abbrev = "Otc",
                        Desc = "Other Than Collision",
                        CovOption = "False",
                        CovInputType = "combo",
                        TextIncrement = "",
                        TextMaxVal = "",
                        TextMinVal = "",
                        TextNoCovVal = "",
                        TextValue = "",
                        Limits = new []{
                            new {
                                Value = "100",
                                Abbrev = "100",
                                Caption = "Actual Cash Value less deductibles: $100",
                                SortOrder = "1",
                                IsNoCov = "False",
                                Tag = ""
                            },
                            new {
                                Value = "250",
                                Abbrev = "250",
                                Caption = "Actual Cash Value less deductibles: $250",
                                SortOrder = "2",
                                IsNoCov = "False",
                                Tag = ""
                            },
                            new {
                                Value = "500",
                                Abbrev = "500",
                                Caption = "Actual Cash Value less deductibles: $500",
                                SortOrder = "3",
                                IsNoCov = "False",
                                Tag = ""
                            },
                            new {
                                Value = "1000",
                                Abbrev = "1000",
                                Caption = "Actual Cash Value less deductibles: $1,000",
                                SortOrder = "4",
                                IsNoCov = "False",
                                Tag = ""
                            },
                            new {
                                Value = "0",
                                Abbrev = "No Cov",
                                Caption = "No Coverage",
                                SortOrder = "5",
                                IsNoCov = "True",
                                Tag = ""
                            }
                        },
                        SelectedLimitValue = quote.getCoverages().item(0).getVehicleCoverages().item(i).getCompDed().ToString(),
                        FAQText = "Previously known as Comprehensive Coverage, pays up to the actual cash value of your vehicle,   less the deductible, for damage that is not caused by impact with another   vehicle or object (for example: fire, theft, vandalism, flood, or striking an animal).   If you lease your vehicle, your finance company may require Other Than Collision Coverage.   If you are considering rejecting this coverage, we recommend that you first contact your finance company.",
                        IsForceEditOnChange = "False"
                    },
                    new {
                        CoverageCode = "CollDed", 
                        QuoteProperty = "CollDed",
                        WebQuestionID = "VEHCOV_COLLDED_LABEL", 
                        SuppressRendering = "False", 
                        VehIndex = i,
                        Invalid = "False",
                        Offered = "True",
                        Abbrev = "Coll",
                        Desc = "Collision Coverage",
                        CovOption = "False",
                        CovInputType = "combo",
                        TextIncrement = "",
                        TextMaxVal = "",
                        TextMinVal = "",
                        TextNoCovVal = "",
                        TextValue = "",
                        Limits = new []{
                            new {
                                Value = "100",
                                Abbrev = "100",
                                Caption = "Actual Cash Value less deductibles: $100",
                                SortOrder = "1",
                                IsNoCov = "False",
                                Tag = ""
                            },
                            new {
                                Value = "250",
                                Abbrev = "250",
                                Caption = "Actual Cash Value less deductibles: $250",
                                SortOrder = "2",
                                IsNoCov = "False",
                                Tag = ""
                            },
                            new {
                                Value = "500",
                                Abbrev = "500",
                                Caption = "Actual Cash Value less deductibles: $500",
                                SortOrder = "3",
                                IsNoCov = "False",
                                Tag = ""
                            },
                            new {
                                Value = "1000",
                                Abbrev = "1000",
                                Caption = "Actual Cash Value less deductibles: $1,000",
                                SortOrder = "4",
                                IsNoCov = "False",
                                Tag = ""
                            },
                            new {
                                Value = "0",
                                Abbrev = "No Cov",
                                Caption = "No Coverage",
                                SortOrder = "5",
                                IsNoCov = "True",
                                Tag = ""
                            }
                        },
                        SelectedLimitValue =  quote.getCoverages().item(0).getVehicleCoverages().item(i).getCollDed().ToString(),
                        FAQText = "Previously known as Comprehensive Coverage, pays up to the actual cash value of your vehicle,   less the deductible, for damage that is not caused by impact with another   vehicle or object (for example: fire, theft, vandalism, flood, or striking an animal).   If you lease your vehicle, your finance company may require Other Than Collision Coverage.   If you are considering rejecting this coverage, we recommend that you first contact your finance company.",
                        IsForceEditOnChange = "False"
                    },
                    new {
                        CoverageCode = "Towing", 
                        QuoteProperty = "TowAndLabor",
                        WebQuestionID = "VEHCOV_TOWLABOR_LABEL", 
                        SuppressRendering = "False", 
                        VehIndex = i,
                        Invalid = "False",
                        Offered = "True",
                        Abbrev = "Tow",
                        Desc = "Towing and Labor:",
                        CovOption = "False",
                        CovInputType = "combo",
                        TextIncrement = "",
                        TextMaxVal = "",
                        TextMinVal = "",
                        TextNoCovVal = "",
                        TextValue = "",
                        Limits = new []{
                            new {
                                Value = "50",
                                Abbrev = "50",
                                Caption = "Each Occurrence: $50",
                                SortOrder = "1",
                                IsNoCov = "False",
                                Tag = ""
                            },
                            new {
                                Value = "75",
                                Abbrev = "75",
                                Caption = "Each Occurrence: $75",
                                SortOrder = "2",
                                IsNoCov = "False",
                                Tag = ""
                            },
                            new {
                                Value = "0",
                                Abbrev = "No Cov",
                                Caption = "No Coverage",
                                SortOrder = "5",
                                IsNoCov = "True",
                                Tag = ""
                            }
                        },
                        SelectedLimitValue =  quote.getCoverages().item(0).getVehicleCoverages().item(i).getTowAndLabor().ToString(),
                        FAQText = "If you have Other Than Collision Coverage and Collision Coverage, you can purchase additional coverage to pay necessary towing and labor costs to move your damaged or inoperable vehicle to a body shop or other location.",
                        IsForceEditOnChange = "False"
                    },
                    new {
                        CoverageCode = "CustEquip", 
                        QuoteProperty = "SoundRecTranCost",
                        WebQuestionID = "", 
                        SuppressRendering = "False", 
                        VehIndex = i,
                        Invalid = "False",
                        Offered = "True",
                        Abbrev = "Cust",
                        Desc = "",
                        CovOption = "False",
                        CovInputType = "text",
                        TextIncrement = "1",
                        TextMaxVal = "10000",
                        TextMinVal = "0",
                        TextNoCovVal = "0",
                        TextValue = "0",
                        Limits = new []{
                            new {
                                Value = "",
                                Abbrev = "",
                                Caption = "",
                                SortOrder = "",
                                IsNoCov = "",
                                Tag = ""
                            }
                        },
                        SelectedLimitValue =  quote.getCoverages().item(0).getVehicleCoverages().item(i).getSoundRecTranCost().ToString(),
                        FAQText = "",
                        IsForceEditOnChange = "False"
                    }
                };
                foreach (var cov in vehCovs)
                {
                    result += "<Coverage>";
                    result += "<CovCode>" + cov.CoverageCode + "</CovCode>";
                    result += "<QuoteProperty>" + cov.QuoteProperty + "</QuoteProperty>";
                    result += "<WebQuestionID>" + cov.WebQuestionID + "</WebQuestionID>";
                    result += "<SuppressRendering>" + cov.SuppressRendering + "</SuppressRendering>";
                    result += "<Invalid>" + cov.Invalid + "</Invalid>";
                    result += "<Offered>" + cov.Offered + "</Offered>";
                    result += "<Abbrev>" + cov.Abbrev + "</Abbrev>";
                    result += "<Desc>" + cov.Desc + "</Desc>";
                    result += "<CovOption>" + cov.CovOption + "</CovOption>";
                    result += "<CovInputType>" + cov.CovInputType + "</CovInputType>";
                    if (cov.CovInputType == "combo")
                    {
                        result += "<Limits>";
                        foreach (var lim in cov.Limits)
                        {
                            result += "<Limit>";
                            result += "<Value>" + lim.Value + "</Value>";
                            result += "<Abbrev>" + lim.Abbrev + "</Abbrev>";
                            result += "<Caption>" + lim.Caption + "</Caption>";
                            result += "<SortOrder>" + lim.SortOrder + "</SortOrder>";
                            result += "<IsNoCov>" + lim.IsNoCov + "</IsNoCov>";
                            result += "<Tag>" + lim.Tag + "</Tag>";
                            result += "</Limit>";
                        }
                        result += "<SelectedLimitValue>" + cov.SelectedLimitValue + "</SelectedLimitValue>";
                        result += "</Limits>";
                    }
                    result += "<FAQText>" + System.Web.HttpUtility.HtmlEncode(cov.FAQText) + "</FAQText>";
                    result += "<IsForceEditOnChange>" + cov.IsForceEditOnChange + "</IsForceEditOnChange>";
                    if (cov.CovInputType == "text")
                    {
                        result += "<TextIncrement>" + cov.TextIncrement + "</TextIncrement>";
                        result += "<TextMaxVal>" + cov.TextMaxVal + "</TextMaxVal>";
                        result += "<TextMinVal>" + cov.TextMinVal + "</TextMinVal>";
                        result += "<TextNoCovVal>" + cov.TextNoCovVal + "</TextNoCovVal>";
                        result += "<TextValue>" + cov.TextValue + "</TextValue>";
                            
                    }
                    result += "</Coverage>";
                }
                result += "<SortOrder>" + (i+1).ToString() + "</SortOrder>";
                result += "<Year>" + quote.getVehicles().item(i).getVehYear().ToString() + "</Year>";
                result += "<Make>" + quote.getVehicles().item(i).getVehMake() + "</Make>";
                result += "<Model>" + quote.getVehicles().item(i).getVehModel() + "</Model>";
                result += "<BodyStyle>" + quote.getVehicles().item(i).getVehBodyStyle() + "</BodyStyle>";
                result += "<VinNumber>" + System.Web.HttpUtility.HtmlEncode(quote.getVehicles().item(i).getVehVinNumber()) + "</VinNumber>";
                result += "</Vehicle>";
            }
            result += "</VehicleCoverages>";
            if (AutoQuoteLibrary.AutoQuoteHelper.Utilities.IsVibeState(quote.getCustomer().getAddressStateCode()))
            {
                result += "<EnhancedCoverages>";
                var enCovs = new[]{
	                new {
                        CovCode = "Bundle1", 
                        QuoteProperty = "Bundle1Test",
                        WebQuestionID = "POLCOV_BUNDLE1TEST",
                        Name = "Protect Your Sanity",
                        SuppressRendering = "False",
                        VehIndex = "-1",
                        HelpText = "",
                        Invalid = "False",
                        Offered = "True",
                        Abbrev = "Bundle1",
                        Desc = "Roadside Assistance, Rental Car Reimbursement, Trip Interruption.",
                        CovOption = "False",
                        CovMessage = "",
                        CovInputType = "bivalue",
                        Purchased = "false",
                        FAQText = "",
                        IsForceEditOnChange = "False"
                    },
	                new {
                        CovCode = "Bundle2", 
                        QuoteProperty = "Bundle2Test",
                        WebQuestionID = "POLCOV_BUNDLE2TEST",
                        Name = "Protect Your Wallet",
                        SuppressRendering = "False",
                        VehIndex = "-1",
                        HelpText = "",
                        Invalid = "False",
                        Offered = "True",
                        Abbrev = "Bundle2",
                        Desc = "Avoid unwanted dents to your wallet with Accident Forgiveness, Renewal Assurance&amp;trade; and Disappearing Deductible.",
                        CovOption = "False",
                        CovMessage = "",
                        CovInputType = "bivalue",
                        Purchased = "false",
                        FAQText = "",
                        IsForceEditOnChange = "False"
                    },
	                new {
                        CovCode = "Bundle3", 
                        QuoteProperty = "Bundle3Test",
                        WebQuestionID = "POLCOV_BUNDLE3TEST",
                        Name = "Protect Your Family",
                        SuppressRendering = "False",
                        VehIndex = "-1",
                        HelpText = "",
                        Invalid = "False",
                        Offered = "True",
                        Abbrev = "Bundle3",
                        Desc = "Enhanced Car Seat Replacement, Pet Protection, Dependent Protection.",
                        CovOption = "False",
                        CovMessage = "",
                        CovInputType = "bivalue",
                        Purchased = "false",
                        FAQText = "",
                        IsForceEditOnChange = "False"
                    }
                };
                foreach (var cov in enCovs)
                {
                    result += "<EnhancedCoverage>";
                    result += "<CovCode>" + cov.CovCode + "</CovCode>";
                    result += "<QuoteProperty>" + cov.QuoteProperty + "</QuoteProperty>";
                    result += "<WebQuestionID>" + cov.WebQuestionID + "</WebQuestionID>";
                    result += "<Name>" + cov.Name + "</Name>";
                    result += "<SuppressRendering>" + cov.SuppressRendering + "</SuppressRendering>";
                    result += "<VehIndex>" + cov.VehIndex + "</VehIndex>";
                    result += "<Invalid>" + cov.Invalid + "</Invalid>";
                    result += "<Offered>" + cov.Offered + "</Offered>";
                    result += "<Abbrev>" + cov.Abbrev + "</Abbrev>";
                    result += "<Desc>" + cov.Desc + "</Desc>";
                    result += "<CovOption>" + cov.CovOption + "</CovOption>";
                    result += "<CovMessage>" + cov.CovMessage + "</CovMessage>";
                    result += "<CovInputType>" + cov.CovInputType + "</CovInputType>";
                    result += "<Purchased>" + cov.Purchased + "</Purchased>";
                    result += "<FAQText>" + cov.FAQText + "</FAQText>";
                    result += "<IsForceEditOnChange>" + cov.IsForceEditOnChange + "</IsForceEditOnChange>";
                    result += "</EnhancedCoverage>";
                }
                result += "</EnhancedCoverages>";
            }
            result += "</Coverages>";

            return result;
        }
        public string GetPolicyPremiums( AutoQuote.Autoquote quote)
        {
		    string Premiums = "<PolicyPremiums>";
            
            var polCovs = new []{
	            new {CoverageCode = "Bi", Prem = 0},
	            new {CoverageCode = "Pd", Prem = 0},
	            new {CoverageCode = "MedPay", Prem = 0},
	            new {CoverageCode = "Umbi", Prem = 0},
	            new {CoverageCode = "Umpd", Prem = 0},
	            new {CoverageCode = "UmpdOption", Prem = 0},
	            new {CoverageCode = "UmType", Prem = 0}
            };

            foreach (var cov in polCovs)
            {
	            double prem = 0;
     	        for (int i = 0; i < quote.getCoverages().item(0).getSixMonthPremiums().count(); i++)
	            {
		            AutoQuote.SixMonthPremium sixMoPrem = quote.getCoverages().item(0).getSixMonthPremiums().item(i);
         	        switch (cov.CoverageCode)
                    {
                        case "Bi":
                            prem += sixMoPrem.getSmBiPrem();
                            break;
                        case "Pd":
                            prem += sixMoPrem.getSmPdSlPrem();
                            break;
                        case "MedPay":
                            prem += sixMoPrem.getSmMedPayPrem();
                            break;
                        case "Umbi":
                            prem += sixMoPrem.getSmUmBiPrem();
                        
                            break;
                    
                        case "Uimbi":
                            prem += sixMoPrem.getSmUimBiPrem();
                            break;
                        
                        case "UmpdOption":
                        case "Umpd":
                      
                                prem += sixMoPrem.getSmUmPdPrem();
                            break;
                        case "Uimpd":
                            prem += sixMoPrem.getSmUimPdPrem();
                            break;
                        case "Mcca":
                            //prem += sixMoPrem.getSmMultiCarPrem() ;
                            prem += sixMoPrem.getSmCatClaimsPrem(); 
                            break;
                        case "PipPackage":
                            prem += sixMoPrem.getSmPipPrem();
                            break;
                        case "PipWorkLossExclusion":
                            //if (state == "KY")
                                prem += sixMoPrem.getSmPipPrem();
                            break;
                        case "PolicySetupFee":
                            prem += quote.getPolicyInfo().getPolicyFee();
                            break;
                        case "PipNonMed":
                           // if (state == "PA" || state == "MD")
                                prem += sixMoPrem.getSmPipPrem();
                            break;
                        case "PipExtended":
                            prem += sixMoPrem.getSmPipPrem();
                            break;
                        //jrenz #4423 3/15/2007 FL Cat Fund 
                        case "OptionFee1":
                            prem = quote.getPolicyInfo().getOptionFee1();
                            break;
                        case "PIPFPB":
                        case "PIPCombined":
                            prem += sixMoPrem.getSmPipPrem();
                            break;
                        case "PDLiability":
                            //prem += sixMoPrem.getSmPdSlPrem();  
                            prem += sixMoPrem.getSmPdSlPrem();
                            break;
                    }    
	            }
	            Premiums += "<Premium><CovCode>" + cov.CoverageCode + "</CovCode><Amount>" + prem + "</Amount></Premium>";
            }
            Premiums += "</PolicyPremiums>";
            return Premiums;
        }//theQuote.getCoverages().item(0).getPolicyCoverage()

        public string GetEnhancedCovPremiums(AutoQuote.Autoquote quote)
        {

            string Premiums = "<EnhancedPremiums>";
            var enCovs = new[]{
	            new {Name = "Protect Your Sanity", Prem = quote.getCoverages().item(0).getPolicyCoverage().getBundle1Prem()},
	            new {Name = "Protect Your Wallet", Prem = quote.getCoverages().item(0).getPolicyCoverage().getBundle2Prem()},
	            new {Name = "Protect Your Family", Prem = quote.getCoverages().item(0).getPolicyCoverage().getBundle3Prem()}
            };
            foreach (var cov in enCovs)
            {

                Premiums += "<Premium><Name>" + cov.Name + "</Name><Amount>" + cov.Prem + "</Amount></Premium>";
            }
            Premiums += "</EnhancedPremiums>";
            return Premiums;
        }

        public string GetVehicleDetailsAndOrPremiums(AutoQuote.Autoquote quote)
        {

            string Premiums = "<VehiclePremiums>";
            var vehCovs = new[]{
	            new {CoverageCode = "OtcDed", Prem = 0},
	            new {CoverageCode = "CollDed", Prem = 0},
	            new {CoverageCode = "MedPay", Prem = 0},
	            new {CoverageCode = "Towing", Prem = 0},
	            new {CoverageCode = "Transportation", Prem = 0},
	            new {CoverageCode = "CustEquip", Prem = 0}
            };

            for (int i = 0; i < quote.getCoverages().item(0).getSixMonthPremiums().count(); i++)
            {
                AutoQuote.SixMonthPremium sixMoPrem = quote.getCoverages().item(0).getSixMonthPremiums().item(i);
                Premiums += "<Vehicle>";
                Premiums += "<VehIndex>" + i.ToString() + "</VehIndex>";
                foreach (var cov in vehCovs)
                {
                    double prem = 0;
                    switch (cov.CoverageCode)
                    {
                        case "OtcDed":
                            prem = sixMoPrem.getSmCompPrem();
                            break;
                        case "CollDed":
                            prem = sixMoPrem.getSmCollPrem();
                            break;
                        case "MedPay":
                            prem = sixMoPrem.getSmMedPayPrem();
                            break;
                        case "Towing":
                            prem = sixMoPrem.getSmTowAndLaborPrem();
                            break;
                        case "Transportation":
                            prem = sixMoPrem.getSmRentalPrem();
                            break;
                        case "CustEquip":
                            prem = sixMoPrem.getSmSoundReproPrem();
                            break;
                    }
                    Premiums += "<Premium><CovCode>" + cov.CoverageCode + "</CovCode><Amount>" + prem + "</Amount></Premium>";

                }
                Premiums += "</Vehicle>";

            }
            Premiums += "</VehiclePremiums>";
            return Premiums;
        }

        private string GetCalculatedPremiums(AutoQuote.Autoquote quote, ref string policPremium)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(policPremium);

            XmlNodeList CoverageNodes = null;
            string CovInputType = "";
            string covPrem = "";
            double biCovPrem = 0;
            double AntiTheftFee = 0;
            AutoQuote.SixMonthPremiums prems6mo = quote.getCoverages().item(0).getSixMonthPremiums();
            double TotalFees = quote.getPolicyInfo().getPolicyFee() + quote.getPolicyInfo().getOptionFee2() + quote.getPolicyInfo().getOptionFee3() + quote.getPolicyInfo().getSr22FilingFee();
            double downpaymentFee = quote.getPolicyInfo().getOptionFee1();
            double TotalPremium = prems6mo.getSmTotalPolPrem() + TotalFees;
            //ysang may don't include TotalFees for ud.com, this is for Trac
            //double TotalPremium = prems6mo.getSmTBasePrem() + TotalFees + downpaymentFee;

            //fcaglar SSR7102 02-10-2011 - CA new quote flow
            if (quote.getCustomer().getAddressStateCode() == "CA")
            {
                TotalPremium = prems6mo.getSmTotalPolPrem() + TotalFees + downpaymentFee;
            }

            //add ploicyFee into BI prem
            CoverageNodes = xml.SelectNodes("PolicyPremiums/Premium");
            for (int i = 0; i < CoverageNodes.Count; i++)
            {
                CovInputType = CoverageNodes[i].SelectSingleNode("CovCode").InnerText;
                if (CovInputType == "Bi")
                {
                    covPrem = CoverageNodes[i].SelectSingleNode("Amount").InnerText;
                    if (covPrem != "")
                    {
                        biCovPrem = Convert.ToDouble(covPrem) + quote.getPolicyInfo().getPolicyFee() + AntiTheftFee;
                        CoverageNodes[i].SelectSingleNode("Amount").InnerText = biCovPrem.ToString();
                        //UDILibrary.StaticUtilities.XmlUtility.SetItemValue(CoverageNodes[i].SelectSingleNode("CovCode"), "Amount", biCovPrem.ToString());
                        break;
                    }
                }
            }
            policPremium = xml.OuterXml;
            //Note: This method may be returning additional information such as fees
            System.Text.StringBuilder sb = new StringBuilder();
            sb.Append("<CalculatedPremiums>");



            //ysang may need to add enhance coverage prem
            string Premium = "";
            if (TotalPremium > 0)
            {
                Premium = TotalPremium.ToString();
            }

            sb.Append("<Amount>" + Premium + "</Amount>");
            sb.Append("<Fees>" + TotalFees.ToString() + "</Fees>");
            sb.Append("<DownpaymentFees>" + downpaymentFee.ToString() + "</DownpaymentFees>");
            sb.Append("</CalculatedPremiums>");
            return sb.ToString();
        }

        private string GetDiscounts(ref AutoQuote.Autoquote quote, ref string state, ref string SalesPhoneNumber, bool x)
        {
            XmlDocument xmlDiscounts = new XmlDocument();
            xmlDiscounts.Load(ConfigurationManager.AppSettings["PluginsPath"] + "\\xsl\\Discounts.xml");
            XmlDocument xmlStateDiscounts = new XmlDocument();
            XmlNode discountNode = xmlDiscounts.SelectSingleNode("//State[@abbrev = '" + state + "']");
            //ysang add try cache 9/22/2011
            try
            {
                //fcaglar SSR07102 02/14/2011
                if (discountNode == null)
                {
                    xmlStateDiscounts.LoadXml(xmlDiscounts.SelectSingleNode("//State[@abbrev = 'default']").OuterXml);
                }
                else
                {
                    xmlStateDiscounts.LoadXml(discountNode.OuterXml);
                }


                //evaluate MultiPolicy Discounts for HO and Renters
                WebDiscount webDiscounts = new WebDiscount();
                //udinzs ssr8575
                webDiscounts.LoadWebDiscountFromQuote(quote);

                //Need to determine Renters vs HomeOwners
                //quote.getCustomer().getRentOwnTest()
                //CustomerHomeowner = 1,CustomerRenter = 0,CustomerOptionEntry = 4
                //update sub_header using {multi_placeholder}
                XmlNode xNodeMultiPolicy = xmlStateDiscounts.SelectSingleNode("//Discount[@display[contains(.,'MultPolicyDiscount')]]");


                string sSubHeader = xNodeMultiPolicy.Attributes["sub_header"].Value;
                //UDILibrary.StaticUtilities.XmlUtility.GetAttributeValue(xNodeMultiPolicy, "sub_header");
                //ysang 6715 9/15/2010           
                string sFaq = xNodeMultiPolicy.Attributes["faq"].Value;
                //string sFaq = UDILibrary.StaticUtilities.XmlUtility.GetAttributeValue(xNodeMultiPolicy, "faq");
                string sDesc = xNodeMultiPolicy.Attributes["description"].Value;
                //string sDesc = UDILibrary.StaticUtilities.XmlUtility.GetAttributeValue(xNodeMultiPolicy, "description");

                //ysang 7123 2/28/2011          
                //string sRenterAuto = GetRenterAndAuto(addInfo);
                //double hompd = 36; //hard code here
                //string sRenterPolicy = GetHOPolicy(addInfo, ref hompd);
                //if (sRenterPolicy.Length > 0)
                //{
                //    //string sHoProduct = quote.getPolicyInfo().getHoProduct();
                //    //string sLOB =  DBManager.getInstance().GetLOBDescriptionById(Convert.ToInt32(sHoProduct));
                //    //ysang TST10547 7/13/2011 CA should get what we defined in discounts.xml
                //    if (state != "CA")
                //    {

                //        string sLOBID = quote.getPolicyInfo().getHoProduct();
                //        if (string.IsNullOrEmpty(sLOBID))
                //            sLOBID = "2";
                //        string sLOB = DBManager.getInstance().GetLOBDescriptionById(Convert.ToInt32(sLOBID));
                //        sFaq = sFaq.Replace("{multi_policy_discount_display}", Constants.HO_MSG.Replace("{LOB}", sLOB));
                //        //this will go to phase II
                //        // sFaq = sFaq.Replace("{$XXX}", hompd.ToString("C"));
                //        UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMultiPolicy, "faq", "");
                //        UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMultiPolicy, "sub_header", sFaq);
                //        addInfo.AddUpdateInfo("multipolicydiscount", "true"); //which will display on discount list page
                //    }
                //    else
                //    {
                //        //ysang 7584 7/20/2011
                //        UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMultiPolicy, "CanBeDeleted", "fasle");
                //        addInfo.AddUpdateInfo("multipolicydiscount", "true"); //which will display on discount list page
                //    }
                //}
                //else if (sRenterAuto.Length > 0)
                //{
                //    addInfo.AddUpdateInfo("multipolicydiscount", "true");
                //    sFaq = sFaq.Replace("{multi_policy_discount_display}", Constants.RENTAUTO_MSG);
                //    //UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMultiPolicy, "description", sFaq);
                //    UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMultiPolicy, "sub_header", sFaq);
                //    UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMultiPolicy, "faq", "");
                //    // <CanBeDeleted>true</CanBeDeleted>
                //    //UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMultiPolicy, "CanBeDeleted", sFaq);
                //}
                //else
                //{

                    if (quote.getCustomer().getRentOwnTest() != 1) //Renter or Other
                    {
                        sSubHeader = sSubHeader.Replace("{multi_placeholder}", ConfigurationManager.AppSettings["MultiRenter"]);
                        xNodeMultiPolicy.Attributes["sub_header"].Value = sSubHeader.ToString();
                        //UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMultiPolicy, "sub_header", sSubHeader.ToString());
                        //ysang TST08314 6842 10/22/2010                
                        sFaq = sFaq.Replace("{multi_policy_discount_display}", Constants.RENTER_MSG.Replace("{CompanyName}", "Kemper Direct"));
                        xNodeMultiPolicy.Attributes["faq"].Value = sFaq.ToString();
                        //UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMultiPolicy, "faq", sFaq.ToString());
                        //UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMultiPolicy, "description", sFaq.ToString());
                    }
                    else //HomeOwner
                    {
                        sSubHeader = sSubHeader.Replace("{multi_placeholder}", ConfigurationManager.AppSettings["MultiHO"]);
                        xNodeMultiPolicy.Attributes["sub_header"].Value = sSubHeader.ToString();
                        //UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMultiPolicy, "sub_header", sSubHeader.ToString());
                        //ysang TST08314 6842 10/22/2010 
                        sFaq = sFaq.Replace("{multi_policy_discount_display}", Constants.HOMEOWNER_MSG.Replace("{CompanyName}", "Kemper Direct") + " 1-" + SalesPhoneNumber + ".");
                        xNodeMultiPolicy.Attributes["faq"].Value = sFaq.ToString();
                        //UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMultiPolicy, "faq", sFaq.ToString());
                        //UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMultiPolicy, "description", sFaq.ToString());

                        //ApartmentCondo = 1,MobileHome = 2,HouseOther =3
                        //if (quote.getCoverages().item(0).getPolicyCoverage().getHomeownershipType() == 2)
                        //{
                        //    UDILibrary.StaticUtilities.XmlUtility.Delete(xmlStateDiscounts, "//Discount[@display[contains(.,'MultPolicyDiscount')]]");
                        //}
                        //else
                        //{
                        //    if (ConfigurationManager.AppSettings["ExcludeMPD4HOStates"].IndexOf(state) != -1)
                        //        UDILibrary.StaticUtilities.XmlUtility.Delete(xmlStateDiscounts, "//Discount[@display[contains(.,'MultPolicyDiscount')]]");

                        //}
                    }
                //}


                //ysang 6715 and fixed prod 
                XmlNode xNodeRetory = xmlStateDiscounts.SelectSingleNode("//Discount[@display[contains(.,'RetroLoyaltyDiscount')]]");
                if (xNodeRetory != null)
                {
                    string sD = xNodeRetory.Attributes["description"].Value;
                    //string sD = UDILibrary.StaticUtilities.XmlUtility.GetAttributeValue(xNodeRetory, "description");
                    sD = sD.Replace("{CompanyName}", "Kemper Direct");
                    xNodeRetory.Attributes["description"].Value = sD;
                    //UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeRetory, "description", sD);
                }

                //udinzs ssr7409 new discount
                XmlNode xNodeWelcome = xmlStateDiscounts.SelectSingleNode("//Discount[@display[contains(.,'WelcomeBackDiscount')]]");
                if (xNodeWelcome != null)
                {
                    string sSH = xNodeWelcome.Attributes["sub_header"].Value;
                    //string sSH = UDILibrary.StaticUtilities.XmlUtility.GetAttributeValue(xNodeWelcome, "sub_header");
                    sSH = sSH.Replace("{CompanyName}", "Kemper Driect");
                    xNodeWelcome.Attributes["sub_header"].Value = sSH;
                    //UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeWelcome, "sub_header", sSH);
                }
                //udinzs ssr7409 update continous coverage
                XmlNode xNodeContinuous = xmlStateDiscounts.SelectSingleNode("//Discount[@display[contains(.,'ContinuousDiscount')]]");
                if (xNodeContinuous != null)
                {
                    string sSHCont = xNodeContinuous.Attributes["sub_header"].Value;
                    //string sSHCont = UDILibrary.StaticUtilities.XmlUtility.GetAttributeValue(xNodeContinuous, "sub_header");
                    int iBrand = quote.getPolicyInfo().getMarketBrand();
                    switch (iBrand)
                    {
                        case 2:
                            break;
                        default:
                            sSHCont = sSHCont.Replace("Responsibility alert!", "Being responsible pays off!");
                            xNodeContinuous.Attributes["sub_header"].Value = sSHCont;
                            //UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeContinuous, "sub_header", sSHCont);
                            break;
                    }
                }

                //Need to set iMingle description text based on Active Policy Connections.
                //update faq and description using {iMingleFaq} and {iMingleDescription}
                //dmetz 12-14-2010 #7401 - Adding {iMingleSubHeader}
                string sSub = "";
                XmlNode xNodeMingleMate = xmlStateDiscounts.SelectSingleNode("//Discount[@display[contains(.,'MingleMateDiscount')]]");
                if (xNodeMingleMate != null)
                {
                    sFaq = xNodeMingleMate.Attributes["description"].Value;
                    //sFaq = UDILibrary.StaticUtilities.XmlUtility.GetAttributeValue(xNodeMingleMate, "faq");
                    sDesc = xNodeMingleMate.Attributes["description"].Value;
                    //sDesc = UDILibrary.StaticUtilities.XmlUtility.GetAttributeValue(xNodeMingleMate, "description");
                    sSub = xNodeMingleMate.Attributes["sub_header"].Value;
                    //string sSub = UDILibrary.StaticUtilities.XmlUtility.GetAttributeValue(xNodeMingleMate, "sub_header");
                    //dmetz 11-14-2011 SSR7537 - Add Network Discount to all Brands
                    string BrandName = "Kemper Direct";
                    string DiscountName = GetDiscountName(quote);
                    string iMingleDescrActive = ConfigurationManager.AppSettings["iMingleDescrActive"];
                    string iMingleSubHeaderActive = ConfigurationManager.AppSettings["iMingleSubHeaderActive"];
                    string iMingleDescrNotActive_faq = ConfigurationManager.AppSettings["iMingleDescrNotActive_faq"];
                    string iMingleDescrNotActive_description = ConfigurationManager.AppSettings["iMingleDescrNotActive_description"];
                    string iMingleSubHeaderNotActive = ConfigurationManager.AppSettings["iMingleSubHeaderNotActive"];
                    //dmetz 01-06-2012 SSR7537/TST12561 - Add Network Discount to all Brands
                    string NonIMingleDescrActive = ConfigurationManager.AppSettings["NonIMingleDescrActive"];

                    //dmetz 01-11-2012 SSR7537/TST12594 - Network Discount should always appear for referral quotes
                    //if (Connected && Referrer != String.Empty) //There is an active policy connected to this one.
                    //if (Referrer != String.Empty) //There is a policy connected to this one.
                    //{
                    //    //SSR7537 WLU 12/21/2011
                    //    XElement member = GetReferrerInfo(ref Referrer, quote.getPolicyInfo().getMarketBrand());

                    //    //dmetz 11-14-2011 SSR7537 - Add Network Discount to all Brands
                    //    iMingleSubHeaderActive = iMingleSubHeaderActive.Replace("{DiscountName}", DiscountName);
                    //    //dmetz 01-06-2012 SSR7537/TST12561 - Add Network Discount to all Brands
                    //    if (quote.getPolicyInfo().getMarketBrand() != 2)
                    //    {
                    //        sFaq = sFaq.Replace("{iMingleFaq}", NonIMingleDescrActive);
                    //        sDesc = sDesc.Replace("{iMingleDescription}", NonIMingleDescrActive);
                    //    }
                    //    else
                    //    {
                    //        sFaq = sFaq.Replace("{iMingleFaq}", iMingleDescrActive);
                    //        sDesc = sDesc.Replace("{iMingleDescription}", iMingleDescrActive);
                    //    }
                    //    sSub = sSub.Replace("{iMingleSubHeader}", iMingleSubHeaderActive);
                    //    sFaq = sFaq.Replace("{John Doe}", member.Element("MemberFirstName").Value.Trim().ToLower().UCFirst() + " " + member.Element("MemberLastName").Value.Trim().ToLower().UCFirst());
                    //    sDesc = sDesc.Replace("{John Doe}", member.Element("MemberFirstName").Value.Trim().ToLower().UCFirst() + " " + member.Element("MemberLastName").Value.Trim().ToLower().UCFirst());
                    //    sSub = sSub.Replace("{John Doe}", member.Element("MemberFirstName").Value.Trim().ToLower().UCFirst() + " " + member.Element("MemberLastName").Value.Trim().ToLower().UCFirst());
                    //    UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMingleMate, "faq", sFaq.ToString());
                    //    UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMingleMate, "description", sDesc.ToString());
                    //    UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMingleMate, "sub_header", sSub.ToString());
                    //    UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMingleMate, "selectable", "false");
                    //}
                    //else //No Active policies connected to this one.
                    //{
                    //dmetz 11-14-2011 SSR7537 - Add Network Discount to all Brands
                    iMingleDescrNotActive_faq = iMingleDescrNotActive_faq.Replace("{Brand}", BrandName);
                    iMingleDescrNotActive_faq = iMingleDescrNotActive_faq.Replace("{DiscountName}", DiscountName);
                    iMingleDescrNotActive_description = iMingleDescrNotActive_description.Replace("{Brand}", BrandName);
                    iMingleSubHeaderNotActive = iMingleSubHeaderNotActive.Replace("{Brand}", BrandName);
                    sFaq = sFaq.Replace("{iMingleFaq}", iMingleDescrNotActive_faq);
                    sDesc = sDesc.Replace("{iMingleDescription}", iMingleDescrNotActive_description);
                    sSub = sSub.Replace("{iMingleSubHeader}", iMingleSubHeaderNotActive);
                    xNodeMingleMate.Attributes["faq"].Value = sFaq.ToString();
                    xNodeMingleMate.Attributes["description"].Value = sDesc.ToString();
                    xNodeMingleMate.Attributes["sub_header"].Value = sSub.ToString();
                    //UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMingleMate, "faq", sFaq.ToString());
                    //UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMingleMate, "description", sDesc.ToString());
                    //UDILibrary.StaticUtilities.XmlUtility.SetAttributeValue(xNodeMingleMate, "sub_header", sSub.ToString());
                    //}
                }
                //Match Discount to HashTables and set premium discount values 
                //prior to transformation
                XmlNodeList nodeList = xmlStateDiscounts.DocumentElement.ChildNodes;
                for (int i = nodeList.Count - 1; i >= 0; i--)
                {
                    if (webDiscounts.PolDiscountList.Contains(nodeList[i].Attributes["display"].Value.ToUpper()))
                    {
                        nodeList[i].Attributes["prem"].Value = Convert.ToString(webDiscounts.PolDiscountList[nodeList[i].Attributes["display"].Value.ToUpper()]);
                    }
                    else //Discount not in Policy Level list - check Vehicle list
                    {
                        if (webDiscounts.VehicleDiscountList.Contains(nodeList[i].Attributes["display"].Value.ToUpper()))
                        {
                            nodeList[i].Attributes["prem"].Value = Convert.ToString(webDiscounts.VehicleDiscountList[nodeList[i].Attributes["display"].Value.ToUpper()]);
                        }
                        else //Discount not in Vehicle list - check Driver List
                        {
                            if (webDiscounts.DriverDiscountList.Contains(nodeList[i].Attributes["display"].Value.ToUpper()))
                            {
                                nodeList[i].Attributes["prem"].Value = Convert.ToString(webDiscounts.DriverDiscountList[nodeList[i].Attributes["display"].Value.ToUpper()]);
                            }
                            else //Discount not in any list - need to remove it
                            {
                                nodeList[i].ParentNode.RemoveChild(nodeList[i]);
                            }
                        }
                    }
                }

                //Create the XsltArgumentList.
                //staylor: for now this parameter is not used but might in the future.
                XsltArgumentList xslArg = new XsltArgumentList();
                xslArg.AddParam("hidedisccov", "", "FALSE");
                //xslArg.AddParam("hidedisccov", "", bHideDiscCov.ToString().ToUpper());

                //XslTransform xslt = new XslTransform();
                XslCompiledTransform xslt2 = new XslCompiledTransform();
                xslt2.Load(ConfigurationManager.AppSettings["PluginsPath"] + "\\xsl\\DiscountsTrans.xslt");

                //xslt.Load(ConfigurationManager.AppSettings["PluginsPath"] + "\\xsl\\DiscountsTrans.xslt");
                XPathDocument mydata = new XPathDocument(new XmlTextReader(xmlStateDiscounts.OuterXml, XmlNodeType.Document, null));
                StringWriter sw = new StringWriter();

                //xslt.Transform(mydata, xslArg, sw);
                xslt2.Transform(mydata, xslArg, sw);

                return sw.ToString();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("[" + quote.getQuoteInfo().getQuoteNo0() + "] " + ex.Message,"AutoquoteLibrary","UD3Plugin","GetDiscounts");
               
                return string.Empty;
            }
        }
        private string GetDiscountName(AutoQuote.Autoquote quote)
        {
            int iBrand = quote.getPolicyInfo().getMarketBrand();
            switch (iBrand)
            {
                case 2:
                    return "iMingle Network Discount";
                case 3:
                    //dmetz 12-20-2011 SSR7537 - BA update
                    return "Teachers' Network™ Discount";
                default:
                    return "Network Discount";
            }

        }

        private string GetGeneralInfo(ref string guid, ref AutoQuote.Autoquote quote, ref string riskState, ref string SalesPhoneNumber, ref bool Connected, ref String Referrer, string sHOPolicy, string sRenterAuto, string sRedirect)
        {
            //Serial the quote so we have access to everything
            //string DRCXML = "<Quote>" + quote.serialize(UDILibrary.DRC.DRCFields.getInstance().GetDeserializeParams()) + "</Quote>";
            string DRCXML = "<Quote>" + quote.serialize(null) + "</Quote>";
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(DRCXML);

            System.Text.StringBuilder sb = new StringBuilder();

            //Customer
            sb.Append("<GeneralInfo>");
            sb.Append("<Customer>");
            sb.Append("<FirstName>" + quote.getCustomer().getFirstNameOfCustomer() + "</FirstName>");
            sb.Append("<LastName>" + quote.getCustomer().getLastNameOfCustomer() + "</LastName>");
            sb.Append("<MiddleInitial>" + quote.getCustomer().getMiddleInitial() + "</MiddleInitial>");
            sb.Append("<RentOwnTest>" + quote.getCustomer().getRentOwnTest().ToString() + "</RentOwnTest>");

            sb.Append("<IsHOOfferedInDRC>false</IsHOOfferedInDRC>");
            //sb.Append("<IsHOOfferedInDRC>" + DBManager.getInstance().GetOfferHomeOnwerInsState(quote.getCustomer().getMasterStateNo()).ToLower() + "</IsHOOfferedInDRC>");
            XmlNode node = xml.SelectSingleNode("Quote/Coverages/Coverage/PolicyCoverage/IpHomeownershipType");
            string HomeownershipType = "";
            if (node != null)
            {
                HomeownershipType = node.InnerText.ToString();
            }
            sb.Append("<HomeownershipType>" + HomeownershipType + "</HomeownershipType>");
            sb.Append("<NoOfDaysLapsed>" + quote.getPolicyInfo().getNoOfDaysLapsed().ToString() + "</NoOfDaysLapsed>");

            sb.Append("</Customer>");


            //SalesOffice
            //this is mostly a database lookup based on amfaccount#
            sb.Append("<SalesOffice>");

            string officeHours = String.Empty;
            //dmetz 09-16-2011 SSR7188/TST11371 - Teachers.com is Affinity but uses non-affinity hours
            //if (IsAffinity && clickThruID != 20000)
            //{
            //    officeHours = DBManager.getInstance().GetCallCenterHours("PFD", "SALES", "WEEK").Trim();
            //    officeHours = officeHours + "&lt;br&gt;" + DBManager.getInstance().GetCallCenterHours("PFD", "SALES", "WEEKEND").Trim();
            //}
            //else
            //{
                officeHours = "9:00am - 5:00pm Mon-Fri";
                //officeHours = officeHours + "&lt;br&gt;" + DBManager.getInstance().GetCallCenterHours("", "SALES", "WEEKEND").Trim();
            //}
            sb.Append("<OfficeHours>" + officeHours + "</OfficeHours>");
            sb.Append("<PhoneNumber>" + SalesPhoneNumber + "</PhoneNumber>");
            //tc #6773 08-18-2010 - HO Sales Phone Number
            //if (IsAffinity)
            //{
            //    sb.Append("<HOPhoneNumber>" + SalesPhoneNumber + "</HOPhoneNumber>");
            //}
            //else
            //{
                sb.Append("<HOPhoneNumber>800-555-1313</HOPhoneNumber>");
            //}
            sb.Append("</SalesOffice>");



            //Quote
            sb.Append("<Quote>");
            sb.Append("<RiskStateAbbreviation>" + riskState + "</RiskStateAbbreviation>");
            sb.Append("<IsAffinity>false</IsAffinity>");
            sb.Append("<IsAffinityAgent>false</IsAffinityAgent>");
            //udinzs PRD19537: Teachers Tenure discount advertised to non-educators           
            sb.Append("<AffinityNo>" + quote.getPolicyInfo().getAffinityNo() + "</AffinityNo>");
            //fcaglar 01-26-2011 SSR07429 - LeasingDesk
            sb.Append("<CTID>10160</CTID>");
            sb.Append("<IsIvanka>1</IsIvanka>");
            //tc #6832 08/31/2011 - New Bind Flow
            //tc #8250 11/29/2011 - Enable Affinity
            //dn #8250 12/08/2011 - Enable Teachers
            sb.Append("<IsNewBind>1</IsNewBind>");
            //tc #6773 08/03/2010 - IsPortal
            String isPortal = "false";
            sb.Append("<IsPortal>" + isPortal + "</IsPortal>");
            sb.Append("<MarketBrand>" + quote.getPolicyInfo().getMarketBrand() + "</MarketBrand>");
            sb.Append("<QuoteNumber>" + quote.getQuoteInfo().getQuoteNo0() + "</QuoteNumber>");
            sb.Append("<QuoteHashCode>" + quote.getQuoteInfo().GetHashCode() + "</QuoteHashCode>");
            //SSR8454 jrenz 5/15/2012 WI, append AmfAccount, InstallmentFees
            sb.Append("<IpAmfAccountNo>" + quote.getPolicyInfo().getAmfAccountNo() + "</IpAmfAccountNo>");
            sb.Append(GetInstallmentFees(quote.getCustomer().getAddressStateNo(), quote.getPolicyInfo().getAmfAccountNo(), quote.getPolicyInfo().getQuoteEffDate(), 3));
            sb.Append("</Quote>");
            //tc #6823 - 09-15-2010
            sb.Append("<Network>");
            sb.Append("<Connected>" + Connected.ToString().ToLower() + "</Connected>");
            sb.Append("<Referral>");
            sb.Append("<Referrer>" + Referrer + "</Referrer>");

            if (Referrer.Length > 0)
            {
                //SSR7537 WLU
               // XElement member = GetReferrerInfo(ref Referrer, quote.getPolicyInfo().getMarketBrand());
                sb.Append("<FirstName>Joe</FirstName>");
                sb.Append("<LastName>Mahem</LastName>");
            }

            sb.Append("</Referral>");
            sb.Append("</Network>");
            //ysang 7123 3/21/2011
            if (sHOPolicy.Length > 0)
            {
                sb.Append("<HOPolicy>");
                sb.Append("<Policy>" + sHOPolicy + "</Policy>");
                sb.Append("</HOPolicy>");
            }
            if (sRenterAuto.Length > 0 && sRenterAuto == "YES")
            {
                sb.Append("<RenterAndAuto>YES</RenterAndAuto>");
            }
            //end 7123
            //tc #8250 12-27-2011 - Redirect
            if (sRedirect.Length > 0)
            {
                sb.Append("<Redirect>" + sRedirect + "</Redirect>");
            }
            sb.Append("</GeneralInfo>");

            return sb.ToString();
        }

        private string GetInstallmentFees(int iStateCode, long lAccount, DateTime effDate, int payPlanType)
        {
            int iInstallFeeDirect = 6;
            int iInstallFeeCC = 5;
            int iInstallFeeEFT = 2;

            return "<InstallmentFees>" +
               "<InstallFeeDirect>" + iInstallFeeDirect.ToString() + "</InstallFeeDirect>" +
               "<InstallFeeCC>" + iInstallFeeCC.ToString() + "</InstallFeeCC>" +
               "<InstallFeeEFT>" + iInstallFeeEFT.ToString() + "</InstallFeeEFT>" +
               "</InstallmentFees>";

        }
        private string GetPayPlans(ref AutoQuote.Autoquote quote, ref string drcXML)
        {
            XmlDocument DRCxml = new XmlDocument();
            DRCxml.LoadXml(drcXML);

            //fcaglar SSR07102 02-15-2011 - CA new quote flow
            string state = quote.getCustomer().getAddressStateCode();

            XmlDocument xmlPayPlans = new XmlDocument();
            //fcaglar SSR07102 02-15-2011 - CA new quote flow
            if (state == "CA")
            {
                xmlPayPlans.Load(ConfigurationManager.AppSettings["PluginsPath"] + "\\xsl\\NonVibePayPlans.xml");
                //wsun prd19638 if no pay roll delete payplanid=6 
                //wsun 3/5/2012  PRD20328/tst13050, should remove payplanid=0
                // if(quote.getDownPayOptions().count()<6) //or check if Pay roll in the list
                if (DRCxml.SelectSingleNode("//PayRollDeductAcct") != null && DRCxml.SelectSingleNode("//PayRollDeductAcct").InnerText == "2")
                {
                    xmlPayPlans.RemoveChild(xmlPayPlans.SelectSingleNode("//PayPlan[@id='0']"));
                    //UDILibrary.StaticUtilities.XmlUtility.Delete(xmlPayPlans, "//PayPlan[@id='0']");
                    if (quote.getCustomer().getSpecialCorresNo() == 0)
                        quote.getCustomer().setSpecialCorresNo(6);

                }

            }
            //jrenz SSR09398 12/31/2014 kdquoteflow non vibe states
            else if (!ConfigurationManager.AppSettings["VibeState"].Contains(state))
            {
                xmlPayPlans.Load(ConfigurationManager.AppSettings["PluginsPath"] + "\\xsl\\NonVibePayPlans.xml");

                xmlPayPlans.Delete("//PayPlan[@id='0']");
                    
                if (quote.getCustomer().getSpecialCorresNo() == 0)
                    quote.getCustomer().setSpecialCorresNo(6);

                    
                //remove monthly for non-vibe non CA states
                xmlPayPlans.Delete("//PayPlan[@id='6']");
                
                xmlPayPlans.SelectSingleNode("//SelectedPayPlan").InnerText = "1"; // default to pif

            }
            else
            {
                xmlPayPlans.Load(ConfigurationManager.AppSettings["PluginsPath"] + "\\xsl\\PayPlans.xml");

                if (DRCxml.SelectSingleNode("//IpNoOfDaysLapsed") != null && ConfigurationManager.AppSettings["NonContinous"].IndexOf(DRCxml.SelectSingleNode("//IpNoOfDaysLapsed").InnerText) != -1)
                {
                   xmlPayPlans.Delete("//PayPlan[@id='7']");
                }

                //remove Payroll
                xmlPayPlans.Delete("//PayPlan[@id='3']");
                
                //dmetz 06-11-2012 SSR6873 - If 6 month term, delete Quarterly payplan
                if (DRCxml.SelectSingleNode("//IpTermFactor").InnerText == "0.5")
                {
                    xmlPayPlans.Delete( "//PayPlan[@id='4']");
                }
            }

            //now loop thru PayPlan Nodes and set the <PreferredPayerDiscount> values
            //from the Quote XML based on the PayPlan Node's drc_node attribute
            XmlDocument quoteXML = new XmlDocument();
            quoteXML.LoadXml("<Quote>" + quote.serialize(null) + "</Quote>");

            XmlNodeList xnlPayPlans = xmlPayPlans.SelectNodes("//PayPlan");
            foreach (XmlNode xNode in xnlPayPlans)
            {
                //fcaglar SSR07102 02-15-2011 - CA new quote flow
                //jrenz SSR09398 12/31/2014 kdquoteflow include non vibe states
                if (!ConfigurationManager.AppSettings["VibeState"].Contains(state))
                //if (state == "CA")
                {
                    //wsun PRD19638 , the item(0) is payroll, the installmentfee is 0
                    if (xNode.SelectSingleNode("Name").InnerText.IndexOf("Payroll", System.StringComparison.CurrentCultureIgnoreCase) > -1)
                    {
                        xNode.AppendNewChild("InstallmentFee").InnerText = quote.getDownPayOptions().item(0).getDpoInstfee().ToString();
                        //xNode.SelectSingleNode("//InstallmentFee").Value = quote.getDownPayOptions().item(0).getDpoInstfee().ToString();
                        //    UDILibrary.StaticUtilities.XmlUtility.SetItemValue(xNode, "InstallmentFee", quote.getDownPayOptions().item(0).getDpoInstfee().ToString());

                    }
                    else
                    {
                        xNode.AppendNewChild("InstallmentFee").InnerText = quote.getDownPayOptions().item(1).getDpoInstfee().ToString();
                        //xNode.SelectSingleNode("//InstallmentFee").Value = quote.getDownPayOptions().item(1).getDpoInstfee().ToString();
                        //UDILibrary.StaticUtilities.XmlUtility.SetItemValue(xNode, "InstallmentFee", quote.getDownPayOptions().item(1).getDpoInstfee().ToString());

                    }
                }
                else
                {
                    //AppendNewChild(xNode, "PreferredPayerDiscount").InnerText = quoteXML.SelectSingleNode("//" + xNode.Attributes["drc_node"].Value).InnerText;
                    //xNode.SelectSingleNode("//PreferredPayerDiscount").InnerText = quoteXML.SelectSingleNode("//" + xNode.Attributes["drc_node"].Value).InnerText;
                   xNode.SetItemValue("PreferredPayerDiscount", quoteXML.SelectSingleNode("//" + xNode.Attributes["drc_node"].Value).InnerText);
                }
            }

            return xmlPayPlans.DocumentElement.OuterXml;
        }

        public bool SaveDRCXMLtoWebSession(string guid, AddInfo addInfo, AutoQuote.Autoquote quote, string state, string errorKey)
        {
            //Save DRC_XML to WebSession
            string sql = String.Empty;
            string QuoteType = "standard"; // Depend on Quote.Save and Suspense

            try
            {
                //DRC XML with AddInfo Node
                //string drcXML = quote.serialize(UDILibrary.DRC.DRCFields.getInstance().GetDeserializeParams());
                string drcXML = quote.serialize(null);
                drcXML = "<DRCXML><RETURN><AiisQuoteMaster>" + drcXML + addInfo.Serialize() + "</AiisQuoteMaster></RETURN></DRCXML>";
                int dnq = 0;
                string sDnq = "";
                string dnqDesc = "";
                //tc #13341 08-27-2010 - Quote Email Being Sent for Incomplete Quotes
                string current_page = addInfo.CurrentPage == "" ? "CALLCENTERQ" : addInfo.CurrentPage;
                //string current_page = "CALLCENTERQ"; // "PACKAGERATING_FLEX" + state.ToUpper();

                if (current_page != "DiscountsPage")
                {
                    if (errorKey.Length > 0)
                    {
                        if (errorKey == "DNQ_QUOTE")
                        {
                            if (addInfo.DNQ.Reason != "")
                                dnq = Convert.ToUInt16(addInfo.DNQ.Reason);
                            dnqDesc = addInfo.DNQ.Description;
                            current_page = "DNQ";
                        }
                        else
                        {
                            current_page = "CALLCENTERQ";
                        }
                    }
                    else
                    {
                        current_page = "PACKAGERATING_FLEX" + state.ToUpper();
                    }
                }

                //FIELDS THAT CHANGE WHEN THE WEBSITE LOADS A QUOTE (from SS nag page to quote page)
                //drc_xml, current_page, err_count(?), err_details(?), 
                //queue_inuse, queue_complete, queue_start, queue_end, drc_end,
                //quote_number, csr_queue, quote_errmsg(?), uw_comp, 
                //oqpremium, oqpremiumfulltort, drc_premium, drc_premiumfulltort, 
                //dnq_description(?), form_complete, email_suppress,
                //last_save, key_code

                using (var context = new AutoQuoteEntitie7())
                {
                    Guid gGuid = Guid.Empty;
                    Guid.TryParse(guid, out gGuid);
                    var websession = from s in context.tbl_web_session
                                     where s.guid.Equals(gGuid)
                                     select s;
                    byte iDnq_template = 0;
                    byte.TryParse(addInfo.DNQ.Template, out iDnq_template);
                    byte iDnq_reason = 0;
                    byte.TryParse(addInfo.DNQ.Reason, out iDnq_reason);
                    byte iEmailSent = 0;
                    byte.TryParse(addInfo.DNQ.EmailSent, out iEmailSent);
                    int iCTID = 0;
                    int.TryParse(addInfo.ClickThruPartnerInfo.CTID, out iCTID);

                    if (websession.Count() == 1)
                    {
                        var rec = websession.First();
                        rec.guid = Guid.Parse(guid);
                        rec.drc_xml = drcXML;
                        rec.current_page = current_page;
                        rec.err_count = 0;
                        rec.err_details = quote.getGmr().errMsg == null ? "" : quote.getGmr().errMsg;
                        rec.last_save = DateTime.Now;
                        rec.queue_complete = 1;
                        rec.queue_start = DateTime.Now;
                        rec.queue_end = DateTime.Now;
                        rec.drc_end = DateTime.Now;
                        rec.quote_number = quote.getQuoteInfo().getQuoteNo0();
                        rec.knockout = addInfo.DNQ.Knockout;
                        rec.dnq_template = iDnq_template;
                        rec.dnq_reason = iDnq_reason;
                        rec.dnq_description = dnqDesc;
                        rec.dnq_email_sent = iEmailSent;
                        rec.csr_queue = addInfo.CSRQueue;
                        rec.quote_errmsg = quote.getGmr().errMsg == null ? "" : quote.getGmr().errMsg;
                        rec.clickthru_partner_id = iCTID;
                        rec.clickthru_custom = addInfo.ClickThruPartnerInfo.Custom;
                        rec.amend_status = quote.getQuoteInfo().getQuoteTransType();
                        rec.amf_account_no = addInfo.ClickThruPartnerInfo.AMFAccountNumber == "" ? (int)quote.getPolicyInfo().getAmfAccountNo() : int.Parse(addInfo.ClickThruPartnerInfo.AMFAccountNumber);
                        rec.quote_number = quote.getQuoteInfo().getQuoteNo0();
                        rec.orig_app = addInfo.Application;
                        rec.keywords = addInfo.ClickThruPartnerInfo.Keywords;
                        rec.email_suppress = quote.getCoverages().item(0).getSixMonthPremiums().getSmTotalPolPrem() != 0 && (quote.getCoverages().item(0).getSixMonthPremiums().getSmTotalPolPrem() < 250 || quote.getCoverages().item(0).getSixMonthPremiums().getSmTotalPolPrem() > 10000) ? (byte)0 : (byte)1;
                        rec.form_complete = 1;
                        rec.drc_premium = (int)quote.getCoverages().item(0).getSixMonthPremiums().getSmTotalPolPrem();
                        rec.drc_premiumfulltort = 0;
                        rec.uw_comp = (int)quote.getPolicyInfo().getUnderwritingCoNo();
                        context.SaveChanges();
                    }
                    else
                    {
                        //error
                    }
                }
                //ysang 7479
//                sql += @";INSERT INTO tbl_web_session_current_page(guid,current_page,save_date_time)
//		                VALUES(@guid,@current_page,getDate())";
//                sqlTextExe.ExecSQLTextReturnNoReturn(sql, sqlParams);

                return true;
            }
            catch (Exception e)
            {
                LogUtility.LogError(e.Message, "AutoQuoteFlow", "UD3Plugin", "SaveDRCXMLtoWebSession");
                return false;
            }

        }
        public bool SaveFlexXML2SessionFlex(string guid, string flexXML)
        {
            try
            {
                using (var context = new AutoQuoteEntitie7())
                {
                    Guid gGuid = Guid.Empty;
                    Guid.TryParse(guid, out gGuid);
                    var websession = from s in context.tbl_web_session_flex
                                     where s.guid.Equals(gGuid)
                                     select s;
                   
                    if (websession.Count() == 1)
                    {
                        var rec = websession.First();
                        rec.flex_xml = flexXML;
                    }
                    else
                    {
                        context.tbl_web_session_flex.Add(new tbl_web_session_flex
                        {
                            guid = gGuid,
                            flex_xml = flexXML
                        });
                    }
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception eSQL)
            {
                return false;
            }
        }
        public string GetXMLFromWebSession(string guid)
        {
            using (var context = new AutoQuoteEntitie7())
            {
                Guid gGuid = Guid.Empty;
                Guid.TryParse(guid, out gGuid);
                var websession = from s in context.tbl_web_session
                                 where s.guid.Equals(gGuid)
                                 select s;

                if (websession.Count() == 1)
                {
                    var rec = websession.First();
                    return rec.drc_xml;
                }
                else
                    return "";
            }
        }
        public string GetXMLFromWebSessionFlex(string guid)
        {
            using (var context = new AutoQuoteEntitie7())
            {
                Guid gGuid = Guid.Empty;
                Guid.TryParse(guid, out gGuid);
                var websession = from s in context.tbl_web_session_flex
                                 where s.guid.Equals(gGuid)
                                 select s;

                if (websession.Count() == 1)
                {
                    var rec = websession.First();
                    return rec.flex_xml;
                }
                else
                    return "";
            }
        }
       
        private string GetInstantRentersInfo(AutoQuote.Autoquote quote, string drcXML) //, AddInfo addinfo)
        {

            System.Text.StringBuilder sb = new StringBuilder();
            string state = quote.getCustomer().getAddressStateCode();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlDocument DRCxml = new XmlDocument();
                DRCxml.LoadXml(drcXML);
                //ysang prd19094 11/16/2011
                bool isEmptyLimit = false;
                /////////////////////////////////////////////////////////
                //ying for test
                string sAddProp = "";
                string sDrcProp = "";

                //11/11/2014 does drXML have addinfo in homeedition?
                XmlNode nodeAddInfo;
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(drcXML);
                nodeAddInfo = xml.SelectSingleNode("DRCXML/RETURN/AiisQuoteMaster/AddInfo");

                //string sAdd = addinfo.XML();
                string sAdd = nodeAddInfo.OuterXml.ToString();

                XmlDocument xmlAdd = new XmlDocument();
                xmlAdd.LoadXml(sAdd);
                XmlNodeList addNode = xmlAdd.SelectNodes("AddInfo/HOIRenterInfo");
                if (addNode[0] != null && addNode[0].SelectSingleNode("HOIRenterProperty") != null)
                {

                    sAddProp = addNode[0].SelectSingleNode("HOIRenterProperty").InnerXml;
                    if (sAddProp == "0" || sAddProp == "")
                        isEmptyLimit = true;
                }
                //ysang 7123
                string sRenterAuto = "";
                XmlNodeList addNode2 = xmlAdd.SelectNodes("AddInfo/ClickThruPartnerInfo");
                if (addNode2[0] != null && addNode2[0].SelectSingleNode("RenterAndAuto") != null)
                {
                    sRenterAuto = addNode2[0].SelectSingleNode("RenterAndAuto").InnerXml;
                }
                if (DRCxml.SelectSingleNode("//HOIRenterProperty") != null && DRCxml.SelectSingleNode("//HOIRenterProperty") != null)
                {
                    sDrcProp = DRCxml.SelectSingleNode("//HOIRenterProperty").InnerText;
                }
                //LogError.Write("ud3plugin", "AutoQuoteServices.getInstantRentersInfo", "sAddProp: " + sAddProp + " sDrcProp" + sDrcProp, 3);
                /////////////////////////////////////////////////////////
                if (DRCxml.SelectSingleNode("//HOIRenterProvide") != null && DRCxml.SelectSingleNode("//HOIRenterProvide").InnerText.ToUpper() == "YES")
                {
                    sb.Append("<InstantRenters>");

                    sb.Append("<HOIRenterInfo>");
                    sb.Append(DRCxml.SelectSingleNode("//HOIRenterProvide").OuterXml); // <HOIRenterProvide>
                    //If OTHER was chosen on the Household Page it is possible that this node is missing
                    //if so, we need to default it to a value of 5
                    if (DRCxml.SelectSingleNode("//HONoOfUnit") != null)
                    {
                        sb.Append(DRCxml.SelectSingleNode("//HONoOfUnit").OuterXml); // <HONoOfUnit>
                    }
                    else
                    {
                        sb.Append("<HONoOfUnit>5</HONoOfUnit>"); // <HONoOfUnit>
                    }
                    if (DRCxml.SelectSingleNode("//HOIRenterProperty") != null)
                    {
                        sb.Append(DRCxml.SelectSingleNode("//HOIRenterProperty").OuterXml); // <HONoOfUnit>
                        if (DRCxml.SelectSingleNode("//HOIRenterProperty").InnerText.Length > 1)
                        {
                            string sProp = DRCxml.SelectSingleNode("//HOIRenterProperty").InnerText;
                            quote.getPolicyInfo().setHoPropLimit1000(Convert.ToInt32(sProp));
                        }
                    }
                    else
                    {
                        sb.Append("<HOIRenterProperty></HOIRenterProperty>");
                    }
                    if (DRCxml.SelectSingleNode("//HOIRenterLiability") != null)
                    {
                        sb.Append(DRCxml.SelectSingleNode("//HOIRenterLiability").OuterXml); // 
                        if (DRCxml.SelectSingleNode("//HOIRenterLiability").InnerText.Length > 1)
                        {
                            string sLiab = DRCxml.SelectSingleNode("//HOIRenterLiability").InnerText;
                            quote.getPolicyInfo().setHoLiabLimit1000(Convert.ToInt32(sLiab));
                        }
                        else
                        {
                            isEmptyLimit = true;
                        }
                    }
                    else
                    {
                        sb.Append("<HOIRenterLiability></HOIRenterLiability>");
                    }
                    if (DRCxml.SelectSingleNode("//HOIRenterDeductible") != null)
                    {
                        sb.Append(DRCxml.SelectSingleNode("//HOIRenterDeductible").OuterXml); // 
                        if (DRCxml.SelectSingleNode("//HOIRenterDeductible").InnerText.Length > 1)
                        {
                            string sDed = DRCxml.SelectSingleNode("//HOIRenterDeductible").InnerText;
                            quote.getPolicyInfo().setHoDedLimit(Convert.ToInt32(sDed));
                        }
                        else
                        {
                            isEmptyLimit = true;
                        }

                    }
                    else
                    {
                        sb.Append("<HOIRenterDeductible></HOIRenterDeductible>");
                    }

                    if (DRCxml.SelectSingleNode("//HOIRenterInculded") != null)
                    {
                        sb.Append(DRCxml.SelectSingleNode("//HOIRenterInculded").OuterXml); // <HONoOfUnit>
                        quote.getPolicyInfo().setHoRentersInterestTest(1);
                        quote.getPolicyInfo().setHoProduct("2");
                    }
                    else
                    {   
                        if (sRenterAuto == "YES")
                            sb.Append("<HOIRenterInculded>1</HOIRenterInculded>");
                        else
                            sb.Append("<HOIRenterInculded></HOIRenterInculded>");
                    }


                    if (DRCxml.SelectSingleNode("//HOIRenterPremium") != null)
                    {
                        sb.Append(DRCxml.SelectSingleNode("//HOIRenterPremium").OuterXml);
                        if (DRCxml.SelectSingleNode("//HOIRenterPremium").InnerText.Length > 1)
                        {
                            string sPrem = DRCxml.SelectSingleNode("//HOIRenterPremium").InnerText;
                            quote.getPolicyInfo().setHoPremium(Convert.ToDouble(sPrem));
                        }
                        else
                        {
                            isEmptyLimit = true;
                        }
                    }
                    else
                    {
                        sb.Append("<HOIRenterPremium></HOIRenterPremium>");
                    }
                    
                    if (isEmptyLimit)
                        sb.Append("<HOIRenterInculded>0</HOIRenterInculded>");
                    

                    sb.Append("</HOIRenterInfo>");
                    sb.Append("</InstantRenters>");

                    xmlDoc.LoadXml(sb.ToString());

                    bool AccountPremierFlag = false;
                    if (xmlAdd.SelectSingleNode("AddInfo/AccountPremierFlag") != null && xmlAdd.SelectSingleNode("AddInfo/AccountPremierFlag").InnerXml != "")
                    {
                        AccountPremierFlag = Convert.ToBoolean(xmlAdd.SelectSingleNode("AddInfo/AccountPremierFlag").InnerXml);
                    }

                    string zipCode = quote.getCustomer().getZipCode1().ToString("00000");
                    DateTime effDt = quote.getPolicyInfo().getEffDate();
                    string effDate = effDt.ToString("MM/dd/yyyy");
                    string units = quote.getPolicyInfo().getHoUnitsInBldg().ToString();
                    XmlNode rentersRates = GetXMLZipRates(state);

                    XmlNode rentersCoverages = GetAvailableCovOptions(rentersRates, state);

                    if (ConfigurationManager.AppSettings["ImpGroupStates"].Contains(state))
                    {
                        int DefLiabilitySel;
                        int marketbrand = quote.getPolicyInfo().getMarketBrand();
                        int bioLimit = quote.getCustomer().getCurrentLimits();
                        if (AccountPremierFlag == true && state == "CA")
                            DefLiabilitySel = 100000;
                        else if ((marketbrand == 1) || ("CT/NJ/MI/PA/VA".IndexOf(state) == -1) || //These are states without animal liability exclustion
                            (bioLimit == 3 || bioLimit == 4)) //$100,000/300,000 and higher
                            DefLiabilitySel = 50000;
                        else
                            DefLiabilitySel = 25000;

                        if (DefLiabilitySel == 25000)
                        {
                            XmlNode nodeToDel = null;
                            foreach (XmlNode n in rentersCoverages.SelectNodes("//Liability"))
                            {
                                if (n.InnerText == "100000")
                                    nodeToDel = n;
                                if (n.InnerText == "50000")
                                    n.Attributes["default"].Value = "N";
                                if (n.InnerText == "25000")
                                    n.Attributes["default"].Value = "Y";
                            }

                            if (nodeToDel != null) //If default is 25000, remove 100,000
                                nodeToDel.ParentNode.RemoveChild(nodeToDel);

                        }
                        if (DefLiabilitySel == 100000)
                        {
                            foreach (XmlNode n in rentersCoverages.SelectNodes("//Liability"))
                            {
                                if (n.InnerText == "100000")
                                    n.Attributes["default"].Value = "Y";
                                if (n.InnerText == "50000")
                                    n.Attributes["default"].Value = "N";
                                if (n.InnerText == "25000")
                                    n.Attributes["default"].Value = "N";
                            }
                        }

                        //Change deduct default to 1000
                        foreach (XmlNode n in rentersCoverages.SelectNodes("//Deductible"))
                        {
                            if (state == "KS" || state == "TN" || state == "CT")
                            {
                                if (n.InnerText == "500")
                                    n.Attributes["default"].Value = "Y";
                            }
                            else
                            {
                                if (n.InnerText == "500")
                                    n.Attributes["default"].Value = "N";
                                if (n.InnerText == "1000")
                                    n.Attributes["default"].Value = "Y";
                            }
                        }
                    }

                    XmlNode test = xmlDoc.ImportNode(rentersCoverages, true);
                    xmlDoc.DocumentElement.AppendChild(test);

                    XmlNode rates = rentersRates.OwnerDocument.CreateNode(XmlNodeType.Element, "Rates", "");

                    XmlNodeList xList = rentersRates.ChildNodes;
                    foreach (XmlNode xNode in xList)
                    {
                        XmlNode xClone = xNode.Clone();
                        rates.AppendChild(xClone);
                    }

                    XmlNode test2 = xmlDoc.ImportNode(rates, true);
                    xmlDoc.DocumentElement["State"].AppendChild(test2);
                }
                else
                {
                    sb.Append("<InstantRenters>");

                    sb.Append("<HOIRenterInfo>");
                    sb.Append("<HOIRenterProvide>NO</HOIRenterProvide>");
                    sb.Append("</HOIRenterInfo>");
                    sb.Append("</InstantRenters>");

                    xmlDoc.LoadXml(sb.ToString());
                }
                return xmlDoc.OuterXml;
            }
            catch (Exception ex)
            {
                LogUtility.LogError(ex.Message, "AutoQuoteLibrary", "UD3Plugin", "GetInstantRentersInfo");
                return "";
            }
        }
        private XmlNode GetAvailableCovOptions(XmlNode rateXml, String state)
        {
            ArrayList PropertyList = new ArrayList();
            ArrayList LiabList = new ArrayList();
            ArrayList deductList = new ArrayList();
            StringBuilder sbP = new StringBuilder();
            StringBuilder sbL = new StringBuilder();
            StringBuilder sbD = new StringBuilder();
            XmlNodeList xList = rateXml.SelectNodes("//Rate");
            foreach (XmlNode xNode in xList)
            {
                String sp = xNode.Attributes["property"].Value;
                String sl = xNode.Attributes["liability"].Value;
                String sd = xNode.Attributes["deductible"].Value;
                if (!PropertyList.Contains(sp))
                {
                    if (sp == "25000")
                        sbP.Append("<Property default=\"Y\">" + sp + "</Property>");
                    else
                        sbP.Append("<Property default=\"N\">" + sp + "</Property>");
                    PropertyList.Add(sp);

                }
                if (!LiabList.Contains(sl))
                {
                    if (sl == "50000")
                        sbL.Append("<Liability default=\"Y\">" + sl + "</Liability>");
                    else
                        sbL.Append("<Liability default=\"N\">" + sl + "</Liability>");
                    LiabList.Add(sl);

                }
                if (!deductList.Contains(sd))
                {
                    sbD.Append("<Deductible default=\"N\">" + sd + "</Deductible>");
                    deductList.Add(sd);

                }
            }

            String SCovs = string.Format("<State statecode=\"{0}\"><Coverages>{1}{2}{3}</Coverages></State>", state, sbP.ToString(), sbL.ToString(), sbD.ToString());

            XmlDocument rateCoverages = new XmlDocument();
            rateCoverages.LoadXml(SCovs);
            XmlNode rootnode = rateCoverages.DocumentElement;

            return rootnode;
        }
        public XmlNode GetXMLZipRates(string state)
        {
            XmlNode result = null;
            try
            {
                XmlDocument xmlRenterRates = new XmlDocument();
                xmlRenterRates.Load(ConfigurationManager.AppSettings["PluginsPath"] + "\\xsl\\HO_StateRates.xml");
                result = xmlRenterRates.SelectSingleNode("//State[@statecode = '" + state + "']");
                return result;
            }
            catch (Exception ex)
            {
                LogUtility.LogError(ex.Message, "AutoQuoteLibrary", "UD3Plugin", "GetXMLZipRates");
            }

            return result;
        }
        public XElement RecalculateQuote(WebSession session)
        {

            //Get DRC XML
            string drcXML = "";

            using (var context = new AutoQuoteEntitie7())
            {
                var drc = from d in context.tbl_web_session
                          where d.guid.Equals(session.Guid)
                          select d;
                if (drc.Count() == 1)
                    drcXML = drc.First().drc_xml;

            }
            string state = session.Quote.getCustomer().getAddressStateCode();
            AddInfo addInfo = new AddInfo();
            addInfo = XElement.Parse("<response>" + drcXML + "</response>").Element("DRCXML").Element("RETURN").Element("AiisQuoteMaster").Element("AddInfo").ToString().Deserialize<AddInfo>();
            
            //string XMLCovUpdates = eXML.ToString();
            UpdateCoverages(session);
            //UpdateSelectedDiscounts(ref addInfo, ref eXML, ref quote);
            //UpdateSelectedPayPlan(ref addInfo, ref eXML, ref quote);

            flexSessionXML = GetXMLFromWebSessionFlex(guid);

            XElement resp = Rate(session.Quote, drcXML);

            //If error
            if (resp.Name.LocalName.ToString() == "ErrorInfo")
            {
                LogUtility.LogError(resp.ToString() + "guid=" + guid, "AutoQuoteLibrary", "UD3Plugin", "RatedSaveWDiscounts");
                throw new Exception("Guid-[" + guid + "]" + resp.ToString());
            }
            else //If successful
            {
                string drcXML1 = GetXMLFromWebSession(guid);
                flexSessionXML = GetXMLFromWebSessionFlex(guid);

                //Read discounts
                XElement flexSession = XElement.Parse(flexSessionXML);
                XElement discountXML = flexSession.Element("Discounts");

                //string xml = "<Response>" + drcXML1 + discountXML.ToString() + "</Response>";
                return flexSession;
            }

        }
    }
}