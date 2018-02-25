using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using KdQuoteLibrary.Services;
//using KdQuoteLibrary.Interfaces;
//using KdQuoteLibrary.QuoteFlowHelper;
using AutoQuoteLibrary.AbstractServices;
using AutoQuoteLibrary.AutoQuoteHelper;
using UDILibrary.UDIExtensions.XMLSerialization;
using Newtonsoft.Json;
using System.Xml;
using System.Xml.Linq;

namespace KemperQuoteAngular2.Controllers
{
    public class AjaxController : Controller
    {
        private ISessionServices sessionService;
        private IVINServices vinService;
        private ILoggingServices loggingService;
        public AjaxController(ISessionServices sessionService, IVINServices vinService, ILoggingServices loggingService)
        {
            this.sessionService = sessionService;
            this.vinService = vinService;
            this.loggingService = loggingService;
        }
        [HttpPost]
        public string LoadSession(string guid, string zip, string ctid)
        {
            Guid myGuid = new Guid(guid);
            WebSession websession1 = sessionService.Load(ref myGuid, zip, false); // ctid);
            WebSessionDRC websession = new WebSessionDRC(websession1);
            string xResult = websession.Serialize<WebSessionDRC>();
            string quote = "<Quote>" + websession.Quote.serialize(null) + "</Quote>";
            xResult = xResult.Replace("<Quote />", quote);
            xResult = xResult.Replace("<Coverage ", "<Coverage json:Array='true' ");
            xResult = xResult.Replace("<VehicleCoverage ", "<VehicleCoverage json:Array='true' ");
            xResult = xResult.Replace("<Driver ", "<Driver json:Array='true' ");
            xResult = xResult.Replace("<Vehicle ", "<Vehicle json:Array='true' ");
            xResult = xResult.Replace("<WebSessionDRC ", "<WebSessionDRC xmlns:json='http://james.newtonking.com/projects/json' ");
            XElement xelement = XElement.Parse(xResult);
            string result = JsonConvert.SerializeXNode(xelement);
            //websession.Quote.deserialize(quote, null);
            //string result = JsonConvert.SerializeObject(websession);
            return result;
        }
        [HttpPost]
        public string CreateNewSession(string zip, string ctid)
        {
            Guid myGuid = Guid.Empty;
            WebSessionDRC websession =  new WebSessionDRC(sessionService.Load(ref myGuid, zip, true)); // ctid);
            string xResult = websession.Serialize<WebSessionDRC>();
            string quote = "<Quote>" + websession.Quote.serialize(null) + "</Quote>";
            xResult = xResult.Replace("<Quote />", quote);
            xResult = xResult.Replace("<Coverage ", "<Coverage json:Array='true' ");
            xResult = xResult.Replace("<VehicleCoverage ", "<VehicleCoverage json:Array='true' ");
            xResult = xResult.Replace("<Driver ", "<Driver json:Array='true' ");
            xResult = xResult.Replace("<Vehicle ", "<Vehicle json:Array='true' ");
            xResult = xResult.Replace("<WebSessionDRC ", "<WebSessionDRC xmlns:json='http://james.newtonking.com/projects/json' ");
            XElement xelement = XElement.Parse(xResult);
            string result = JsonConvert.SerializeXNode(xelement);
            return result;
        }
        [ValidateInput(false)]
        [HttpPost]
        public string LoadCoveragesAndDiscounts(string session)
        {
            try
            {
                XDocument sessionXml = JsonConvert.DeserializeXNode(session);
                WebSessionDRC websession =  new WebSessionDRC(sessionXml.ToString().Deserialize<WebSessionDRC>());
                websession.Quote = new AutoQuote.Autoquote();
                websession.Quote.deserialize(sessionXml.Element("WebSessionDRC").Element("Quote").ToString().Replace("<Quote>", "<DRCXML><RETURN><AiisQuoteMaster>").Replace("</Quote>", "</AiisQuoteMaster></RETURN></DRCXML>"), null);
                if (!sessionService.LoadCoveragesAndDiscounts(websession))
                    loggingService.logError("LoadCoveragesAndDiscounts returned false: ", "KemperQuoteAng2", "AutoQuoteLibrary", "LoadCoveragesAndDiscounts");
                string xResult = websession.Serialize<WebSessionDRC>();
                string quote = "<Quote>" + websession.Quote.serialize(null) + "</Quote>";
                xResult = xResult.Replace("<Quote />", quote);
                xResult = xResult.Replace("<Coverage ", "<Coverage json:Array='true' ");
                //xResult = xResult.Replace("<VehicleCoverage ", "<VehicleCoverage json:Array='true' ");
                xResult = xResult.Replace("<VehicleCoverage>", "<VehicleCoverage json:Array='true' >");
                xResult = xResult.Replace("<Driver ", "<Driver json:Array='true' ");
                xResult = xResult.Replace("<Vehicle ", "<Vehicle json:Array='true' ");
                xResult = xResult.Replace("<WebSessionDRC ", "<WebSessionDRC xmlns:json='http://james.newtonking.com/projects/json' ");
                XElement xelement = XElement.Parse(xResult);
                string result = JsonConvert.SerializeXNode(xelement);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.logError("LoadCoveragesAndDiscounts Error guid: " + ex.Message, "KemperQuoteAng2", "AutoQuoteLibrary", "LoadCoveragesAndDiscounts");
                return "Error: " + ex.Message;
            }
        }
        [ValidateInput(false)]
        [HttpPost]
        public string SaveSession(string session)
        {
            Guid guid = new Guid();
            try
            {
                XDocument sessionXml = JsonConvert.DeserializeXNode(session);
                WebSessionDRC websession = new WebSessionDRC(sessionXml.ToString().Deserialize<WebSessionDRC>());
                websession.Quote = new AutoQuote.Autoquote();
                websession.Quote.deserialize(sessionXml.Element("WebSessionDRC").Element("Quote").ToString().Replace("<Quote>", "<DRCXML><RETURN><AiisQuoteMaster>").Replace("</Quote>", "</AiisQuoteMaster></RETURN></DRCXML>"), null);
                guid = sessionService.Save(websession);
                loggingService.logError("SaveSession Success " + guid.ToString(), "KemperQuoteAng2", "AutoQuoteLibrary", "LoadCoveragesAndDiscounts");
                return "Success";
            }
            catch (Exception ex)
            {
                loggingService.logError("SaveSession Error guid: " + guid.ToString() + " + ex.Message", "KemperQuoteAng2", "AutoQuoteLibrary", "LoadCoveragesAndDiscounts");
                return "Error: " + ex.Message;
            }
        }

