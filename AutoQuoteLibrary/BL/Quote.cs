using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using AutoQuote;


namespace AutoQuoteLibrary.BL
{
    public enum ReturnAndSaveDiscount{None=0,ComeBackandSave=1,WelcomeBack=2}
    public class Quote
    {
        protected Autoquote _aiisQuoteMaster = new Autoquote();
        protected XElement _addInfo = XElement.Load(ConfigurationManager.AppSettings["PluginsPath"] + "\\QuoteFlowData\\Quote\\AddInfo.xml");

        //private SessionDAO _sessionDAO = new SessionDAO();

        public Quote()
        {

        }

        public Autoquote AiisQuoteMaster
        {
            get
            {
                return _aiisQuoteMaster;
            }
        }

        public XElement AddInfo
        {
            get
            {
                return _addInfo;
            }
        }

        public XElement Serialize()
        {
            return XElement.Parse("<DRCXML><RETURN><AiisQuoteMaster>" + _aiisQuoteMaster.serialize(null) + _addInfo + "</AiisQuoteMaster></RETURN></DRCXML>");
        }

        public void Deserialize(XElement quote)
        {
            _aiisQuoteMaster.deserialize(quote.ToString(), null);

            foreach (XElement element in quote.Element("RETURN").Element("AiisQuoteMaster").Element("AddInfo").Elements())
            {
                XElement compare = AddInfo.Element(element.Name);

                if (compare != null)
                {
                    compare.ReplaceWith(element);
                }
                else
                {
                    AddInfo.Add(element);
                }
            }
        }

        //public void Recall(String quoteNumber)
        //{
        //    _aiisQuoteMaster.getQuoteInfo().setQuoteNo0(quoteNumber);

        //    if (!_sessionDAO.SelectSuspense(this))
        //    {
        //        _aiisQuoteMaster.retrieve();
        //    }
        //}

        public Boolean Load(String guid)
        {
            _addInfo.Element("Guid").Value = guid;
            //return _sessionDAO.SelectWebSession(this);
            using (var context = new AutoQuoteEntitie7())
            {
                Guid gGuid = Guid.Empty;
                Guid.TryParse(_addInfo.Element("Guid").Value, out gGuid);
                var websession = from s in context.tbl_web_session
                                 where s.guid.Equals(gGuid)
                                 select s;
                if (websession.Count() > 0)
                {
                    var rec = websession.First();
                    this.Deserialize(XElement.Parse(rec.guid.ToString()));
                    return true;
                }
            }
            return false;
        }

