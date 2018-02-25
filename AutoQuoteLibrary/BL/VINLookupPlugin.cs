using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
//using UDILibrary.DB.UDShared;
using System.Data.SqlClient;
using System.Data;
using System.Security;
using MoreLinq;

namespace AutoQuoteLibrary.BL
{
    public class VINLookupPlugin
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eXML"></param>
        /// <returns></returns>
        public XElement vinlookup(XElement reqXML)
        {
            string retstr = "<VINLookupReply><Valid>0</Valid><Message>Unknown Error</Message></VINLookupReply>";
            XElement serializedElement = null;

            try
            {
                //prd17509 - Added Web Model Functionality
                // If tag <WEBMODEL> exists and is set to 1, then the web model (vo_web_model) is used - otherwise old default (vo_model).
                XElement e = reqXML.Element("WEBMODEL");
                string usewebmodel = "0";
                if (e != null)
                {
                    string webmodel_test = e.Value.Trim();
                    if (!String.IsNullOrEmpty(webmodel_test))
                    {
                        if (webmodel_test == "1"){usewebmodel = "1";}
                    }
                }
                // end of set usewebmodel.

                AutoQuote.VehicleInfo vehInfo = new AutoQuote.VehicleInfo();

                e = reqXML.Element("VIN");
                if (e != null)
                {
                    string vin = e.Value.Trim();

                    if (!String.IsNullOrEmpty(vin))
                    {
                        //ysang prd16133 3/29/2011
                        e = reqXML.Element("PRODUCTVERSION");
                        if (e != null)
                        {
                            int iProd = 2;
                            Int32.TryParse(e.Value.Trim(), out iProd);
                            vehInfo.setProductVersion(iProd);
                        }
                        vehInfo.setVin(vin);
                        vehInfo.vinlookup();

                        // Note, only model is checked (not web model) as model will always exist on D014900 record by definition (ssr7615).
                        if (vehInfo.getYear() == 0 || vehInfo.getMake() == "" || vehInfo.getModel() == "")
                        {
                            retstr = "<VINLookupReply><Valid>0</Valid><Message>Invalid VIN</Message></VINLookupReply>";
                        }
                        else
                        {
                            retstr = "<VINLookupReply><Valid>1</Valid><Message>OK</Message><VehicleInfo>";

                            retstr += (vehInfo.getAdjToMakeModel() != null) ? "<AdjToMakeModel>" + vehInfo.getAdjToMakeModel().Trim() + "</AdjToMakeModel>" : "";
                            //PRD15381 WLU 1/19/2011
                            retstr += (vehInfo.getAntiLockBrake() != null) ? "<AntiLockBrake>" + MappingAntiLockBrake(vehInfo.getAntiLockBrake().Trim()) + "</AntiLockBrake>" : "";
                            //PRD15381 WLU 1/19/2011
                            retstr += (vehInfo.getAntiTheftInfo() != null) ? "<AntiTheftInfo>" + MappingAntiTheftInfo(vehInfo.getAntiTheftInfo().Trim()) + "</AntiTheftInfo>" : "";
                            retstr += (vehInfo.getBodyStyle() != null) ? "<BodyStyle>" + vehInfo.getBodyStyle().Trim() + "</BodyStyle>" : "";
                            retstr += (vehInfo.getDescription() != null) ? "<Description>" + vehInfo.getDescription().Trim() + "</Description>" : "";

                            retstr += "<ExposureType>" + vehInfo.getExposureType().ToString() + "</ExposureType>";
                            retstr += "<HiPerfInd>" + vehInfo.getHiPerfInd().ToString() + "</HiPerfInd>";

                            retstr += (vehInfo.getMake() != null) ? "<Make>" + vehInfo.getMake().Trim() + "</Make>" : "";
                            retstr += (vehInfo.getMakeNo() != null) ? "<MakeNo>" + vehInfo.getMakeNo().Trim() + "</MakeNo>" : "";

                            //PRD17509 pcraig 7/25/2011.
                            if (usewebmodel == "1")
                            {
                                retstr += (vehInfo.getWebModel() != null) ? "<Model>" + vehInfo.getWebModel().Trim() + "</Model>" : "";
                            }else{
                                retstr += (vehInfo.getModel() != null) ? "<Model>" + vehInfo.getModel().Trim() + "</Model>" : "";
                            }

                            retstr += (vehInfo.getModelNo() != null) ? "<ModelNo>" + vehInfo.getModelNo().Trim() + "</ModelNo>" : "";
                            retstr += (vehInfo.getPerfType() != null) ? "<PerfType>" + vehInfo.getPerfType().Trim() + "</PerfType>" : "";
                            //PRD15381 WLU 1/19/2011
                            retstr += (vehInfo.getRestraint() != null) ? "<Restraint>" + MappingRestraint(vehInfo.getRestraint().Trim()) + "</Restraint>" : "";

                            retstr += "<SafeVehicle>" + vehInfo.getSafeVehicle().ToString() + "</SafeVehicle>";

                            retstr += (vehInfo.getSymbol() != null) ? "<Symbol>" + vehInfo.getSymbol().Trim() + "</Symbol>" : "";
                            retstr += (vehInfo.getSymbolColl() != null) ? "<SymbolColl>" + vehInfo.getSymbolColl().Trim() + "</SymbolColl>" : "";
                            retstr += (vehInfo.getSymbolComp() != null) ? "<SymbolComp>" + vehInfo.getSymbolComp().Trim() + "</SymbolComp>" : "";
                            retstr += (vehInfo.getSymbolLiab() != null) ? "<SymbolLiab>" + vehInfo.getSymbolLiab().Trim() + "</SymbolLiab>" : "";
                            retstr += (vehInfo.getSymbolPip() != null) ? "<SymbolPip>" + vehInfo.getSymbolPip().Trim() + "</SymbolPip>" : "";

                            //SSR6871 PRD18507 WLU 9/27/2011
                            retstr += (vehInfo.getSymbolIsoComp() != null) ? "<SymbolIsoComp>" + vehInfo.getSymbolIsoComp().Trim() + "</SymbolIsoComp>" : "";
                            retstr += (vehInfo.getSymbolIsoColl() != null) ? "<SymbolIsoColl>" + vehInfo.getSymbolIsoColl().Trim() + "</SymbolIsoColl>" : "";

                            retstr += "<SymbolYearAdj>" + vehInfo.getSymbolYearAdj().ToString() + "</SymbolYearAdj>";
                            //tc #6773 08-20-2010 - VIN Revisions
                            retstr += "<YearMakeModel>" + vehInfo.getYearMakeModel().Trim() + "</YearMakeModel>";
                            retstr += "<TerrBi>" + vehInfo.getTerrBi().ToString() + "</TerrBi>";
                            retstr += "<TerrColl>" + vehInfo.getTerrColl().ToString() + "</TerrColl>";
                            retstr += "<TerrComp>" + vehInfo.getTerrComp().ToString() + "</TerrComp>";
                            retstr += "<TerrPd>" + vehInfo.getTerrPd().ToString() + "</TerrPd>";
                            retstr += "<TerrPip>" + vehInfo.getTerrPip().ToString() + "</TerrPip>";
                            retstr += "<TerrUm>" + vehInfo.getTerrUm().ToString() + "</TerrUm>";

                            retstr += (vehInfo.getVehicleType() != null) ? "<VehicleType>" + vehInfo.getVehicleType().Trim() + "</VehicleType>" : "";
                            retstr += (vehInfo.getVin() != null) ? "<Vin>" + SecurityElement.Escape(vehInfo.getVin()).Trim() + "</Vin>" : "";

                            retstr += "<Year>" + vehInfo.getYear().ToString() + "</Year>";

                            retstr += "</VehicleInfo></VINLookupReply>";
                        }
                    }
                }
                else
                {
                    e = reqXML.Element("Year");
                    if (e != null)
                    {
                        string year = e.Value;
                        int nYear;
                        if (int.TryParse(year, out nYear))
                        {
                            e = reqXML.Element("MakeNO");
                            if (e != null)
                            {
                                string makeno = e.Value.Trim();
                                if (!String.IsNullOrEmpty(makeno))
                                {
                                    e = reqXML.Element("Model");
                                    if (e != null)
                                    {
                                        string model = e.Value.Trim();
                                        if (!String.IsNullOrEmpty(model))
                                        {
                                            //PRD17509 pcraig 7/25/2011
                                            retstr = GetVehicleByYearMakeMode(year, makeno, model, usewebmodel);
                                        }
                                        else
                                        {
                                            retstr = "<VINLookupReply><Valid>0</Valid><Message>Invalid Model</Message></VINLookupReply>";
                                        }
                                    }
                                    else
                                    {
                                        //PRD17509 pcraig 7/25/2011
                                        retstr = GetModelByYearMake(year, makeno, usewebmodel);
                                    }
                                }
                                else
                                {
                                    retstr = "<VINLookupReply><Valid>0</Valid><Message>Invalid Make Number</Message></VINLookupReply>";
                                }
                            }
                            else
                            {
                                retstr = GetMakeByYear(year);
                            }
                        }
                        else
                        {
                            retstr = "<VINLookupReply><Valid>0</Valid><Message>Invalid Year</Message></VINLookupReply>";
                        }
                    }
                    else
                    {
                        retstr = "<VINLookupReply><Valid>0</Valid><Message>Invalid Parameter</Message></VINLookupReply>";
                    }
                }
            }
            catch (Exception ex)
            {
                retstr = "<VINLookupReply><Valid>0</Valid><Message>" + ex.Message + "</Message></VINLookupReply>";
            }

            //tc #19689 01-10-2012 - Escape & as it is an XML entity
            serializedElement = XElement.Parse(retstr.Replace("& ", "&amp; "));
            return serializedElement;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reqXML"></param>
        /// <returns></returns>
        public XElement GetModelNameByNumber(XElement reqXML)
        {
            //prd17509 - Added Web Model Functionality
            // If tag <WEBMODEL> exists and is set to 1, then the web model (vo_web_model) is returned instead of model (vo_model).
            //XElement e = reqXML.Element("WEBMODEL");
            //string usewebmodel = "0";
            //if (e != null)
            //{
            //    string webmodel_test = e.Value.Trim();
            //    if (!String.IsNullOrEmpty(webmodel_test))
            //    {
            //        if (webmodel_test == "1"){usewebmodel = "1";}
            //    }
            //}
            //// end of set usewebmodel.

            //e = reqXML.Element("ModelNo");
            //if (e == null)
            //    throw new Exception("VINLookupPlugin.GetModelNameByNumber failed: missing model number");

            //string modelno = e.Value.Trim();

            //string sqlQry = "SELECT k_vo_model_ver1 = SUBSTRING(vo_model,1,20) FROM " + Environmental.getDRCDatabase() + ".dbo.D014900 WHERE vo_model_id = " + modelno;

            ////prd17509 pcraig 7/25/2011.
            //if (usewebmodel == "1")
            //{
            //    sqlQry = "SELECT k_vo_model_ver1 = vo_web_model FROM " + Environmental.getDRCDatabase() + ".dbo.D014900 WHERE vo_model_id = " + modelno;
            //}

            //DatabaseHelper dbHelp = DatabaseHelper.getInstance();
            //SqlCommand myCommand = dbHelp.getSQLCommand(sqlQry);

            //SqlDataReader dr = myCommand.ExecuteReader();
            //dr.Read();
            //string modelName = dr["k_vo_model_ver1"].ToString().Trim();

            //dr.Close();
            //dbHelp.closeSQLCommand(myCommand);
            ////tc #19689 01-10-2012 - Escape & as it is an XML entity
            //return XElement.Parse("<ModelName>" + modelName.Replace("& ", "&amp; ") + "</ModelName>");
            return null;
        }
        public string GetVehicleByYearMakeModel(string year, string makeNo, string model, string webmodel)
        {
            //    sSql = "SELECT DISTINCT VO_MODEL_ID, VO_BODY, VO_DESCRIPTION, VO_MAKE_CODE, VO_VIN_NO, VO_VEH_TYPE, VO_PERFORMANCE, VO_SYMBOL, VO_SYMBOL_2, VO_SYMBOL_3, VO_SYMBOL_5, VO_SYMBOL_6, VO_SYMBOL_7, VO_SYMBOL_8, VO_ADJUST_TO_MAKE_MODEL, VO_SAFE_VEHICLE, VO_EXPOSURE, vo_year_make_model, VO_SYMBOL_9   " +
            //        "FROM " + Environmental.getDRCDatabase() + ".dbo.D014900 b   " +
            //        "WHERE PRIME_KEY IN ( " +
            //        "SELECT MAX(PRIME_KEY) FROM " + Environmental.getDRCDatabase() + ".dbo.D014900 C " +
            //        " WHERE VO_MD_YEAR = @YEAR AND   " +
            //        "       VO_MAKE_NO = @MAKENO AND   " +
            //        "       VO_WEB_MODEL = @MODEL AND   " +
            //        "   VO_OLD_FLAG not in('Y','V'))";
            using (var context = new AutoQuoteEntitie7())
            {
                string makeNo4 = int.Parse(makeNo).ToString("0000");
                var vehicles = (from v in context.Vehicles
                               where v.VO_YEAR == year && v.VO_MAKE_NO == makeNo4 && v.VO_WEB_MODEL == model.Trim() && v.VO_OLD_FLAG != "Y" && v.VO_OLD_FLAG != "V"
                                select new { 
                                    VO_MODEL_ID = v.VO_MODEL_ID, 
                                    VO_BODY = v.VO_BODY, 
                                    VO_DESCRIPTION = v.VO_DESCRIPTION, 
                                    VO_MAKE_CODE = v.VO_MAKE_CODE, 
                                    VO_VIN_NO = v.VO_VIN_NO, 
                                    VO_VEH_TYPE = v.VO_VEH_TYPE, 
                                    VO_PERFORMANCE = v.VO_PERFORMANCE, 
                                    VO_SYMBOL = v.VO_SYMBOL, 
                                    VO_SYMBOL_2 = v.VO_SYMBOL_2, 
                                    VO_SYMBOL_3 = v.VO_SYMBOL_3, 
                                    VO_SYMBOL_5 = v.VO_SYMBOL_5, 
                                    VO_SYMBOL_6 = v.VO_SYMBOL_6, 
                                    VO_SYMBOL_7 = v.VO_SYMBOL_7, 
                                    VO_SYMBOL_8 = v.VO_SYMBOL_8, 
                                    VO_ADJUST_TO_MAKE_MODEL = v.VO_SYMBOL_8, 
                                    VO_SAFE_VEHICLE = v.VO_SAFE_VEHICLE, 
                                    VO_EXPOSURE = v.VO_EXPOSURE, 
                                    vo_year_make_model = v.vo_year_make_model, 
                                    VO_SYMBOL_9 = v.VO_SYMBOL_9 }).Distinct(); 

                string retstr = "";
                if (vehicles.Count() == 0)
                {
                    retstr = "<VINLookupReply><Valid>0</Valid><Message>No Model Found</Message></VINLookupReply>";
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<VINLookupReply><Valid>1</Valid><Message>OK</Message>");
                    var veh = vehicles.First();
                    sb.Append("<VehicleInfo>");
                    sb.Append("<Vehicle>");

                    string bod = veh.VO_BODY.Trim();
                    sb.Append("<Body>" + bod + "</Body>");

                    string sty = veh.VO_DESCRIPTION.Trim();
                    sb.Append("<Style>" + sty + "</Style>");

                    string modelId = veh.VO_MODEL_ID.Trim();
                    sb.Append("<ModelID>" + modelId + "</ModelID>");

                    string modelVIN = veh.VO_MAKE_CODE.Trim() + veh.VO_VIN_NO.Trim();
                    modelVIN = SecurityElement.Escape(modelVIN);
                    sb.Append("<ModelVIN>" + modelVIN + "</ModelVIN>");

                    string vehType = veh.VO_VEH_TYPE.ToString();
                    sb.Append("<VehicleType>" + vehType + "</VehicleType>");

                    string voPerf = veh.VO_PERFORMANCE.ToString();
                    sb.Append("<Performance>" + voPerf + "</Performance>");

                    string voSymb = veh.VO_SYMBOL.ToString();
                    //jrenz 10/04/2006 add Symbol to drop down
                    string uiString = bod + " " + sty + " (Symb " + voSymb.PadLeft(3) + ", " + modelVIN + ")";

                    if (uiString.IndexOf("MAN") > 0)
                        uiString = uiString.Replace("MAN", "") + " M";
                    if (uiString.IndexOf("AUTO") > 0)
                        uiString = uiString.Replace("AUTO", "") + " A";

                    while (uiString.IndexOf("  ") > 0)
                        uiString = uiString.Replace("  ", " ");

                    sb.Append("<Symbol>" + uiString + "</Symbol>");

                    string voSymbLiab = veh.VO_SYMBOL_5.ToString();
                    sb.Append("<SymbolLiab>" + voSymbLiab + "</SymbolLiab>");

                    string voSymbComp = veh.VO_SYMBOL_6.ToString();
                    sb.Append("<SymbolComp>" + voSymbComp + "</SymbolComp>");

                    string voSymbColl = veh.VO_SYMBOL_7.ToString();
                    sb.Append("<SymbolColl>" + voSymbColl + "</SymbolColl>");

                    string voSymbPip = veh.VO_SYMBOL_8.ToString();
                    sb.Append("<SymbolPip>" + voSymbPip + "</SymbolPip>");

                    //SSR6871 PRD18507 WLU 9/27/2011
                    string voSymbIsoComp = veh.VO_SYMBOL_2.ToString();
                    sb.Append("<SymbolIsoComp>" + voSymbIsoComp + "</SymbolIsoComp>");

                    string voSymbIsoColl = veh.VO_SYMBOL_3.ToString();
                    sb.Append("<SymbolIsoColl>" + voSymbIsoColl + "</SymbolIsoColl>");

                    string voAdjust = veh.VO_ADJUST_TO_MAKE_MODEL.ToString();
                    sb.Append("<AdjToMakeModel>" + voAdjust + "</AdjToMakeModel>");

                    string voSafeVeh = veh.VO_SAFE_VEHICLE.ToString();
                    sb.Append("<SafeVehicle>" + voSafeVeh + "</SafeVehicle>");

                    string voExposure = veh.VO_EXPOSURE.ToString();
                    sb.Append("<Exposure>" + voExposure + "</Exposure>");

                    //jrenz #4353 11/28/2006 
                    string voYearMakeModel = veh.vo_year_make_model.Trim();
                    sb.Append("<YearMakeModel>" + voYearMakeModel + "</YearMakeModel>");

                    //jrenz #PRD01006 02/27/2007
                    string voHiPerfInd = veh.VO_PERFORMANCE.ToString();
                    sb.Append("<HiPerfInd>" + voHiPerfInd + "</HiPerfInd>");

                    sb.Append("</Vehicle>");
                    sb.Append("</VehicleInfo>");
                    sb.Append("</VINLookupReply>");
                    retstr = sb.ToString();
                }
                return retstr;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public string GetMakeByYear(string year)
        {
            if (year.CompareTo("2014") > 0) // AutoQuoteEntitie7 loadedwith partial data.
                year = "2014";
            using (var context = new AutoQuoteEntitie7())
            {
                var makes = from m in context.YearMakes
                            where m.mk_year.Equals(year)
                            orderby m.MK_DESCRIP
                            select m;

                string retstr = "";
                if (makes.Count() == 0)
                {
                    retstr = "<VINLookupReply><Valid>0</Valid><Message>No make found</Message></VINLookupReply>";
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<VINLookupReply><Valid>1</Valid><Message>OK</Message>");
                    foreach (YearMake make in makes)
                    {
                        sb.Append("<VehicleInfo>");
                        sb.Append("<Make>");
                        sb.Append("<Name>" + make.MK_DESCRIP + "</Name>");
                        sb.Append("<MakeNumber>" + make.MK_NUMBER_VER + "</MakeNumber>");
                        sb.Append("</Make>");
                        sb.Append("</VehicleInfo>");
                    }
                    sb.Append("</VINLookupReply>");
                    retstr = sb.ToString();
                }
                return retstr;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="makeNo"></param>
        /// <returns></returns>
        public string GetModelByYearMake(string year, string makeNo, string usewebmodel)
        {
            //AutoQuoteEntitie7 Vehicle table only has 2014
            year = "2014";
            using (var context = new AutoQuoteEntitie7())
            {
                string makeNo4 = int.Parse(makeNo).ToString("0000");
                var models = from m in context.ModelYearMakes
                            join v in context.Vehicles on m.MODEL_ID equals v.VO_MODEL_ID
                             where m.YEAR == year && v.VO_MAKE_NO == makeNo4 && v.VO_OLD_FLAG != "Y" && v.VO_OLD_FLAG != "V"
                            select v;
                var distinctMModels = models.DistinctBy(x => x.VO_MODEL);
                if (usewebmodel == "1")
                    distinctMModels = models.DistinctBy(x => x.VO_WEB_MODEL);
                string retstr = "";
                if (models.Count() == 0)
                {
                    retstr = "<VINLookupReply><Valid>0</Valid><Message>No Model Found</Message></VINLookupReply>";
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<VINLookupReply><Valid>1</Valid><Message>OK</Message>");
                    foreach (Vehicle veh in distinctMModels)
                    {
                        sb.Append("<VehicleInfo>");
                        sb.Append("<Model>");
                        if (usewebmodel == "1")
                            sb.Append("<Name>" + SecurityElement.Escape(veh.VO_WEB_MODEL) + "</Name>");
                        else
                            sb.Append("<Name>" + SecurityElement.Escape(veh.VO_MODEL) + "</Name>");
                        sb.Append("</Model>");
                        sb.Append("</VehicleInfo>");
                    }
                    sb.Append("</VINLookupReply>");
                    retstr = sb.ToString();
                }
                return retstr;
            }
        }

        public string GetTrimByYearMakeModel(string year, string makeNo, string model)
        {
            //AutoQuoteEntitie7 Vehicle table only has 2014
            year = "2014";

            using (var context = new AutoQuoteEntitie7())
            {
                string makeNo4 = int.Parse(makeNo).ToString("0000");
                var vehicles = from v in context.Vehicles
                               where v.VO_YEAR == year && v.VO_MAKE_NO == makeNo4 && v.VO_MODEL == model && v.VO_OLD_FLAG != "Y" && v.VO_OLD_FLAG != "V"
                               group v by v.PRIME_KEY into vehGroups
                               select vehGroups;

                string retstr = "";
                if (vehicles.Count() == 0)
                {
                    retstr = "<VINLookupReply><Valid>0</Valid><Message>No Model Found</Message></VINLookupReply>";
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<VINLookupReply><Valid>1</Valid><Message>OK</Message>");
                    foreach (var group in vehicles)
                    {
                        foreach (var veh in group)
                        {
                            sb.Append("<VehicleInfo>");
                            sb.Append("<Vehicle>");

                            string bod = veh.VO_BODY.Trim();
                            sb.Append("<Body1>" + bod + "</Body1>");

                            string sty = veh.VO_DESCRIPTION.Trim();
                            sb.Append("<Style>" + sty + "</Style>");

                            string modelId = veh.VO_MODEL_ID.Trim();
                            sb.Append("<ModelID>" + modelId + "</ModelID>");

                            string modelVIN = veh.VO_MAKE_CODE.Trim() + veh.VO_VIN_NO.Trim();
                            modelVIN = SecurityElement.Escape(modelVIN);
                            sb.Append("<ModelVIN>" + modelVIN + "</ModelVIN>");

                            string vehType = veh.VO_VEH_TYPE.ToString();
                            sb.Append("<VehicleType>" + vehType + "</VehicleType>");

                            string voPerf = veh.VO_PERFORMANCE.ToString();
                            sb.Append("<Performance>" + voPerf + "</Performance>");

                            string voSymb = veh.VO_SYMBOL.ToString();
                            //jrenz 10/04/2006 add Symbol to drop down
                            string uiString = bod + " " + sty + " (Symb " + voSymb.PadLeft(3) + ", " + modelVIN + ")";

                            if (uiString.IndexOf("MAN") > 0)
                                uiString = uiString.Replace("MAN", "") + " M";
                            if (uiString.IndexOf("AUTO") > 0)
                                uiString = uiString.Replace("AUTO", "") + " A";

                            while (uiString.IndexOf("  ") > 0)
                                uiString = uiString.Replace("  ", " ");

                            sb.Append("<Symbol>" + uiString + "</Symbol>");

                            string voSymbLiab = veh.VO_SYMBOL_5.ToString();
                            sb.Append("<SymbolLiab>" + voSymbLiab + "</SymbolLiab>");

                            string voSymbComp = veh.VO_SYMBOL_6.ToString();
                            sb.Append("<SymbolComp>" + voSymbComp + "</SymbolComp>");

                            string voSymbColl = veh.VO_SYMBOL_7.ToString();
                            sb.Append("<SymbolColl>" + voSymbColl + "</SymbolColl>");

                            string voSymbPip = veh.VO_SYMBOL_8.ToString();
                            sb.Append("<SymbolPip>" + voSymbPip + "</SymbolPip>");

                            //SSR6871 PRD18507 WLU 9/27/2011
                            string voSymbIsoComp = veh.VO_SYMBOL_2.ToString();
                            sb.Append("<SymbolIsoComp>" + voSymbIsoComp + "</SymbolIsoComp>");

                            string voSymbIsoColl = veh.VO_SYMBOL_3.ToString();
                            sb.Append("<SymbolIsoColl>" + voSymbIsoColl + "</SymbolIsoColl>");

                            string voAdjust = veh.VO_ADJUST_TO_MAKE_MODEL.Trim();
                            sb.Append("<AdjToMakeModel>" + voAdjust + "</AdjToMakeModel>");

                            string voSafeVeh = veh.VO_SAFE_VEHICLE.ToString();
                            sb.Append("<SafeVehicle>" + voSafeVeh + "</SafeVehicle>");

                            string voExposure = veh.VO_EXPOSURE.ToString();
                            sb.Append("<Exposure>" + voExposure + "</Exposure>");

                            //jrenz #4353 11/28/2006 
                            string voYearMakeModel = veh.vo_year_make_model.Trim();
                            sb.Append("<YearMakeModel>" + voYearMakeModel + "</YearMakeModel>");

                            //jrenz #PRD01006 02/27/2007
                            string voHiPerfInd = veh.VO_PERFORMANCE.ToString();
                            sb.Append("<HiPerfInd>" + voHiPerfInd + "</HiPerfInd>");

                            sb.Append("</Vehicle>");
                            sb.Append("</VehicleInfo>");
                        }
                    }
                    sb.Append("</VINLookupReply>");
                    retstr = sb.ToString();
                }
                return retstr;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="makeNo"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private string GetVehicleByYearMakeMode(string year, string makeNo, string model, string usewebmodel)
        {
            //ysang prd14991 1/11/2011 include "V"
            //SSR6871 PRD18507 WLU 9/27/2011
            //string sSql = "SELECT DISTINCT VO_MODEL_ID, VO_BODY, VO_DESCRIPTION, VO_MAKE_CODE, VO_VIN_NO, VO_VEH_TYPE, VO_PERFORMANCE, VO_SYMBOL, VO_SYMBOL_2, VO_SYMBOL_3, VO_SYMBOL_5, VO_SYMBOL_6, VO_SYMBOL_7, VO_SYMBOL_8, vo_adjust_to_make_model, vo_safe_vehicle, vo_exposure, VO_YEAR_MAKE_MODEL, VO_SYMBOL_9   " +
            //              "FROM " + Environmental.getDRCDatabase() + ".dbo.D014900 b   " +
            //              "WHERE PRIME_KEY IN ( " +
            //              "SELECT MIN(PRIME_KEY) FROM " + Environmental.getDRCDatabase() + ".dbo.D014900 C " +
            //              "WHERE VO_MD_YEAR = @YEAR AND   " +
            //              "      VO_MAKE_NO = @MAKENO AND   " +
            //              "      VO_MODEL = @MODEL AND   " +
            //              "      VO_OLD_FLAG not in('Y','V') " +
            //              "GROUP BY vo_body, vo_description) " +
            //              "ORDER BY vo_body, vo_description";

            //if (usewebmodel == "1")
            //{
            //    // PRD17509 - pcraig - 7/18/2011
            //    // SSR07615 - pcraig - 6/8/2011 (Change to Web Model)
            //    //SSR6871 PRD18507 WLU 9/27/2011
            //    sSql = "SELECT DISTINCT VO_MODEL_ID, VO_BODY, VO_DESCRIPTION, VO_MAKE_CODE, VO_VIN_NO, VO_VEH_TYPE, VO_PERFORMANCE, VO_SYMBOL, VO_SYMBOL_2, VO_SYMBOL_3, VO_SYMBOL_5, VO_SYMBOL_6, VO_SYMBOL_7, VO_SYMBOL_8, VO_ADJUST_TO_MAKE_MODEL, VO_SAFE_VEHICLE, VO_EXPOSURE, vo_year_make_model, VO_SYMBOL_9   " +
            //        "FROM " + Environmental.getDRCDatabase() + ".dbo.D014900 b   " +
            //        "WHERE PRIME_KEY IN ( " +
            //        "SELECT MAX(PRIME_KEY) FROM " + Environmental.getDRCDatabase() + ".dbo.D014900 C " +
            //        " WHERE VO_MD_YEAR = @YEAR AND   " +
            //        "       VO_MAKE_NO = @MAKENO AND   " +
            //        "       VO_WEB_MODEL = @MODEL AND   " +
            //        "   VO_OLD_FLAG not in('Y','V'))";
            //}

            //DatabaseHelper dm = DatabaseHelper.getInstance();
            //SqlCommand myCommand = dm.getSQLCommand(sSql);

            //SqlParameter yearParam = myCommand.Parameters.Add("@YEAR", SqlDbType.VarChar);
            //yearParam.Value = year;
            //SqlParameter makeParam = myCommand.Parameters.Add("@MAKENO", SqlDbType.VarChar);
            //makeParam.Value = makeNo;
            //SqlParameter modelParam = myCommand.Parameters.Add("@MODEL", SqlDbType.VarChar);
            //modelParam.Value = model;

            //SqlDataReader reader = myCommand.ExecuteReader();
            //StringBuilder sb = new StringBuilder();

            //while (reader.Read())
            //{
            //    sb.Append("<Vehicle>");

            //    string bod = reader["VO_BODY"].ToString().Trim();
            //    sb.Append("<Body>" + bod + "</Body>");

            //    string sty = reader["VO_DESCRIPTION"].ToString().Trim();
            //    sb.Append("<Style>" + sty + "</Style>");

            //    string modelId = reader["VO_MODEL_ID"].ToString().Trim();
            //    sb.Append("<ModelID>" + modelId + "</ModelID>");

            //    string modelVIN = reader["vo_make_code"].ToString().Trim() + reader["vo_vin_no"].ToString().Trim();
            //    modelVIN = SecurityElement.Escape(modelVIN);
            //    sb.Append("<ModelVIN>" + modelVIN + "</ModelVIN>");

            //    string vehType = reader["vo_veh_type"].ToString().Trim();
            //    sb.Append("<VehicleType>" + vehType + "</VehicleType>");

            //    string voPerf = reader["vo_performance"].ToString().Trim();
            //    sb.Append("<Performance>" + voPerf + "</Performance>");

            //    string voSymb = reader["vo_symbol"].ToString().Trim();
            //    //jrenz 10/04/2006 add Symbol to drop down
            //    string uiString = bod + " " + sty + " (Symb " + voSymb.PadLeft(3) + ", " + modelVIN + ")";

            //    if (uiString.IndexOf("MAN") > 0)
            //        uiString = uiString.Replace("MAN", "") + " M";
            //    if (uiString.IndexOf("AUTO") > 0)
            //        uiString = uiString.Replace("AUTO", "") + " A";

            //    while (uiString.IndexOf("  ") > 0)
            //        uiString = uiString.Replace("  ", " ");

            //    sb.Append("<Symbol>" + uiString + "</Symbol>");

            //    string voSymbLiab = reader["vo_symbol_5"].ToString().Trim();
            //    sb.Append("<SymbolLiab>" + voSymbLiab + "</SymbolLiab>");

            //    string voSymbComp = reader["vo_symbol_6"].ToString().Trim();
            //    sb.Append("<SymbolComp>" + voSymbComp + "</SymbolComp>");

            //    string voSymbColl = reader["vo_symbol_7"].ToString().Trim();
            //    sb.Append("<SymbolColl>" + voSymbColl + "</SymbolColl>");

            //    string voSymbPip = reader["vo_symbol_8"].ToString().Trim();
            //    sb.Append("<SymbolPip>" + voSymbPip + "</SymbolPip>");

            //    //SSR6871 PRD18507 WLU 9/27/2011
            //    string voSymbIsoComp = reader["vo_symbol_2"].ToString().Trim();
            //    sb.Append("<SymbolIsoComp>" + voSymbIsoComp + "</SymbolIsoComp>");

            //    string voSymbIsoColl = reader["vo_symbol_3"].ToString().Trim();
            //    sb.Append("<SymbolIsoColl>" + voSymbIsoColl + "</SymbolIsoColl>");

            //    string voAdjust = reader["vo_adjust_to_make_model"].ToString().Trim();
            //    sb.Append("<AdjToMakeModel>" + voAdjust + "</AdjToMakeModel>");

            //    string voSafeVeh = reader["vo_safe_vehicle"].ToString().Trim();
            //    sb.Append("<SafeVehicle>" + voSafeVeh + "</SafeVehicle>");

            //    string voExposure = reader["vo_exposure"].ToString().Trim();
            //    sb.Append("<Exposure>" + voExposure + "</Exposure>");

            //    //jrenz #4353 11/28/2006 
            //    string voYearMakeModel = reader["VO_YEAR_MAKE_MODEL"].ToString().Trim();
            //    sb.Append("<YearMakeModel>" + voYearMakeModel + "</YearMakeModel>");

            //    //jrenz #PRD01006 02/27/2007
            //    string voHiPerfInd = reader["vo_symbol_9"].ToString().Trim();
            //    sb.Append("<HiPerfInd>" + voHiPerfInd + "</HiPerfInd>");

            //    sb.Append("</Vehicle>");
            //}

            //dm.closeSQLCommand(myCommand);

            //string retstr = "";
            //if (sb.Length == 0)
            //{
            //    retstr = "<VINLookupReply><Valid>0</Valid><Message>No make found</Message></VINLookupReply>";
            //}
            //else
            //{
            //    retstr = "<VINLookupReply><Valid>1</Valid><Message>OK</Message>" +
            //        "<VehicleInfo>" + sb.ToString() + "</VehicleInfo>" +
            //        "</VINLookupReply>";
            //}

            //return retstr;
            return "";
        }

        public string GetVehicleYearMakeModel(string vin)
        {
            using (var context = new AutoQuoteEntitie7())
            {
                string retstr = "";
                if (vin.Length < 10)
                    return "<VINLookupReply><Valid>0</Valid><Message>No Model Found</Message></VINLookupReply>";

                string makeCode = vin.Trim().Substring(0, 3);
                string vinNo = System.Web.HttpUtility.HtmlDecode(vin).Trim().Substring(3,7);
                var vehicles = from v in context.Vehicles
                               where v.VO_MAKE_CODE == makeCode && v.VO_VIN_NO == vinNo
                               select v;

                if (vehicles.Count() == 0)
                {
                    retstr = "<VINLookupReply><Valid>0</Valid><Message>No Model Found</Message></VINLookupReply>";
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<VINLookupReply><Valid>1</Valid><Message>OK</Message>");
                    var veh = vehicles.First();
                    sb.Append("<VehicleInfo>");
                    sb.Append("<Vehicle>");

                    int iMakeNo = 0;
                    int.TryParse(veh.VO_MAKE_NO, out iMakeNo);
                    string mkNumberVer = iMakeNo.ToString();
                    var makes = from m in context.YearMakes
                                where m.MK_NUMBER_VER == mkNumberVer
                                    select m;
                    if (makes.Count() > 0)
                    {
                        var make = makes.First();
                        string makeDescription = make.MK_DESCRIP.Trim();
                        sb.Append("<Make>" + makeDescription + "</Make>");
                    }

                    string webModel = veh.VO_WEB_MODEL.Trim();
                    sb.Append("<WebModel>" + webModel + "</WebModel>");

                    string model = veh.VO_MODEL.Trim();
                    sb.Append("<Model>" + model + "</Model>");

                    string bod = veh.VO_BODY.Trim();
                    sb.Append("<Body1>" + bod + "</Body1>");

                    string sty = veh.VO_DESCRIPTION.Trim();
                    sb.Append("<Style>" + sty + "</Style>");

                    string modelId = veh.VO_MODEL_ID.Trim();
                    sb.Append("<ModelID>" + modelId + "</ModelID>");

                    string modelVIN = veh.VO_MAKE_CODE.Trim() + veh.VO_VIN_NO.Trim();
                    modelVIN = SecurityElement.Escape(modelVIN);
                    sb.Append("<ModelVIN>" + modelVIN + "</ModelVIN>");

                    string vehType = veh.VO_VEH_TYPE.ToString();
                    sb.Append("<VehicleType>" + vehType + "</VehicleType>");

                    string voSymb = veh.VO_SYMBOL.ToString();
                    //jrenz 10/04/2006 add Symbol to drop down
                    string uiString = bod + " " + sty + " (Symb " + voSymb.PadLeft(3) + ", " + modelVIN + ")";

                    if (uiString.IndexOf("MAN") > 0)
                        uiString = uiString.Replace("MAN", "") + " M";
                    if (uiString.IndexOf("AUTO") > 0)
                        uiString = uiString.Replace("AUTO", "") + " A";

                    while (uiString.IndexOf("  ") > 0)
                        uiString = uiString.Replace("  ", " ");

                    sb.Append("<Symbol>" + uiString + "</Symbol>");

                    string voSymbLiab = veh.VO_SYMBOL_5.ToString();
                    sb.Append("<SymbolLiab>" + voSymbLiab + "</SymbolLiab>");

                    string voSymbComp = veh.VO_SYMBOL_6.ToString();
                    sb.Append("<SymbolComp>" + voSymbComp + "</SymbolComp>");

                    string voSymbColl = veh.VO_SYMBOL_7.ToString();
                    sb.Append("<SymbolColl>" + voSymbColl + "</SymbolColl>");

                    string voSymbPip = veh.VO_SYMBOL_8.ToString();
                    sb.Append("<SymbolPip>" + voSymbPip + "</SymbolPip>");

                    //SSR6871 PRD18507 WLU 9/27/2011
                    string voSymbIsoComp = veh.VO_SYMBOL_2.ToString();
                    sb.Append("<SymbolIsoComp>" + voSymbIsoComp + "</SymbolIsoComp>");

                    string voSymbIsoColl = veh.VO_SYMBOL_3.ToString();
                    sb.Append("<SymbolIsoColl>" + voSymbIsoColl + "</SymbolIsoColl>");

                    string voAdjust = veh.VO_ADJUST_TO_MAKE_MODEL.ToString();
                    sb.Append("<AdjToMakeModel>" + voAdjust + "</AdjToMakeModel>");

                    string voSafeVeh = veh.VO_SAFE_VEHICLE.ToString();
                    sb.Append("<SafeVehicle>" + voSafeVeh + "</SafeVehicle>");

                    string voAntiLock = veh.VO_ANTI_LOCK_BRAKE.ToString();
                    sb.Append("<AntiLock>" + voAntiLock + "</AntiLock>");

                    string voRestraint = veh.VO_RESTRAINT.ToString();
                    sb.Append("<Restraint>" + voRestraint + "</Restraint>");

                    string voAirBag = veh.VO_AIR_BAG.ToString();
                    sb.Append("<AirBag>" + voAirBag + "</AirBag>");

                    string voExposure = veh.VO_EXPOSURE.ToString();
                    sb.Append("<Exposure>" + voExposure + "</Exposure>");

                    //jrenz #4353 11/28/2006 
                    string voYearMakeModel = veh.vo_year_make_model.Trim();
                    sb.Append("<YearMakeModel>" + voYearMakeModel + "</YearMakeModel>");

                    //jrenz #PRD01006 02/27/2007
                    string voHiPerfInd = veh.VO_PERFORMANCE.ToString();
                    sb.Append("<HiPerfInd>" + voHiPerfInd + "</HiPerfInd>");

                    sb.Append("</Vehicle>");
                    sb.Append("</VehicleInfo>");
                    sb.Append("</VINLookupReply>");
                    retstr = sb.ToString();
                }
                return retstr;
            }
        }

        //PRD15381 WLU 1/19/2011
        private string MappingRestraint(string sRestraint)
        {
            string sTemp = "";
            switch (sRestraint)
            {
                case "A":
                case "P":
                case "M":
                case "N":
                    sTemp = "9";     //not offered
                    break;
                case "B":
                case "H":
                case "X": 
                case "D":
                    sTemp = "1";    //driver side only
                    break;
                default:
                    sTemp = "2";    //both sides
                    break;
            }
            return sTemp;
        }

        //PRD15381 WLU 1/19/2011
        private string MappingAntiTheftInfo(string sAntiTheftInfo)
        {
            string sTemp = "";
            switch (sAntiTheftInfo)
            {
                case "A":
                    sTemp = "1";     //alarm only
                    break;
                case "P":
                case "E":
                    sTemp = "2";     //passive only
                    break;
                case "O":
                case "D":
                case "T":
                case "U":
                case "R":
                    sTemp = "5";     //alarm optional
                    break;
            }
            return sTemp;
        }

        //PRD15381 WLU 1/19/2011
        private string MappingAntiLockBrake(string sAntiLockBrake)
        {
            string sTemp = "";
            switch (sAntiLockBrake)
            {
                case "A":
                case "P":
                case "M":
                case "N":
                    sTemp = "9";     //not offered
                    break;
                case "B":
                case "H":
                case "X":
                case "D":
                    sTemp = "1";     //anti lock
                    break;
                default:
                    sTemp = "2";     //optional
                    break;
            }
            return sTemp;
        }
    }
}
