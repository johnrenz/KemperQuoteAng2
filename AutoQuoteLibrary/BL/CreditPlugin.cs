using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Data.SqlClient;
using System.Reflection;

namespace AutoQuoteLibrary.BL
{
    //jrenz PRD20727 4/16/2012
    public enum eOrderCreditAddress
    {
        Default = 0,
        CurrentAddress = 1,
        PriorAddress = 2
    }
    public class CreditPlugin
    {
        private DateTime CreditScoreEffDate = DateTime.Now;
        private bool CreditScoreEffDateProvided = false;
        private Type credClassType;
        private Type subjectsType;
        private Type subjectType;
        private Type addressesType;
        private Type addressType;
        private Type savedCredRptsType;
        private Type savedCredRptType;
        private string sDOB;
        private bool bPriorAddressProvided = false;

       


        /// <summary>
        /// 
        /// </summary>
        /// <param name="credReqXml"></param>
        /// <returns></returns>
        public XElement OrderCredit(XElement credReqXml)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("<OrderCreditReturn>");
            
            //fake return
            sb.Append("<ReturnValue>0</ReturnValue>");
            sb.Append("<CreditInfo>");
            sb.Append("<CreditScoreType>1</CreditScoreType>");
            sb.Append("<Result>Complete</Result>");
            sb.Append("<CreditScore>800</CreditScore>");
            sb.Append("<CreditModel>0Q15</CreditModel>");
            sb.Append("<CreditSource>1</CreditSource>");
            sb.Append("<CreditVendor>1</CreditVendor>");
            sb.Append("<CreditScoreEffDate>" + DateTime.Now.ToShortDateString() + "</CreditScoreEffDate>");
            sb.Append("<CreditReportId>1</CreditReportId>");
            sb.Append("<MortgageExists>1</MortgageExists>");
            sb.Append("<ReturnedSSN>1234</ReturnedSSN>");
                    
            sb.Append("</CreditInfo>");

            sb.Append("</OrderCreditReturn>");

            return XElement.Parse(sb.ToString());
        }

       
    }
}