        public void Save()
        {
            //_sessionDAO.InsertUpdateWebSession(this);
            using (var context = new AutoQuoteEntitie7())
            {
                Guid guid = Guid.Empty;
                Guid.TryParse(_addInfo.Element("Guid").Value, out guid);
                var websession = from s in context.tbl_web_session 
                                 where s.guid.Equals(guid)
                                 select s;
                byte iDnq_template = 0;
                byte.TryParse(_addInfo.Element("DNQ").Element("Template").Value,out iDnq_template);
                byte iDnq_reason = 0;
                byte.TryParse(_addInfo.Element("DNQ").Element("Reason").Value,out iDnq_reason);
                byte iEmailSent = 0;
                byte.TryParse(_addInfo.Element("DNQ").Element("EmailSent").Value,out iEmailSent);
                int iCTID = 0;
                int.TryParse(_addInfo.Element("ClickThruPartnerInfo").Element("CTID").Value, out iCTID);
                    
                if (websession.Count() == 0)
                {
                   context.tbl_web_session.Add(new tbl_web_session
                    {
                        guid = Guid.Parse(_addInfo.Element("Guid").Value),
                        drc_xml = this.Serialize().ToString(SaveOptions.DisableFormatting),
                        current_page = _addInfo.Element("CurrentPage").Value,
                        email = _aiisQuoteMaster.getCustomer().getEMailAddress(),
                        err_count = (byte)_aiisQuoteMaster.getGmr().getGmrMessageRec().count(),
                        err_details = getQuoteErrorDetails(this),
                        knockout = _addInfo.Element("DNQ").Element("Knockout").Value,
                        dnq_template = iDnq_template,
                        dnq_reason = iDnq_reason,
                        dnq_description = _addInfo.Element("DNQ").Element("Description").Value,
                        dnq_email_sent = iEmailSent,
                        quote_errmsg = getQuoteErrorDetails(this),
                        csr_queue = _addInfo.Element("CSRQueue").Value,
                        clickthru_partner_id = iCTID,
                        clickthru_custom = _addInfo.Element("ClickThruPartnerInfo").Element("Custom").Value,
                        keycode = _addInfo.Element("ClickThruPartnerInfo").Element("MarketKeyCode").Value == "" ? _aiisQuoteMaster.getCustomer().getMarketKeyCode() : _addInfo.Element("ClickThruPartnerInfo").Element("MarketKeyCode").Value,
                        amend_status = _aiisQuoteMaster.getQuoteInfo().getQuoteTransType(),
                        amf_account_no = _addInfo.Element("ClickThruPartnerInfo").Element("AMFAccountNumber").Value == "" ? (int)_aiisQuoteMaster.getPolicyInfo().getAmfAccountNo() : int.Parse(_addInfo.Element("ClickThruPartnerInfo").Element("AMFAccountNumber").Value),
                        quote_number = _aiisQuoteMaster.getQuoteInfo().getQuoteNo0(),
                        state = _addInfo.Element("RiskState").Value.ToUpper() == "" ? _aiisQuoteMaster.getCustomer().getAddressStateCode() : _addInfo.Element("RiskState").Value.ToUpper(),
                        orig_app = _addInfo.Element("Application").Value,
                        keywords = _addInfo.Element("ClickThruPartnerInfo").Element("Keywords").Value,
                        last_save = DateTime.Now,
                        first_save = DateTime.Now,
                        uw_comp = 0,
                        referer_id = 888
                    });
                   context.SaveChanges();
                }
                else if (websession.Count() == 1)
                {
                    var rec = websession.First();
                    rec.guid = Guid.Parse(_addInfo.Element("Guid").Value);
                    rec.drc_xml = this.Serialize().ToString(SaveOptions.DisableFormatting);
                    rec.current_page = _addInfo.Element("CurrentPage").Value;
                    rec.email = _aiisQuoteMaster.getCustomer().getEMailAddress();
                    rec.err_count = (byte)_aiisQuoteMaster.getGmr().getGmrMessageRec().count();
                    rec.err_details = getQuoteErrorDetails(this);
                    rec.knockout = _addInfo.Element("DNQ").Element("Knockout").Value;
                    rec.dnq_template = iDnq_template;
                    rec.dnq_reason = iDnq_reason;
                    rec.dnq_description = _addInfo.Element("DNQ").Element("Description").Value;
                    rec.dnq_email_sent = iEmailSent;
                    rec.quote_errmsg = getQuoteErrorDetails(this);
                    rec.csr_queue = _addInfo.Element("CSRQueue").Value;
                    rec.clickthru_partner_id = iCTID;
                    rec.clickthru_custom = _addInfo.Element("ClickThruPartnerInfo").Element("Custom").Value;
                    rec.keycode = _addInfo.Element("ClickThruPartnerInfo").Element("MarketKeyCode").Value == "" ? _aiisQuoteMaster.getCustomer().getMarketKeyCode() : _addInfo.Element("ClickThruPartnerInfo").Element("MarketKeyCode").Value;
                    rec.amend_status = _aiisQuoteMaster.getQuoteInfo().getQuoteTransType();
                    rec.amf_account_no = _addInfo.Element("ClickThruPartnerInfo").Element("AMFAccountNumber").Value == "" ? (int)_aiisQuoteMaster.getPolicyInfo().getAmfAccountNo() : int.Parse(_addInfo.Element("ClickThruPartnerInfo").Element("AMFAccountNumber").Value);
                    rec.quote_number = _aiisQuoteMaster.getQuoteInfo().getQuoteNo0();
                    rec.state = _addInfo.Element("RiskState").Value.ToUpper() == "" ? _aiisQuoteMaster.getCustomer().getAddressStateCode() : _addInfo.Element("RiskState").Value.ToUpper();
                    rec.orig_app = _addInfo.Element("Application").Value;
                    rec.keywords = _addInfo.Element("ClickThruPartnerInfo").Element("Keywords").Value;
                    rec.last_save = DateTime.Now;
                    context.SaveChanges();
                }
                else
                {
                    //error
                }
            }
        }
        public String getQuoteErrorDetails(Quote quote)
        {
            String errors = String.Empty;

            for (Int32 i = 0; i < _aiisQuoteMaster.getGmr().getGmrMessageRec().count(); i++)
            {
                errors += _aiisQuoteMaster.getGmr().getGmrMessageRec().item(i).getGmrMessage();
            }

            return errors;
        }
            //    int iPolicyEffDate = 0;
            //    int.TryParse(_aiisQuoteMaster.getQuoteInfo().getPolicyEffDate().Year.ToString("0000") + _aiisQuoteMaster.getQuoteInfo().getPolicyEffDate().Month.ToString("00") + _aiisQuoteMaster.getQuoteInfo().getPolicyEffDate().Day.ToString("00"), out iPolicyEffDate);
            //    int iPolicyExpDate = 0;
            //    int.TryParse(_aiisQuoteMaster.getQuoteInfo().getPolicyExpDate().Year.ToString("0000") + _aiisQuoteMaster.getQuoteInfo().getPolicyExpDate().Month.ToString("00") + _aiisQuoteMaster.getQuoteInfo().getPolicyExpDate().Day.ToString("00"), out iPolicyExpDate);
            //    int iEffDate = 0;
            //    int.TryParse(_aiisQuoteMaster.getPolicyInfo().getEffDate().Year.ToString("0000") + _aiisQuoteMaster.getPolicyInfo().getEffDate().Month.ToString("00") + _aiisQuoteMaster.getPolicyInfo().getEffDate().Day.ToString("00"), out iEffDate);
            //    int iExpDate = 0;
            //    int.TryParse(_aiisQuoteMaster.getPolicyInfo().getExpDate().Year.ToString("0000") + _aiisQuoteMaster.getPolicyInfo().getExpDate().Month.ToString("00") + _aiisQuoteMaster.getPolicyInfo().getExpDate().Day.ToString("00"), out iExpDate);
            //    int iQuoteEffDate = 0;
            //    int.TryParse(_aiisQuoteMaster.getPolicyInfo().getQuoteEffDate().Year.ToString("0000") + _aiisQuoteMaster.getPolicyInfo().getQuoteEffDate().Month.ToString("00") + _aiisQuoteMaster.getPolicyInfo().getQuoteEffDate().Day.ToString("00"), out iEffDate);
            //    int iVersionDate = 0;
            //    int.TryParse(_aiisQuoteMaster.getPolicyInfo().getVersionDate().Year.ToString("0000") + _aiisQuoteMaster.getPolicyInfo().getVersionDate().Month.ToString("00") + _aiisQuoteMaster.getPolicyInfo().getVersionDate().Day.ToString("00"), out iEffDate);
            //    int iCreditScoreEffDate = 0;
            //    int.TryParse(_aiisQuoteMaster.getPolicyInfo().getCreditScoreEffDate().Year.ToString("0000") + _aiisQuoteMaster.getPolicyInfo().getCreditScoreEffDate().Month.ToString("00") + _aiisQuoteMaster.getPolicyInfo().getCreditScoreEffDate().Day.ToString("00"), out iEffDate);
            //    int iIssueDate = 0;
            //    int.TryParse(_aiisQuoteMaster.getPolicyInfo().getIssueDate().Year.ToString("0000") + _aiisQuoteMaster.getPolicyInfo().getIssueDate().Month.ToString("00") + _aiisQuoteMaster.getPolicyInfo().getIssueDate().Day.ToString("00"), out iEffDate);
            //    int iOrigQuoteDate = 0;
            //    int.TryParse(_aiisQuoteMaster.getQuoteInfo().getOrigQuoteDate().Year.ToString("0000") + _aiisQuoteMaster.getQuoteInfo().getOrigQuoteDate().Month.ToString("00") + _aiisQuoteMaster.getQuoteInfo().getOrigQuoteDate().Day.ToString("00"), out iEffDate);
            //    int iOrigCompleteDate = 0;
            //    int.TryParse(_aiisQuoteMaster.getQuoteInfo().getOrigCompleteDate().Year.ToString("0000") + _aiisQuoteMaster.getQuoteInfo().getOrigCompleteDate().Month.ToString("00") + _aiisQuoteMaster.getQuoteInfo().getOrigCompleteDate().Day.ToString("00"), out iEffDate);
            //    int iRateVersionDate = 0;
            //    int.TryParse(_aiisQuoteMaster.getPolicyInfo().getRateVersionDate().Year.ToString("0000") + _aiisQuoteMaster.getPolicyInfo().getRateVersionDate().Month.ToString("00") + _aiisQuoteMaster.getPolicyInfo().getRateVersionDate().Day.ToString("00"), out iEffDate);
            //    int iRequoteDate = 0;
            //    int.TryParse(_aiisQuoteMaster.getQuoteInfo().getRequoteDate().Year.ToString("0000") + _aiisQuoteMaster.getQuoteInfo().getRequoteDate().Month.ToString("00") + _aiisQuoteMaster.getQuoteInfo().getRequoteDate().Day.ToString("00"), out iEffDate);
            //    int iQuoteConvDate = 0;
            //    int.TryParse(_aiisQuoteMaster.getQuoteInfo().getQuoteConvDate().Year.ToString("0000") + _aiisQuoteMaster.getQuoteInfo().getQuoteConvDate().Month.ToString("00") + _aiisQuoteMaster.getQuoteInfo().getQuoteConvDate().Day.ToString("00"), out iEffDate);
            //    int iRatemakerVersDate = 0;
            //    int.TryParse(_aiisQuoteMaster.getPolicyInfo().getRatemakerVersDate().Year.ToString("0000") + _aiisQuoteMaster.getPolicyInfo().getRatemakerVersDate().Month.ToString("00") + _aiisQuoteMaster.getPolicyInfo().getRatemakerVersDate().Day.ToString("00"), out iEffDate);
                
