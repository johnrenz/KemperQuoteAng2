using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UDILibrary.UDIExtensions.XMLSerialization;
using SynchronousPluginHelper.VINLookup.Objects;
using SynchronousPluginHelper.VINLookup.Parameters;
using SynchronousPluginHelper;
using KdQuoteLibrary.Interfaces;

namespace KdQuoteLibrary.Services
{
    public class VINServices : IVINServices
    {
        //private static readonly VINServices _vinServices = new VINServices();

        //private VINServices()
        //{

        //}

        //public static VINServices Instance
        //{
        //    get
        //    {
        //        return _vinServices;
        //    }
        //}

        public string GetMakeByYear(string year)
        {
            string response;

            string request = "<GetVINRequest><Year>" + year + "</Year></GetVINRequest>";
            XElement xReq = XElement.Parse(request);

            //XMLSyncProcess process = new XMLSyncProcess(new XMLSyncHeader("VINLookupPlugin", "Service", "vinlookup"), xReq);

            //using (ProcessWCF client = new ProcessWCF())
            //{
            //    XElement xResult = client.Execute(process);
            //    response = xResult.ToString();
            //}
            XElement xRes = XElement.Load(@"C:\VS projects\KemperQuoteAng2\GetMakeByYearXml.txt");
            response = xRes.ToString();
            return response;

            //this works to create request xml with serialized object
            //GetVINRequest request = new GetVINRequest { Year = Int32.Parse(year), ProductVersion = 1 };
            //XElement xReq = XElement.Parse(request.Serialize<GetVINRequest>());

            //this works to run vinlookupPlugin.dll from reference instead of from SynchPlugin wcf
            //VINLookupPlugin.Service svc = new VINLookupPlugin.Service();
            //XElement xResponse = svc.vinlookup(xReq);
            //response = xResponse.ToString();

        }
        public string GetModelByYearMake(string year, string makeno)
        {
            string response;

            string request = "<GetVINRequest><Year>" + year + "</Year><MakeNO>" + makeno + "</MakeNO><MODEL>1</MODEL><PRODUCTVERSION>2</PRODUCTVERSION></GetVINRequest>";
            XElement xReq = XElement.Parse(request);

            //XMLSyncProcess process = new XMLSyncProcess(new XMLSyncHeader("VINLookupPlugin", "Service", "vinlookup"), xReq);

            //using (ProcessWCF client = new ProcessWCF())
            //{
            //    XElement xResult = client.Execute(process);
            //    response = xResult.ToString();
            //}
            XElement xRes = XElement.Load(@"C:\VS projects\KemperQuoteAng2\GetModelByYearMakeXml.txt");
            response = xRes.ToString();
            return response;

            //this works to create request xml with serialized object
            //GetVINRequest request = new GetVINRequest { Year = Int32.Parse(year), MakeNO = Int32.Parse(makeno), ProductVersion = 1 };
            //XElement xReq = XElement.Parse(request.Serialize<GetVINRequest>());

            //this works to run vinlookupPlugin.dll from reference instead of from SynchPlugin wcf
            //VINLookupPlugin.Service svc = new VINLookupPlugin.Service();
            //XElement xResponse = svc.vinlookup(xReq);
            //response = xResponse.ToString();

        }
        public string GetWebModelByYearMake(string year, string makeno)
        {
            string response;

            string request = "<GetVINRequest><Year>" + year + "</Year><MakeNO>" + makeno + "</MakeNO><WEBMODEL>1</WEBMODEL><PRODUCTVERSION>2</PRODUCTVERSION></GetVINRequest>";
            XElement xReq = XElement.Parse(request);

            //XMLSyncProcess process = new XMLSyncProcess(new XMLSyncHeader("VINLookupPlugin", "Service", "vinlookup"), xReq);

            //using (ProcessWCF client = new ProcessWCF())
            //{
            //    XElement xResult = client.Execute(process);
            //    response = xResult.ToString();
            //}
            XElement xRes = XElement.Load(@"C:\VS projects\KemperQuoteAng2\GetWebModelByYearMakeXml.txt");
            response = xRes.ToString();
            return response;

            //this works to create request xml with serialized object
            //GetVINRequest request = new GetVINRequest { Year = Int32.Parse(year), MakeNO = Int32.Parse(makeno), ProductVersion = 1 };
            //XElement xReq = XElement.Parse(request.Serialize<GetVINRequest>());

            //this works to run vinlookupPlugin.dll from reference instead of from SynchPlugin wcf
            //VINLookupPlugin.Service svc = new VINLookupPlugin.Service();
            //XElement xResponse = svc.vinlookup(xReq);
            //response = xResponse.ToString();

        }
        public string GetTrimByYearMakeModel(string year, string makeno, string model)
        {
            string response;

            string request = "<GetVINRequest><Year>" + year + "</Year><MakeNO>" + makeno + "</MakeNO><Model>" + model + "</Model><MODEL>1</MODEL><PRODUCTVERSION>2</PRODUCTVERSION></GetVINRequest>";
            XElement xReq = XElement.Parse(request);

            XMLSyncProcess process = new XMLSyncProcess(new XMLSyncHeader("VINLookupPlugin", "Service", "vinlookup"), xReq);

            using (ProcessWCF client = new ProcessWCF())
            {
                XElement xResult = client.Execute(process);
                response = xResult.ToString();
            }
            return response;

            //this works to create request xml with serialized object
            //GetVINRequest request = new GetVINRequest { Year = Int32.Parse(year), MakeNO = Int32.Parse(makeno), ProductVersion = 1 };
            //XElement xReq = XElement.Parse(request.Serialize<GetVINRequest>());

            //this works to run vinlookupPlugin.dll from reference instead of from SynchPlugin wcf
            //VINLookupPlugin.Service svc = new VINLookupPlugin.Service();
            //XElement xResponse = svc.vinlookup(xReq);
            //response = xResponse.ToString();

        }
        public string GetVehicleYearMakeModel(string vin)
        {
            string response;

            string request = "<GetVINRequest><VIN>" + vin + "</VIN><MODEL>1</MODEL><PRODUCTVERSION>2</PRODUCTVERSION></GetVINRequest>";
            XElement xReq = XElement.Parse(request);

            XMLSyncProcess process = new XMLSyncProcess(new XMLSyncHeader("VINLookupPlugin", "Service", "vinlookup"), xReq);

            using (ProcessWCF client = new ProcessWCF())
            {
                XElement xResult = client.Execute(process);
                response = xResult.ToString();
            }
            return response;
        }
        public string GetVehicleYearMakeWebModel(string vin)
        {
            string response;

            string request = "<GetVINRequest><VIN>" + vin + "</VIN><WEBMODEL>1</WEBMODEL><PRODUCTVERSION>2</PRODUCTVERSION></GetVINRequest>";
            XElement xReq = XElement.Parse(request);

            //XMLSyncProcess process = new XMLSyncProcess(new XMLSyncHeader("VINLookupPlugin", "Service", "vinlookup"), xReq);

            //using (ProcessWCF client = new ProcessWCF())
            //{
            //    XElement xResult = client.Execute(process);
            //    response = xResult.ToString();
            //}

            XElement xRes = XElement.Load(@"C:\VS projects\KemperQuoteAng2\GetVehicleYearMakeWebModelXml.txt");
            response = xRes.ToString();
            return response;
        }
        public string GetVehicleByYearMakeModel(string year, string makeno, string model, string webmodel)
        {
            //GetVehicleByYearMakeMode
            string response;

            string request = "<GetVINRequest><Year>" + year + "</Year><MakeNO>" + makeno + "</MakeNO><Model>" + model + "</Model><WEBMODEL>" +webmodel + "</WEBMODEL><PRODUCTVERSION>2</PRODUCTVERSION></GetVINRequest>";
            XElement xReq = XElement.Parse(request);

            //XMLSyncProcess process = new XMLSyncProcess(new XMLSyncHeader("VINLookupPlugin", "Service", "vinlookup"), xReq);

            //using (ProcessWCF client = new ProcessWCF())
            //{
            //    XElement xResult = client.Execute(process);
            //    response = xResult.ToString();
            //}

            XElement xRes = XElement.Load(@"C:\VS projects\KemperQuoteAng2\GetVehicleByYearMakeModelXml.txt");
            response = xRes.ToString();
            return response;
        }

    }
}