        [ValidateInput(false)]
        [HttpPost]
        public string RatedSave(string session)
        {
            XDocument sessionXml = JsonConvert.DeserializeXNode(session);
            WebSessionDRC websession =  new WebSessionDRC(sessionXml.ToString().Deserialize<WebSessionDRC>());
            websession.Quote = new AutoQuote.Autoquote();
            websession.Quote.deserialize(sessionXml.Element("WebSessionDRC").Element("Quote").ToString().Replace("<Quote>", "<DRCXML><RETURN><AiisQuoteMaster>").Replace("</Quote>", "</AiisQuoteMaster></RETURN></DRCXML>"), null);
            if (sessionService.RatedSave(websession))
                return "Success";
            else
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("Error. ");
                if (websession.HasErrors())
                    foreach (ErrorMessage err in websession.AddInfo.ErrorMessages)
                        sb.Append(err.Error + ". ");
                return sb.ToString();

            }
        }

        [ValidateInput(false)]
        [HttpPost]
        public string Recalculate(string session)
        {
            XDocument sessionXml = JsonConvert.DeserializeXNode(session);
            WebSessionDRC websession = new WebSessionDRC(sessionXml.ToString().Deserialize<WebSessionDRC>());
            websession.Quote = new AutoQuote.Autoquote();
            websession.Quote.deserialize(sessionXml.Element("WebSessionDRC").Element("Quote").ToString().Replace("<Quote>", "<DRCXML><RETURN><AiisQuoteMaster>").Replace("</Quote>", "</AiisQuoteMaster></RETURN></DRCXML>"), null);
            try
            {
                sessionService.Recalculate(websession);
            }
            catch (Exception ex)
            {
                websession.AddInfo.ErrorMessages.Add(new ErrorMessage { Error = ex.Message, Module = "AjaxController", Function = "Recalculate", Page = "Coverages" });
                loggingService.logError("Recalculate exception:" + ex.Message, "KemperQuoteAng2", "AutoQuoteLibrary", "LoadCoveragesAndDiscounts");
            }
            string xResult = websession.Serialize<WebSessionDRC>();
            string quote = "<Quote>" + websession.Quote.serialize(null) + "</Quote>";
            xResult = xResult.Replace("<Quote />", quote);
            xResult = xResult.Replace("<Coverage ", "<Coverage json:Array='true' ");
            //xResult = xResult.Replace("<VehicleCoverage ", "<VehicleCoverage json:Array='true' ");
            xResult = xResult.Replace("<VehicleCoverage>", "<VehicleCoverage json:Array='true' >");
            xResult = xResult.Replace("<Driver ", "<Driver json:Array='true' ");
            xResult = xResult.Replace("<Vehicle ", "<Vehicle json:Array='true' ");
            xResult = xResult.Replace("<WebSessionDRC ", "<WebSessionDRC xmlns:json='http://james.newtonking.com/projects/json' ");
            XElement xelement = XElement.Parse(xResult);
            string result = JsonConvert.SerializeXNode(xelement);
            return result;            
        }