            //    context.Quotes.Add(new AutoQuoteLibrary.Quote 
            //    { 
            //        QM_QUOTE_NO_0 = _aiisQuoteMaster.getQuoteInfo().getQuoteNo0(),
            //        QM_FIRST_NAME_OF_CUSTOMER = _aiisQuoteMaster.getCustomer().getFirstNameOfCustomer(),
            //        QM_LAST_NAME_OF_CUSTOMER = _aiisQuoteMaster.getCustomer().getLastNameOfCustomer(),
            //        QM_MIDDLE_INITIAL = _aiisQuoteMaster.getCustomer().getMiddleInitial(),
            //        QM_ADDRESS_CITY = _aiisQuoteMaster.getCustomer().getAddressCity(),
            //        QM_ADDRESS_LINE_1 = _aiisQuoteMaster.getCustomer().getAddressLine1(),
            //        QM_ADDRESS_LINE_2 = _aiisQuoteMaster.getCustomer().getAddressLine2(),
            //        QM_ADDRESS_STATE_CODE = _aiisQuoteMaster.getCustomer().getAddressStateCode(),
            //        QM_ADDRESS_STATE_NO = (short)_aiisQuoteMaster.getCustomer().getAddressStateNo(),
            //        QM_MASTER_STATE_NO = (short)_aiisQuoteMaster.getCustomer().getMasterStateNo(),
            //        QM_ZIP_CODE_1 = (int)_aiisQuoteMaster.getCustomer().getZipCode1(),
            //        QM_ZIP_CODE_2 = (int)_aiisQuoteMaster.getCustomer().getZipCode2(),
            //        QM_ADDRESS_TERR = (short)_aiisQuoteMaster.getCustomer().getAddressTerr(),
            //        QM_ADDTL_CUSTOMER_TITLE = _aiisQuoteMaster.getCustomer().getAddtlCustomerTitle(),
            //        QM_ADDTL_FIRST_NAME_OF_CUST = _aiisQuoteMaster.getCustomer().getAddtlFirstNameOfCust(),
            //        QM_ADDTL_LAST_NAME_OF_CUST = _aiisQuoteMaster.getCustomer().getAddtlLastNameOfCust(),
            //        QM_ADDTL_MIDDLE_INITIAL = _aiisQuoteMaster.getCustomer().getAddtlMiddleInitial(),
            //        QM_ADDRESS_VERIFICATION_TEST = (short)_aiisQuoteMaster.getCustomer().getAddressVerificationTest(),
            //        QM_CUSTOMER_TITLE = _aiisQuoteMaster.getCustomer().getCustomerTitle(),
            //        QM_DAY_AREA_CODE = (short)_aiisQuoteMaster.getCustomer().getDayAreaCode(),
            //        QM_DAY_PHONE1 = (short)_aiisQuoteMaster.getCustomer().getDayPhone1(),
            //        QM_DAY_PHONE2 = (short)_aiisQuoteMaster.getCustomer().getDayPhone2(),
            //        QM_DAY_EXT = (short)_aiisQuoteMaster.getCustomer().getDayExt(),
            //        QM_DAY_PHONE_TYPE = (short)_aiisQuoteMaster.getCustomer().getDayPhoneType(),
            //        QM_EVE_AREA_CODE = (short)_aiisQuoteMaster.getCustomer().getEveAreaCode(),
            //        QM_EVE_PHONE1 = (short)_aiisQuoteMaster.getCustomer().getEvePhone1(),
            //        QM_EVE_PHONE2 = (short)_aiisQuoteMaster.getCustomer().getEvePhone2(),
            //        QM_EVE_EXT = (short)_aiisQuoteMaster.getCustomer().getEveExt(),
            //        QM_EVE_PHONE_TYPE = (short)_aiisQuoteMaster.getCustomer().getEvePhoneType(),
            //        QM_E_MAIL_ADDRESS = _aiisQuoteMaster.getCustomer().getEMailAddress(),
            //        QM_POLICY_EFF_DATE = iPolicyEffDate,
            //        QM_POLICY_EXP_DATE = iPolicyExpDate,
            //        QM_EFF_DATE = iEffDate,
            //        QM_EXP_DATE = iExpDate,
            //        QM_QUOTE_EFF_DATE = iQuoteEffDate,
            //        QM_EMAIL_ADDRESS_TEST = (short)_aiisQuoteMaster.getPolicyInfo().getEmailAddressTest(),
            //        QM_RISK_ADDRESS_LINE_1 = _aiisQuoteMaster.getCustomer().getRiskAddressLine1(),
            //        QM_RISK_ADDRESS_LINE_2 = _aiisQuoteMaster.getCustomer().getRiskAddressLine2(),
            //        QM_RISK_ADDRESS_CITY = _aiisQuoteMaster.getCustomer().getRiskAddressCity(),
            //        QM_RISK_STATE = (short)_aiisQuoteMaster.getCustomer().getRiskState(),
            //        QM_RISK_ZIP_CODE_1 = (short)_aiisQuoteMaster.getCustomer().getRiskZipCode1(),
            //        QM_RISK_ZIP_CODE_2 = (short)_aiisQuoteMaster.getCustomer().getRiskZipCode2(),
            //        QM_RISK_TERR = (short)_aiisQuoteMaster.getCustomer().getRiskTerr(),
            //        qm_market_brand = (short)_aiisQuoteMaster.getPolicyInfo().getMarketBrand(),
            //        QM_MARKET_KEY_CODE = _aiisQuoteMaster.getCustomer().getMarketKeyCode(),
            //        QM_MARKET_SOURCE_ADQ = (short)_aiisQuoteMaster.getCustomer().getMarketSourceAdq(),
            //        QM_PRODUCT_CODE = (short)_aiisQuoteMaster.getPolicyInfo().getProductCode(),
            //        QM_VERSION_NO = (short)_aiisQuoteMaster.getPolicyInfo().getVersionNo(),
            //        QM_VERSION_DATE = iVersionDate,
            //        QM_BILLING_METHOD = (short)_aiisQuoteMaster.getPolicyInfo().getBillingMethod(),
            //        QM_CREDIT_MODEL = _aiisQuoteMaster.getCustomer().getCreditModel(),
            //        QM_CREDIT_SCORE = _aiisQuoteMaster.getCustomer().getCreditScore(),
            //        QM_CREDIT_SCORE_EFF_DATE = iCreditScoreEffDate,
            //        QM_CREDIT_SCORE_TYPE = (short)_aiisQuoteMaster.getPolicyInfo().getCreditScoreType(),
            //        QM_CREDIT_SOURCE = (short)_aiisQuoteMaster.getPolicyInfo().getCreditSource(),
            //        QM_CREDIT_VENDOR = (short)_aiisQuoteMaster.getPolicyInfo().getCreditVendor(),
            //        QM_DNQ_NO = (short)_aiisQuoteMaster.getQuoteInfo().getDnqNo(),
            //        QM_CURRENT_CARRIER_NO = (short)_aiisQuoteMaster.getCustomer().getCurrentCarrierNo(),
            //        QM_CURRENT_CARRIER_PREM = (short)_aiisQuoteMaster.getPolicyInfo().getCurrentCarrierPrem(),
            //        QM_CURRENT_CARRIER_TYPE = (short)_aiisQuoteMaster.getCustomer().getCurrentCarrierType(),
            //        QM_CURRENT_LIMITS = (short)_aiisQuoteMaster.getCustomer().getCurrentLimits(),
            //        QM_CURRENT_CARRIER_TEST = (short)_aiisQuoteMaster.getPolicyInfo().getCurrentCarrierTest(),
            //        QM_COMPLETE_CODE = (short)_aiisQuoteMaster.getQuoteInfo().getCompleteCode(),
            //        QM_AMF_ACCOUNT_NO = (int)_aiisQuoteMaster.getPolicyInfo().getAmfAccountNo(),
            //        QM_UNDERWRITING_CO_NO = (int)_aiisQuoteMaster.getPolicyInfo().getUnderwritingCoNo(),
            //        QM_TOTAL_PREMIUM = (int)_aiisQuoteMaster.getPolicyInfo().getTotalPremium(),
            //        QM_TERM_FACTOR = (int)_aiisQuoteMaster.getPolicyInfo().getTermFactor(),
            //        QM_NO_OF_DRIVERS = (short)_aiisQuoteMaster.getDrivers().count(),
            //        QM_NO_OF_VEHICLES = (short)_aiisQuoteMaster.getVehicles().count(),
            //        QM_NO_OF_VIOLATIONS = (short)_aiisQuoteMaster.getViolations().count(),
            //        QM_NO_OF_ACC_COMP = (short)_aiisQuoteMaster.getAccidents().count(),
            //        QM_NO_OF_ADD_3_YRS = (short)_aiisQuoteMaster.getCustomer().getNoOfAdd3Yrs(),
            //        QM_NO_OF_DAYS_LAPSED = (short)_aiisQuoteMaster.getPolicyInfo().getNoOfDaysLapsed(),
            //        QM_NO_OF_YOUTHFULS = (short)_aiisQuoteMaster.getPolicyInfo().getNoOfYouthfuls(),
            //        QM_MULTI_POLICY_TEST = (short)_aiisQuoteMaster.getPolicyInfo().getMultiPolicyTest(),
            //        QM_MULTI_STATE_TEST = (short)_aiisQuoteMaster.getCustomer().getMultiStateTest(),
            //        QM_MASTER_CO_NO = (short)_aiisQuoteMaster.getCustomer().getMasterCoNo(),
            //        QM_NO_OF_EMP_3_YRS = (short)_aiisQuoteMaster.getCustomer().getNoOfEmp3Yrs(),
            //        QM_ALT_QUOTE_TEST = (short)_aiisQuoteMaster.getQuoteInfo().getAltQuoteTest(),
            //        qm_channel_method = (short)_aiisQuoteMaster.getPolicyInfo().getChannelMethod(),
            //        QM_USER_ID_NO = (short)_aiisQuoteMaster.getPolicyInfo().getUserIdNo(),
            //        QM_LOCATION_NO = (short)_aiisQuoteMaster.getPolicyInfo().getLocationNo(),
            //        QM_ISSUE_DATE = iIssueDate,
            //        QM_METHOD_CV_FORMS = (short)_aiisQuoteMaster.getPolicyInfo().getMethodCvForms(),
            //        QM_SPECIAL_CORRES_NO = (short)_aiisQuoteMaster.getCustomer().getSpecialCorresNo(),
            //        QM_PRODUCT_VERSION = (short)_aiisQuoteMaster.getCustomer().getProductVersion(),
            //        QM_QUOTE_PRINT_TEST = (short)_aiisQuoteMaster.getQuoteInfo().getQuotePrintTest(),
            //        QM_RESPONSE_NO = (short)_aiisQuoteMaster.getQuoteInfo().getResponseNo(),
            //        QM_QUOTE_TRANS_TYPE = (short)_aiisQuoteMaster.getQuoteInfo().getQuoteTransType(),
            //        QM_ORIG_QUOTE_DATE = iOrigQuoteDate,
            //        QM_ORIG_COMPLETE_DATE = iOrigCompleteDate,
            //        QM_RATE_ADJ_TERM = (short)_aiisQuoteMaster.getPolicyInfo().getRateAdjTerm(),
            //        QM_RATE_ADJ_FACTOR = (short)_aiisQuoteMaster.getPolicyInfo().getRateAdjFactor(),
            //        QM_RATE_CALC_TYPE = (short)_aiisQuoteMaster.getPolicyInfo().getRateCalcType(),
            //        QM_RATE_ADJ_FACTOR_INCRMNT = (short)_aiisQuoteMaster.getPolicyInfo().getRateAdjFactorIncrmnt(),
            //        QM_RATE_VERSION_DATE = iRateVersionDate,
            //        QM_CONTACT_TYPE_NO = (short)_aiisQuoteMaster.getCustomer().getContactTypeNo(),
            //        QM_EFT_TEST = (short)_aiisQuoteMaster.getPolicyInfo().getEftTest(),
            //        QM_DEPT_NO = (short)_aiisQuoteMaster.getPolicyInfo().getDeptNo(),
            //        QM_ENDORSER_USER_ID_NO = (short)_aiisQuoteMaster.getPolicyInfo().getEndorserUserIdNo(),
            //        QM_DRIV_EXCLUSION_TEST = (short)_aiisQuoteMaster.getCustomer().getDrivExclusionTest(),
            //        QM_ERMF_FACTOR = (short)_aiisQuoteMaster.getPolicyInfo().getErmfFactor(),
            //        QM_CONVERTED_UW_COMPANY = (short)_aiisQuoteMaster.getPolicyInfo().getUnderwritingCoNo()
            //        QM_ASSIST_SCORE = 0,
            //        QM_DOCUMENT_NO = (short)_aiisQuoteMaster.getPolicyInfo().getDocumentNo(),
            //        QM_CUST_PROFILE_NO = _aiisQuoteMaster.getCustomer().getCustProfileNo(),
            //        QM_CONTACT_DATE_END = 0,
            //        QM_CONTACT_DATE_START = 0,
            //        QM_CONTACT_RECV_DATE = 0,
            //        QM_CONTACT_TIME_END = 0,
            //        QM_CONTACT_TIME_START = 0,
            //        QM_EXCLUDE_AUTO_CALL = (short)_aiisQuoteMaster.getCustomer().getExcludeAutoCall(),
            //        QM_DNQ_BY_UW_CO_TEST = 0,
            //        QM_HOME_OFFICE_NO = (short)_aiisQuoteMaster.getPolicyInfo().getHomeOfficeNo(),
            //        QM_HOMEOWNER_VERIFY_TEST = (short)_aiisQuoteMaster.getCustomer().getHomeownerVerifyTest(),
            //        QM_BEST_TIME_TO_CALL = 0,
            //        qm_e_document_level = 0,
            //        QM_LEVEL_I_NO = (short)_aiisQuoteMaster.getPolicyInfo().getLevelIiNo(),
            //        qm_level_i_comm_rate = (short)_aiisQuoteMaster.getPolicyInfo().getLevelIiCommRate(),
            //        qm_level_ii_comm_rate = (short)_aiisQuoteMaster.getPolicyInfo().getLevelIiiCommRate(),
            //        QM_LEVEL_III_NO = (short)_aiisQuoteMaster.getPolicyInfo().getLevelIiiNo(),
            //        qm_level_iii_comm_rate = (short)_aiisQuoteMaster.getPolicyInfo().getLevelIiiCommRate(),
            //        QM_LEVEL_II_NO = (short)_aiisQuoteMaster.getPolicyInfo().getLevelIiNo(),
            //        QM_NEXT_VEHICLE_NO = (short)_aiisQuoteMaster.getPolicyInfo().getNextVehNo(),
            //        QM_NEXT_DRIVER_NO = (short)_aiisQuoteMaster.getPolicyInfo().getNextDriverNo(),
            //        qm_no_of_hh_drivers = (short)_aiisQuoteMaster.getPolicyInfo().getNoOfHhDrivers(),
            //        QM_NO_OF_REQUOTES  =(short)_aiisQuoteMaster.getQuoteInfo().getNoOfRequotes(),
            //        QM_MEMBER_NO = _aiisQuoteMaster.getPolicyInfo().getMemberNo2(),
            //        QM_OPTION_FEE_1 = (short)_aiisQuoteMaster.getPolicyInfo().getOptionFee1(),
            //        QM_OPTION_FEE_2 = (short)_aiisQuoteMaster.getPolicyInfo().getOptionFee2(),
            //        QM_OPTION_FEE_3 = (short)_aiisQuoteMaster.getPolicyInfo().getOptionFee3(),
            //        QM_OPTION_FEE_4 = (short)_aiisQuoteMaster.getPolicyInfo().getOptionFee4(),
            //        QM_OPTION_FEE_1_DIFF = (short)_aiisQuoteMaster.getPolicyInfo().getOptionFee1Diff(),
            //        QM_OPTION_FEE_2_DIFF = (short)_aiisQuoteMaster.getPolicyInfo().getOptionFee2Diff(),
            //        QM_OPTION_FEE_3_DIFF = (short)_aiisQuoteMaster.getPolicyInfo().getOptionFee3Diff(),
            //        QM_OPTION_FEE_4_DIFF = (short)_aiisQuoteMaster.getPolicyInfo().getOptionFee4Diff(),
            //        QM_MESSAGE_LINE1 = _aiisQuoteMaster.getCustomer().getMessageLine1(),
            //        QM_MESSAGE_LINE2 = _aiisQuoteMaster.getCustomer().getMessageLine2(),
            //        QM_OPTION_FEE1_TFA = (short)_aiisQuoteMaster.getPolicyInfo().getOptionFee1Tfa(),
            //        QM_OPTION_FEE2_TFA = (short)_aiisQuoteMaster.getPolicyInfo().getOptionFee2Tfa(),
            //        QM_OPTION_FEE3_TFA = (short)_aiisQuoteMaster.getPolicyInfo().getOptionFee3Tfa(),
            //        QM_OPTION_FEE4_TFA = (short)_aiisQuoteMaster.getPolicyInfo().getOptionFee4Tfa(),
            //        QM_ORIGIN_CODE_NO = 0,
            //        QM_PAID_IN_FULL_TEST = (short)_aiisQuoteMaster.getPolicyInfo().getPaidInFullTest(),
            //        QM_PAID_IN_FULL_OPTION_PREM = (int)_aiisQuoteMaster.getPolicyInfo().getPaidInFullOptionPrem(),
            //        QM_POLICY_FEE = (short)_aiisQuoteMaster.getPolicyInfo().getPolicyFee(),
            //        qm_no_of_can_notice_12_month = (short)_aiisQuoteMaster.getPolicyInfo().getNoOfCanNotice12Month(),
            //        QM_PHONE_QUOTE_TEST = 0,
            //        QM_MASTER_TERR_NO = (short)_aiisQuoteMaster.getCustomer().getMasterTerrNo(),
            //        qm_policy_form  =(short)_aiisQuoteMaster.getPolicyInfo().getPolicyForm(),
            //        QM_QUOTE_TEST = (short)_aiisQuoteMaster.getQuoteInfo().getQuoteTest(),
            //        QM_REFERENCE_NO = _aiisQuoteMaster.getCustomer().getReferenceNo(),
            //        QM_RENT_OWN_TEST = (short)_aiisQuoteMaster.getCustomer().getRentOwnTest(),
            //        QM_REG_OWNER_TEST = (short)_aiisQuoteMaster.getCustomer().getRegOwnerTest(),
            //        QM_REGUARANTEE_QUOTE_TEST = (short)_aiisQuoteMaster.getQuoteInfo().getReguaranteeQuoteTest(),
            //        QM_REQUOTE_DATE = iRequoteDate,
            //        QM_SOCIAL_SECURITY_NO = _aiisQuoteMaster.getCustomer().getSocialSecurityNo(),
            //        QM_SERVICING_OFFICE = 0,
            //        QM_QUOTE_TAX = 0,
            //        QM_QUASI_BIND_TEST = 0,
            //        QM_QUOTE_CONV_DATE = iQuoteConvDate,
            //        QM_SOLICIT_ID = _aiisQuoteMaster.getCustomer().getSolicitId(),
            //        QM_PREMIUM_TERM = (short)_aiisQuoteMaster.getPolicyInfo().getPremiumTerm(),
            //        QM_QUOTE_CANCEL_DATE = 0,
            //        qm_social_security_no_enc = 0,
            //        QM_WEB_DISCOUNT = 1,
            //        QM_SR22_APPLIED_TEST = (short)_aiisQuoteMaster.getCustomer().getSr22AppliedTest(),
            //        QM_SR22_FILING_FEE = (short)_aiisQuoteMaster.getPolicyInfo().getSr22FilingFee(),
            //        QM_SUB_PRODUCT_CODE = 0,
            //        QM_TELE_CONTACT_DATE = 0,
            //        QM_RISK_STNO_LOCK_TEST = 0,
            //        QM_RISK_ZIP_LOCK_TEST = 0,
            //        QM_TIER_LOCK_TEST = (short)_aiisQuoteMaster.getQuoteInfo().getTierLockTest(),
            //        QM_STATE_CO_LOCK_TEST = 0,
            //        QM_TORT_PREMIUM = 0,
            //        QM_QUOTE_PRINT_POINTER = 0,
            //        QM_POST_MARK_DATE = 0,
            //        QM_PRIOR_CONT_INS_TEST = (short)_aiisQuoteMaster.getPolicyInfo().getPriorContInsTest(),
            //        QM_USE_NEW_BILLING_FLAG = (short)_aiisQuoteMaster.getPolicyInfo().getUseNewBillingFlag(),
            //        qm_ratemaker_vers_date = iRatemakerVersDate,
            //        QM_ALTERNATE_CONTACT = "",
            //        QM_ANNUAL_INCOME = (short)_aiisQuoteMaster.getCustomer().getAnnualIncome(),
            //        QM_ACCTNUM_LOCK_TEST = 0,
            //        QM_LAST_REQUOTE_TRANS = 0,
            //        QM_MAIL_ID = "",
            //        QM_MAIL_STNO_LOCK_TEST = 0,
            //        QM_MARKET_KEY_RESERVE = "",
            //        QM_MAIL_ZIP_LOCK_TEST = 0,
            //        QM_LL_3_APPLIED_TEST = 0,
            //        QM_PHONE_EXTENSION = (short)_aiisQuoteMaster.getCustomer().getPhoneExtension(),
            //        QM_PREMIUM_FINANCE_TEST = 0,
            //        QM_PREMIUM_FINANCE_CO_NO = 0,
            //        QM_INSP_CONT_COV_SINCE_DATE = 0,
            //        QM_INSP_PRI_INS_WAIVER_TEST = 0,
            //        QM_LAST_ENDORSEMENT_DATE = 0,
            //        QM_MASTER_TERR_ADJ_TYPE = 0,
            //        QM_MASTER_TERR_ADQ_TYPE = 0,
            //        QM_PERSONAL_MESS_NO = 0,
            //        QM_POLICY_REFERENCE_NO = ""
            //    });
            //    for (int i=0;i<_aiisQuoteMaster.getVehicles().count();i++)
            //    {
            //        context.Vehicles.Add(new Vehicle
            //        {
            //            VO_YEAR = _aiisQuoteMaster.getVehicles().item(i).getVehYear().ToString("0000"),
            //            vo_year_make_model= "",
            //            VO_MAKE_NO = "",
            //            VO_MAKE_CODE = "",
            //            VO_MODEL = "",
            //            VO_MODEL_ID = "",
            //            VO_BODY = "",
            //            VO_4_DOOR = "",
            //            VO_ABS = "",
            //            VO_AIR_BAG = 0,
            //            VO_ANTI_THEFT = 0,
            //            VO_ANTI_LOCK_BRAKE = "",
            //            VO_DESCRIPTION = "",
            //            VO_COST_NEW = 0,
            //            VO_ADJUST_TO_MAKE_MODEL = "",
            //            VO_ANTI_THEFT_INFO = "",
            //            VO_MD_YEAR = "",
            //            VO_YEAR1 = "",
            //            VO_SEAT_BELTS = 0,
            //            VO_SAFE_VEHICLE = 0,
            //            VO_VIN_NO = "",
            //            VO_WEB_MODEL = "",
            //            VO_VEH_TYPE = 0,
            //            VO_DAYTIME_LIGHTS_TEST = 0,
            //            VO_DATE_ADDED = 0,
            //            VO_PERFORMANCE = 0,
            //            VO_RESTRAINT = "",
            //            VO_DRL = 0,
            //            VO_EXPOSURE = 0,
            //            VO_SYMBOL = 0,
            //            VO_SYMBOL_2 = 0
            //            VO_SYMBOL_3 = 0,
            //            VO_SYMBOL_4 = 0,
            //            VO_SYMBOL_5 = 0,
            //            VO_SYMBOL_6 = 0,
            //            VO_SYMBOL_7 = 0,
            //            VO_SYMBOL_8 = 0,
            //            VO_SYMBOL_9 = 0,
            //            VO_SYMBOL_10 = 0,
            //            VO_SOURCE = 0,
            //            VO_WHEELS_DRIVEN = "",
            //            VO_SYMBOL_EXCEPTION = 0,
            //            VO_OLD_FLAG = "",
            //            VO_IIN_CENTURY = "",
            //            VO_IIN_NO = "",
            //            VO_IIN_YEAR = "",
            //            VO_SPECIAL_INFO = "",
            //            VO_INELIGIBLE_VEH_TYPE = 0
            //        });
            //    }
            //}
        
    }
}
