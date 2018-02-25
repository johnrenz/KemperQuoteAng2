using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using AutoQuoteLibrary.BL;
using AutoQuoteLibrary.AbstractServices;

namespace AutoQuoteLibrary.Services
{
    public class VINServices :IVINServices
    {
        public string GetMakeByYear(string year)
        {
            string response="";

            try
            {
                string request = "<GetVINRequest><Year>" + year + "</Year></GetVINRequest>";
                XElement xReq = XElement.Parse(request);

                AutoQuoteLibrary.BL.VINLookupPlugin service = new BL.VINLookupPlugin();
                response = service.GetMakeByYear(year);
            }
            catch (Exception ex)
            {
                LogUtility.LogError(ex.Message, "AutoQuoteLibrary", "VINServices", "GetMakeByYear");
            }
            return response;
        }
        public string GetModelByYearMake(string year, string makeno)
        {
            string response="";

            AutoQuoteLibrary.BL.VINLookupPlugin service = new BL.VINLookupPlugin();
            response = service.GetModelByYearMake(year, makeno, "0");
            
            return response;
        }
        public string GetWebModelByYearMake(string year, string makeno)
        {
            string response = "";

            AutoQuoteLibrary.BL.VINLookupPlugin service = new BL.VINLookupPlugin();
            response = service.GetModelByYearMake(year, makeno, "1");

            return response;
        }
        public string GetTrimByYearMakeModel(string year, string makeno, string model)
        {
            string response="";

             AutoQuoteLibrary.BL.VINLookupPlugin service = new BL.VINLookupPlugin();
             response = service.GetTrimByYearMakeModel(year, makeno, model);

            return response;

        }
        public string GetVehicleYearMakeModel(string vin)
        {
            string response="";

            AutoQuoteLibrary.BL.VINLookupPlugin service = new BL.VINLookupPlugin();
            response = service.GetVehicleYearMakeModel(vin);

            return response;
        }
        public string GetVehicleYearMakeWebModel(string vin)
        {
            string response="";

            string request = "<GetVINRequest><VIN>" + vin + "</VIN><WEBMODEL>1</WEBMODEL><PRODUCTVERSION>2</PRODUCTVERSION></GetVINRequest>";
            XElement xReq = XElement.Parse(request);

            AutoQuoteLibrary.BL.VINLookupPlugin service = new BL.VINLookupPlugin();
            response = service.GetVehicleYearMakeModel(vin);
            
            return response;
        }
        public string GetVehicleByYearMakeModel(string year, string makeno, string model, string webmodel)
        {
            //AutoQuoteEntitie7 Vehicle table only has 2014
            year = "2014";
            string response="";

            AutoQuoteLibrary.BL.VINLookupPlugin service = new BL.VINLookupPlugin();
            if (webmodel == "0")
                response = service.GetTrimByYearMakeModel(year, makeno, model);
            else 
                response = service.GetVehicleByYearMakeModel(year, makeno, model, webmodel);


            return response;
        }

    }
}