        [ValidateInput(false)]
        [HttpPost]
        public string GetVehicleMakes(string year)
        {
            string xmlResult = vinService.GetMakeByYear(year);
            XElement xResult = XElement.Parse(xmlResult);
            var resultObject = from make in xResult.Elements("VehicleInfo").Elements("Make")
                               select new { Value = make.Element("MakeNumber").Value, Description = make.Element("Name").Value };
            string result = JsonConvert.SerializeObject(resultObject);
            return result;
        }
        [ValidateInput(false)]
        [HttpPost]
        public string GetWebVehicleModels(string year, string makeNo)
        {
            string xmlResult = vinService.GetWebModelByYearMake(year, makeNo);
            XElement xResult = XElement.Parse(xmlResult);
            var resultObject = from model in xResult.Elements("VehicleInfo").Elements("Model")
                               select new { Value = model.Element("Name").Value, Description = model.Element("Name").Value };
            string result = JsonConvert.SerializeObject(resultObject);
            return result;
        }

        [ValidateInput(false)]
        [HttpPost]
        public String GetVehicleByYearMakeModel(string year, string makeNo, string model, string webmodel)
        {
            string xmlResult = vinService.GetVehicleByYearMakeModel(year, makeNo, model, webmodel);
            XElement xResult = XElement.Parse(xmlResult);
            //var resultObject = new { Vin = xResult.Element("VehicleInfo").Element("Vehicle").Element("ModelVIN").Value };
            var resultObject = new
            {
                Vin = xResult.Element("VehicleInfo").Element("Vehicle").Element("ModelVIN").Value,
                BodyStyle = xResult.Element("VehicleInfo").Element("Vehicle").Element("Body").Value,
                VehicleTrim = xResult.Element("VehicleInfo").Element("Vehicle").Element("Style").Value,
                ModelNumber = xResult.Element("VehicleInfo").Element("Vehicle").Element("ModelID").Value,
                VehWebModel = model, //TODO - update plugin to get webmodel

                VehType = xResult.Element("VehicleInfo").Element("Vehicle").Element("VehicleType").Value,
                Performance = xResult.Element("VehicleInfo").Element("Vehicle").Element("Performance").Value,
                VehSymbol = xResult.Element("VehicleInfo").Element("Vehicle").Element("Symbol").Value,
                VehSymbolLiab = xResult.Element("VehicleInfo").Element("Vehicle").Element("SymbolLiab").Value,
                VehSymbolComp = xResult.Element("VehicleInfo").Element("Vehicle").Element("SymbolComp").Value,
                VehSymbolColl = xResult.Element("VehicleInfo").Element("Vehicle").Element("SymbolColl").Value,
                VehSymbolPip = xResult.Element("VehicleInfo").Element("Vehicle").Element("SymbolPip").Value,
                VehSymbolIsoComp = xResult.Element("VehicleInfo").Element("Vehicle").Element("SymbolIsoComp").Value,
                VehSymbolIsoColl = xResult.Element("VehicleInfo").Element("Vehicle").Element("SymbolIsoColl").Value,
                AdjustToMakeModel = xResult.Element("VehicleInfo").Element("Vehicle").Element("AdjToMakeModel").Value,
                SafeVeh = xResult.Element("VehicleInfo").Element("Vehicle").Element("SafeVehicle").Value,
                VehExposure = xResult.Element("VehicleInfo").Element("Vehicle").Element("Exposure").Value,
                YearMakeModel = xResult.Element("VehicleInfo").Element("Vehicle").Element("YearMakeModel").Value,
                VehHiPerfInd = xResult.Element("VehicleInfo").Element("Vehicle").Element("HiPerfInd").Value
            };
            string result = JsonConvert.SerializeObject(resultObject);
            return result;
        }

