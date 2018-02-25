using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Xsl;

using AutoQuoteLibrary;

namespace AutoQuoteLibrary.BL
{
    internal class AccidentViolationPlugin 
    {
        //private AccidentViolationDAO _accidentViolationDAO = new AccidentViolationDAO();

        /// <summary>
        /// Provides for Abstract XML that allows for State Specific Overrides
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public XElement Load(String state)
        {
            XDocument accidentViolations = XDocument.Load(ConfigurationManager.AppSettings["PluginsPath"] + "\\QuoteFlowData\\AccidentViolations\\BaseAccidentViolations.xml");

            if (File.Exists(ConfigurationManager.AppSettings["PluginsPath"] + "\\QuoteFlowData\\AccidentViolations\\" + state.ToUpper() + "AccidentViolations.xml"))
            {
                XDocument overrides = XDocument.Load(ConfigurationManager.AppSettings["PluginsPath"] + "\\QuoteFlowData\\AccidentViolations\\" + state.ToUpper() + "AccidentViolations.xml");

                //SSR07928 udiaes 10/25/2011 expand to drive state-specific values solely from xml
                //  replaces stateOverrides routine
                foreach (XElement accidentViolation in overrides.Root.Elements())
                {
                    String type = accidentViolation.Attribute("Type").Value;
                    String id = accidentViolation.Attribute("ID").Value;
                    String action = accidentViolation.Attribute("Action").Value;
                    XElement baseAccidentViolation = accidentViolations.Root.Elements().Where(node => node.Attribute("Type").Value == type && node.Attribute("ID").Value == id).FirstOrDefault();

                    if (baseAccidentViolation != null)
                    {
                        if (action == "remove")
                        {
                            baseAccidentViolation.Remove();
                        }
                        else
                        {
                            baseAccidentViolation.ReplaceWith(accidentViolation);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(action) && action == "add")
                        {
                            accidentViolations.Root.Add(accidentViolation);
                        }
                    }
                }
            }

            //String keyAccidents = "AccidentViolation_Accidents";
            //String keyViolations = "AccidentViolation_Violations";

            //DataTable dtAccidents = (DataTable)CacheManager.GetData(keyAccidents);
            //DataTable dtViolations = (DataTable)CacheManager.GetData(keyViolations);

            //if (dtAccidents == null)
            //{
            //    dtAccidents = _accidentViolationDAO.GetAccidents();
            //    CacheManager.Add(keyAccidents, dtAccidents, CacheManager.ExpireEverySundayAtThree);
            //}

            //if (dtViolations == null)
            //{
            //    dtViolations = _accidentViolationDAO.GetViolations();
            //    CacheManager.Add(keyViolations, dtViolations, CacheManager.ExpireEverySundayAtThree);
            //}

            //foreach (XElement accidentViolation in accidentViolations.Root.Elements())
            //{
            //    if (accidentViolation.Attribute("Type").Value == "accident" || accidentViolation.Attribute("Type").Value == "loss")
            //    {
            //        DataRow row = dtAccidents.Select("ad_acc_ver_number = " + accidentViolation.Element("Number").Value).First();
            //        accidentViolation.Add(new XElement("Injury", row["ad_acc_injury"]));
            //        accidentViolation.Add(new XElement("Sdip", row["ad_acc_sdip"]));
            //        accidentViolation.Add(new XElement("Incident", row["ad_acc_in_number"]));
            //    }
            //    else if (accidentViolation.Attribute("Type").Value == "violation")
            //    {
            //        DataRow row = dtViolations.Select("vd_vio_ver_number = " + accidentViolation.Element("Number").Value).First();
            //        accidentViolation.Add(new XElement("Type", row["vd_vio_type"]));
            //        accidentViolation.Add(new XElement("Incident", row["vd_vio_in_number"]));
            //    }
            //}

            //SSR07928 udiaes 10/25/2011 
            //// dmetz 04-14-2011 SSR7695 - New PA At-Fault Accident Threshold
            //stateOverrides(state, accidentViolations);
            //udinzs ssr8551 KS added sort by description
            //sort the output
            List<XElement> ordered = accidentViolations.Root.Elements("AccidentViolation")
                .OrderBy(element => element.Attribute("Type").Value)
                .OrderBy(element => element.Element("Description").Value)
                .ToList();
            // Clear everything from the root element         
            accidentViolations.Root.RemoveAll();
            accidentViolations.Root.Add(ordered);

            return accidentViolations.Root;
        }

		// dmetz 04-14-2011 SSR7695 - New PA At-Fault Accident Threshold
		private void stateOverrides(String state, XDocument accidentViolations)
		{
			if (state.ToUpper() != "PA")
			{
				accidentViolations.Element("AccidentViolations").Elements().Where(element => element.Attribute("Type").Value == "accident" && element.Attribute("ID").Value == "2041").Remove();
			}
			// dmetz 09-02-2011 SSR6871 - CT, KS, TN
			if (state.ToUpper() != "CT" && state.ToUpper() != "TN")
			{
				accidentViolations.Element("AccidentViolations").Elements().Where(element => element.Attribute("Type").Value == "accident" && element.Attribute("ID").Value == "3141+").Remove();
				accidentViolations.Element("AccidentViolations").Elements().Where(element => element.Attribute("Type").Value == "accident" && element.Attribute("ID").Value == "3141-").Remove();
			}
		}
    }
}
