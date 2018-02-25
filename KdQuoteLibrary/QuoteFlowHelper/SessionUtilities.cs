using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using SynchronousPluginHelper;
using KdQuoteLibrary.Services;

namespace KdQuoteLibrary.QuoteFlowHelper
{
    public static class SessionUtilities
    {
        public static XElement UpdateCoveragesAndDiscounts(WebSession session)
        {
            XElement response;
            XElement request = XElement.Parse("<Request><Guid>" + session.Guid.ToString() + "</Guid></Request>");

            XMLSyncProcess process = new XMLSyncProcess(new XMLSyncHeader("UD3Plugin", "AutoQuoteServices", "LoadQuote"), request);
            using (ProcessWCF client = new ProcessWCF())
            {
                try
                {
                    response = client.Execute(process);
                    if (response == null)
                        return null;
                    if (response.Element("Coverages") != null)
                    {
                        if (response.Element("Coverages").Element("PolicyCoverages") != null)
                            foreach (XElement covElement in response.Element("Coverages").Element("PolicyCoverages").Elements())
                            {
                                Coverage cov = session.PolicyCoverages.Find(c => c.CovCode == covElement.GetValue("CovCode"));
                                if (cov != null)
                                    if (covElement.Element("Limits") != null)
                                        if (covElement.Element("Limits").Element("SelectedLimitValue") != null)
                                            covElement.Element("Limits").Element("SelectedLimitValue").Value = cov.SelectedLimit.Value;
                            }
                        if (response.Element("Coverages").Element("VehicleCoverages") != null)
                            foreach (XElement vehElement in response.Element("Coverages").Element("VehicleCoverages").Elements("Vehicle"))
                            {
                                VehicleCoverage vc = session.VehicleCoverages.Find(v => v.VehicleNumber == vehElement.GetValue("VehIndex"));
                                foreach (XElement covElement in vehElement.Elements("Coverage"))
                                {
                                    Coverage cov = vc.Coverages.Find(c => c.CovCode == covElement.GetValue("CovCode"));
                                    if (cov != null)
                                        if (covElement.Element("Limits") != null)
                                            covElement.Element("Limits").Element("SelectedLimitValue").Value = cov.SelectedLimit.Value;
                                }
                            }

                        if (response.Element("Coverages").Element("EnhancedCoverages") != null)
                            foreach (XElement covElement in response.Element("Coverages").Element("EnhancedCoverages").Elements())
                            {
                                Coverage cov = session.EnhancedCoverages.Find(c => c.CovCode == covElement.GetValue("CovCode"));
                                if (cov != null)
                                    if (covElement.Element("Purchased") != null)
                                    {
                                        covElement.Element("Purchased").Value = cov.Purchased ? "true" : "false";
                                        switch (cov.CovCode)
                                        {
                                            case "Bundle1":
                                                if (cov.Purchased)
                                                    ((WebSessionDRC)session).Quote.getPolicyInfo().setBundle1Test(1);
                                                else
                                                    ((WebSessionDRC)session).Quote.getPolicyInfo().setBundle1Test(0);
                                                break;
                                            case "Bundle2":
                                                if (cov.Purchased)
                                                    ((WebSessionDRC)session).Quote.getPolicyInfo().setBundle2Test(1);
                                                else
                                                    ((WebSessionDRC)session).Quote.getPolicyInfo().setBundle2Test(0);
                                                break;
                                            case "Bundle3":
                                                if (cov.Purchased)
                                                    ((WebSessionDRC)session).Quote.getPolicyInfo().setBundle3Test(1);
                                                else
                                                    ((WebSessionDRC)session).Quote.getPolicyInfo().setBundle3Test(0);
                                                break;
                                        }
                                    }
                            }
                    }

                    //StringBuilder sb = new StringBuilder();
                    //foreach (Discount dis in session.CoveragePageDiscounts)
                    //    sb.Append("session.Discounts[x].Name=" + dis.Name + ",.Purchased=" + dis.Purchased.ToString() + ";");
                    //LoggingServices.Instance.logError("CoveragePageDiscounts: " + sb.ToString(), "SessionServices.UpdateCoveragesAndDiscounts", UDILibrary.Log.LogSeverity.Error);

                    if (response.Element("Discounts").Element("DiscountCoverages") != null)
                        //sb = new StringBuilder();
                        foreach (XElement disElement in response.Element("Discounts").Element("DiscountCoverages").Elements())
                        {
                            string name = disElement.GetValue("Name").FormatDiscountDescription((WebSessionDRC)session);
                            if (name.Contains("Network"))
                            {
                                name = name.Replace("iMingle", "");
                            }
                            //sb.Append("name=" + name + ",.Purchased=" + disElement.GetValue("Purchased") + ";");

                            Discount dis = session.CoveragePageDiscounts.Find(d => d.Name == name);

                            if (dis != null)
                            {
                                //LoggingServices.Instance.logError("disElement.GetValue(Name)=" + disElement.GetValue("Name") + ",dis.ID=" + dis.ID + ",dis.Name=" + dis.Name + ",disElement.Purchased=" + disElement.Element("Purchased").Value + ",dis.Purchased=" + dis.Purchased, "SessionServices.UpdateCoveragesAndDiscounts", UDILibrary.Log.LogSeverity.Error);

                                disElement.Element("Purchased").Value = dis.Purchased ? "true" : "false";
                                if (name.Contains("Network"))
                                {
                                    if (dis.Purchased)
                                    {
                                        session.AddInfo.iminglediscount = "1";
                                        ((WebSessionDRC)session).Quote.getPolicyInfo().setMingleMateDis(1);
                                        ((WebSessionDRC)session).Quote.getPolicyInfo().setMinglePledgeTest(1);
                                    }
                                    else
                                    {
                                        session.AddInfo.iminglediscount = "0";
                                        ((WebSessionDRC)session).Quote.getPolicyInfo().setMingleMateDis(0);
                                        ((WebSessionDRC)session).Quote.getPolicyInfo().setMinglePledgeTest(0);
                                        foreach (Discount di in session.Discounts)
                                        {
                                            if (di.ID == "NetworkDiscount")
                                                di.Purchased = false;
                                        }
                                    }
                                }
                            }
                        }
                    //LoggingServices.Instance.logError("responseDsicountElements: " + sb.ToString(), "SessionServices.UpdateCoveragesAndDiscounts", UDILibrary.Log.LogSeverity.Error);

                    if (response.Element("PayPlans") != null)
                    {
                        string[] valuesArray = session.SelectedPayPlan.Value.Split('~');
                        if (valuesArray.Length > 7)
                            response.Element("PayPlans").Element("SelectedPayPlan").Value = valuesArray[7];

                    }
                    if (response.Element("InstantRenters") != null)
                    {
                        if (response.Element("InstantRenters").Element("HOIRenterInfo") != null)
                        {
                            if (response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterProvide").Value == "YES")
                            {
                                if (response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterProperty") != null)
                                    response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterProperty").Value = session.AddInfo.HOIRenterInfo.HOIRenterProperty.ToString();
                                if (response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterLiability") != null)
                                    response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterLiability").Value = session.AddInfo.HOIRenterInfo.HOIRenterLiability.ToString();
                                if (response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterDeductible") != null)
                                    response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterDeductible").Value = session.AddInfo.HOIRenterInfo.HOIRenterDeductible.ToString();
                                if (response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterInculded") != null)
                                    response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterInculded").Value = session.AddInfo.HOIRenterInfo.HOIRenterInculded.ToString();
                                if (response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterPremium") != null)
                                    response.Element("InstantRenters").Element("HOIRenterInfo").Element("HOIRenterPremium").Value = session.AddInfo.HOIRenterInfo.HOIRenterPremium.ToString();
                            }
                        }
                    }
                    return response;
                }
                catch (Exception ex)
                {
                    LoggingServices.Instance.logError(ex.Message + ";guid=" + session.Guid, "UpdateCoveragesAndDiscounts", UDILibrary.Log.LogSeverity.Error);
                    LoggingServices.Instance.logError(ex.StackTrace, "UpdateCoveragesAndDiscounts", UDILibrary.Log.LogSeverity.Error);
                    session.AddErrorMessage("UpdateCoveragesAndDiscounts", session.AddInfo.CurrentPage, "KdQuoteLibrary", ex.Message);
                    return null;
                }
            }

        }
        public static bool RecalculateQuote(XElement request, WebSession session)
        {
            bool getNonVibeMPD = false;
            double mpdPrem = 0;
            double nonMpdPrem = 0;
            if (session.AddInfo.HOIRenterInfo.HOIRenterProvide == HOIRenterInfo.EnumRenterProvide.Yes)
            {
                //rate twice to get savings for non vibe.
                if (!session.IsVibeState)
                    getNonVibeMPD = true;
            }
            XMLSyncProcess process;
            XElement response;
            using (ProcessWCF client = new ProcessWCF())
            {
                try
                {
                    if (getNonVibeMPD) //rate twice to get with and w/out MPD
                    {
                        if (request.Element("NonVibeMPDIndicator") != null)
                            request.Element("NonVibeMPDIndicator").Value = "2";
                        else
                            request.Add(new XElement("NonVibeMPDIndicator", "2"));
                        process = new XMLSyncProcess(new XMLSyncHeader("UD3Plugin", "AutoQuoteServices", "RecalculateQuote"), request);
                        response = client.Execute(process);
                        //response = new UD3Plugin.AutoQuoteServices().RecalculateQuote(request);
                        if ((response.Element("Coverages") != null) &&
                            (response.Element("Coverages").Element("CalculatedPremiums") != null))
                        {
                            double.TryParse(response.Element("Coverages").Element("CalculatedPremiums").Element("Amount").Value, out mpdPrem);
                        }
                        if (response.Element("Coverages").Element("EditErrors") == null)
                        {
                            request.Element("NonVibeMPDIndicator").Value = "0";
                            process = new XMLSyncProcess(new XMLSyncHeader("UD3Plugin", "AutoQuoteServices", "RecalculateQuote"), request);
                            response = client.Execute(process);
                            //response = new UD3Plugin.AutoQuoteServices().RecalculateQuote(request);
                            double.TryParse(response.Element("Coverages").Element("CalculatedPremiums").Element("Amount").Value, out nonMpdPrem);
                            session.AddInfo.multipolicydiscountNumeric = (decimal)(nonMpdPrem - mpdPrem);
                            session.AddInfo.multipolicydiscount = session.AddInfo.multipolicydiscountNumeric.ToString("C");
                        }

                    }
                    else
                    {
                        process = new XMLSyncProcess(new XMLSyncHeader("UD3Plugin", "AutoQuoteServices", "RecalculateQuote"), request);
                        response = client.Execute(process);
                        //response = new UD3Plugin.AutoQuoteServices().RecalculateQuote(request);
                    }

                    session.PolicyCoverageErrors = new List<CoverageError>();
                    session.VehicleCoverageErrors = new List<CoverageError>();

                    if (response.Element("Quote-Errors") != null)
                    {
                        CoverageError err = new CoverageError();
                        err.Message = response.Element("Quote-Errors").Value;
                        session.PolicyCoverageErrors.Add(err);
                        LoggingServices.Instance.logError("PolicyCoverageErrors:" + err, "RecalculateQuote", UDILibrary.Log.LogSeverity.Info);
                    }
                    if ((response.Element("Coverages") != null) &&
                        (response.Element("Coverages").Element("EditErrors") != null))
                    {
                        foreach (XElement errorElement in response.Element("Coverages").Element("EditErrors").Elements("EditError"))
                        {
                            CoverageError err = new CoverageError();
                            err.Message = errorElement.GetValue("EditMessage");
                            if (errorElement.Element("Coverage") != null)
                            {
                                XElement covElement = errorElement.Element("Coverage");
                                err.CovCode = covElement.GetValue("CovCode");
                                err.VehIndex = covElement.GetValue("VehIndex");
                                if ((covElement.GetValue("VehIndex") == "") ||
                                    (covElement.GetValue("VehIndex") == "-1"))

                                    session.PolicyCoverageErrors.Add(err);
                                else
                                    session.VehicleCoverageErrors.Add(err);
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("<FilterID>"))
                    {
                        session.AddInfo.DNQ.Filter = ex.Message.Substring(ex.Message.IndexOf("<FilterID>") + 10, ex.Message.Length - ex.Message.IndexOf("</FilterID>"));
                        return true;
                    }
                    if (ex.Message.Contains("<DNQ-Quote>"))
                    {
                        if (ex.Message.Contains("<DNQReason>"))
                        {
                            session.AddInfo.DNQ.Reason = ex.Message.Substring(ex.Message.IndexOf("<DNQReason>") + 11, ex.Message.Length - ex.Message.IndexOf("</DNQReason>"));
                        }
                        if (ex.Message.Contains("<DNQDescription>"))
                        {
                            session.AddInfo.DNQ.Description = ex.Message.Substring(ex.Message.IndexOf("<DNQDescription>") + 16, ex.Message.Length - ex.Message.IndexOf("</DNQDescription>"));
                        }
                        session.AddInfo.DNQ.Knockout = "yes";
                        return true;
                    }
                    session.AddErrorMessage("Recalculate", session.AddInfo.CurrentPage, "KdQuoteLibrary", ex.Message);
                    session.AddErrorMessage("Recalculate", session.AddInfo.CurrentPage, "KdQuoteLibrary", ex.StackTrace);
                    LoggingServices.Instance.logError(ex.Message, "Recalculate", UDILibrary.Log.LogSeverity.Info);
                    LoggingServices.Instance.logError(ex.StackTrace, "Recalculate", UDILibrary.Log.LogSeverity.Info);

                    return false;
                }
            }

        }
        public static bool GetQuote(WebSession session)
        {
            XElement request = new XElement("Request", new XElement("QuoteNo", ((WebSessionDRC)session).Quote.getQuoteInfo().getQuoteNo0()));
            XMLSyncProcess process = new XMLSyncProcess(new XMLSyncHeader("UD3Plugin", "AutoQuoteServices", "GetQuote"), request);

            using (ProcessWCF client = new ProcessWCF())
            {
                try
                {
                    XElement response = client.Execute(process);
                    //XElement response = new UD3Plugin.AutoQuoteServices().GetQuote(request);
                    ((WebSessionDRC)session).Quote = new AutoQuote.Autoquote();
                    ((WebSessionDRC)session).Quote.deserialize(response.ToString(), null);
                    return true;
                }
                catch (Exception ex)
                {
                    session.AddErrorMessage("GetQuote", session.AddInfo.CurrentPage, "KdQuoteLibrary", ex.Message);
                    session.AddErrorMessage("GetQuote", session.AddInfo.CurrentPage, "KdQuoteLibrary", ex.StackTrace);

                    return false;
                }
            }
        }
    }
}