        [ValidateInput(false)]
        [HttpPost]
        public String GetVehicleYearMakeWebModel(string vin)
        {
            string result = "";
            try
            {

                string xmlResult = vinService.GetVehicleYearMakeWebModel(System.Web.HttpUtility.HtmlEncode(vin));
                XElement xResult = XElement.Parse(xmlResult);
                var resultObject = new
                {
                    //Year = xResult.Element("VehicleInfo").Element("Vehicle").Element("Year").Value,
                    Year = "2014",
                    Make = xResult.Element("VehicleInfo").Element("Vehicle").Element("Make").Value,
                    //MakeNo = xResult.Element("VehicleInfo").Element("Vehicle").Element("MakeNo").Value,
                    MakeNo = "0006",
                    Model = xResult.Element("VehicleInfo").Element("Vehicle").Element("Model").Value,
                    VehWebModel = xResult.Element("VehicleInfo").Element("Vehicle").Element("WebModel").Value,
                    BodyStyle = xResult.Element("VehicleInfo").Element("Vehicle").Element("Style").Value,
                    VehicleTrim = xResult.Element("VehicleInfo").Element("Vehicle").Element("Style").Value,
                    ModelNumber = xResult.Element("VehicleInfo").Element("Vehicle").Element("ModelID").Value,
                    VehType = xResult.Element("VehicleInfo").Element("Vehicle").Element("VehicleType").Value,
                    Performance = xResult.Element("VehicleInfo").Element("Vehicle").Element("HiPerfInd").Value,
                    VehSymbol = xResult.Element("VehicleInfo").Element("Vehicle").Element("Symbol").Value,
                    VehSymbolLiab = xResult.Element("VehicleInfo").Element("Vehicle").Element("SymbolLiab").Value,
                    VehSymbolComp = xResult.Element("VehicleInfo").Element("Vehicle").Element("SymbolComp").Value,
                    VehSymbolColl = xResult.Element("VehicleInfo").Element("Vehicle").Element("SymbolColl").Value,
                    VehSymbolPip = xResult.Element("VehicleInfo").Element("Vehicle").Element("SymbolPip").Value,
                    VehSymbolIsoComp = xResult.Element("VehicleInfo").Element("Vehicle").Element("SymbolIsoComp").Value,
                    VehSymbolIsoColl = xResult.Element("VehicleInfo").Element("Vehicle").Element("SymbolIsoColl").Value,
                    AdjustToMakeModel = xResult.Element("VehicleInfo").Element("Vehicle").Element("AdjToMakeModel").Value,
                    SafeVeh = xResult.Element("VehicleInfo").Element("Vehicle").Element("SafeVehicle").Value,
                    VehExposure = xResult.Element("VehicleInfo").Element("Vehicle").Element("Exposure").Value,
                    YearMakeModel = xResult.Element("VehicleInfo").Element("Vehicle").Element("YearMakeModel").Value,
                    VehHiPerfInd = xResult.Element("VehicleInfo").Element("Vehicle").Element("HiPerfInd").Value,
                    AntiLockBrake = xResult.Element("VehicleInfo").Element("Vehicle").Element("AntiLock").Value,
                    //AntiTheftInfo = xResult.Element("VehicleInfo").Element("Vehicle").Element("AntiTheftInfo").Value,
                    AntiTheftInfo = "1",
                    Restraint = xResult.Element("VehicleInfo").Element("Vehicle").Element("Restraint").Value
                };
                result = JsonConvert.SerializeObject(resultObject);
            }
            catch (Exception ex)
            {
                loggingService.logError(ex.Message, "KemperQuoteAng2", "AutoQuoteLibrary", "GetVehicleYearMakeWebModel");
            }
            return result;
        }
    }
}