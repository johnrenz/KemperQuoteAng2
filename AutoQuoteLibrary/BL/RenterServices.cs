using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using UDILibrary.Environmental;

namespace QuoteFlowPlugin.BL
{
    internal class RenterServices : BaseService
    {
        //tc #13640 09-09-2010 - HO Timeout
        public String[] GetZipInfo(Quote quote)
        {
            //HO_StateRatesWS.HO_StateRatesWS ws = new QuoteFlowPlugin.HO_StateRatesWS.HO_StateRatesWS();
            //ysang PRD14158 I din't not why cannot set timeout here, it would get operation has timeout.
            // if we set timeout in pagegenQ, we will get this error:Client:Soap client is not initialized. HRESULT=0x80040007: Uninitialized object
            //why we directly call HO web serivce
            DecisionMakerRateMaker.RateMaker ws = new QuoteFlowPlugin.DecisionMakerRateMaker.RateMaker();
            //ws.Url = EnvironmentXMLReader.GetInstance.ReadValueFromNode(EnvironmentalXmlNodeTypes.WebService) + "/HO_StateRatesWS/HO_StateRatesWS.asmx";
            ws.Url = EnvironmentXMLReader.GetInstance.ReadValueFromNode(EnvironmentalXmlNodeTypes.HOWebService) + "/DecisionMaker/aspscript/RateMaker.asmx";
            ws.Timeout = 5000;
            //return ws.GetZipInfo(quote.AiisQuoteMaster.getCustomer().getZipCode1().ToString("00000"), quote.AiisQuoteMaster.getPolicyInfo().getQuoteEffDate().ToString("MM/dd/yyyy")).Split('|');
            return ws.LookupZipCode(quote.AiisQuoteMaster.getCustomer().getZipCode1().ToString("00000"), quote.AiisQuoteMaster.getPolicyInfo().getQuoteEffDate().ToString("MM/dd/yyyy")).Split('|');
          
        }
    }
}
