using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;

namespace KdQuoteLibrary.QuoteFlowHelper
{
    public static class LinqUtilities
    {
        public static List<Coverage> DeserializePolicyCoverages(XElement coveragesXml)
        {
            return (from c in coveragesXml.Element("PolicyCoverages").Elements()
                    select new Coverage()
                    {
                        CovCode = c.Element("CovCode").Value,
                        CovInputType = c.Element("CovInputType").Value,
                        IncludeCaptionInLayout = !(c.Element("CovCode").Value == "pipProfile"),
                        IncludePremiumInLayout = !(c.Element("CovCode").Value == "pipProfile"),
                        Caption = c.Element("Desc").Value,
                        Name = c.Element("Desc").Value,
                        HelpText = c.Element("FAQText").Value,
                        SuppressRendering = c.Element("SuppressRendering").Value.ToLower() == "true",
                        WebQuestionID = c.Element("WebQuestionID").Value,
                        SelectedValue = c.Element("Limits") == null ? "" : c.Element("Limits").Element("SelectedLimitValue").Value,
                        SelectedLimit = c.Element("Limits") == null ? new Limit()
                                                    {
                                                        Abbrev = "",
                                                        Caption = "",
                                                        IsNoCov = "",
                                                        Value = "",
                                                        SortOrder = ""
                                                    } :
                                        (from l in c.Element("Limits").Elements("Limit")
                                         select new Limit()
                                         {
                                             Abbrev = l.Element("Abbrev").Value,
                                             Caption = l.Element("Caption").Value,
                                             Value = l.Element("Value").Value,
                                             SortOrder = l.Element("SortOrder").Value,
                                             IsNoCov = l.Element("IsNoCov").Value,
                                         })
                                         .DefaultIfEmpty(
                                             new Limit()
                                             {
                                                 Abbrev = "",
                                                 Caption = c.Element("CovInputType").Value == "label" ? c.Element("LabelDescription").Value : "",
                                                 Value = c.Element("CovInputType").Value == "label" ? c.Element("LabelValue").Value : "",
                                                 SortOrder = "",
                                                 IsNoCov = "",
                                             }
                                         )
                                         .FirstOrDefault(lim => lim.Value == c.Element("Limits").Element("SelectedLimitValue").Value),
                        Limits = c.Element("Limits") == null ? new List<Limit>() : 
                                          (from l in c.Element("Limits").Elements("Limit")
                                          select new Limit()
                                          {
                                              Abbrev = l.Element("Abbrev").Value,
                                              Caption = l.Element("Caption").Value,
                                              Value = l.Element("Value").Value,
                                              SortOrder = l.Element("SortOrder").Value,
                                              IsNoCov = l.Element("IsNoCov").Value,
                                          }).ToList(),
                        Premium = (from p in coveragesXml.Element("PolicyPremiums").Elements("Premium")
                                   where p.Element("CovCode").Value == c.Element("CovCode").Value
                                   select (p.Element("Amount").Value == "" ? "" : string.Format("{0:c}", decimal.Parse(p.Element("Amount").Value)))).DefaultIfEmpty("").First(),
                        PremiumNumeric = decimal.Parse((from p in coveragesXml.Element("PolicyPremiums").Elements("Premium")
                                                        where p.Element("CovCode").Value == c.Element("CovCode").Value
                                                        select (p.Element("Amount").Value == "" ? "0" : p.Element("Amount").Value)).DefaultIfEmpty("0").First())

                    }).ToList();
        }

        public static List<VehicleCoverage> DeserializeVehicleCoverages(XElement coveragesXml)
        {
            return (from v in coveragesXml.Element("VehicleCoverages").Elements("Vehicle")
                    select new VehicleCoverage()
                    {
                        Year = v.Element("Year").Value,
                        Make = v.Element("Make").Value,
                        Model = v.Element("Model").Value,
                        VehicleNumber = v.Element("VehIndex").Value,
                        Coverages = (from c in v.Elements("Coverage")
                                     select new Coverage()
                                     {
                                         CovCode = c.Element("CovCode").Value,
                                         Name = c.Element("Desc").Value,
                                         HelpText = c.Element("FAQText").Value,
                                         SuppressRendering = c.Element("SuppressRendering").Value.ToLower() == "true",
                                         CovInputType = c.Element("CovInputType").Value,
                                         WebQuestionID = c.Element("WebQuestionID").Value,
                                         SelectedValue = c.Element("Limits") == null ? "" : c.Element("Limits").Element("SelectedLimitValue").Value,
                                         SelectedLimit = c.Element("Limits") == null ?
                                                         new Limit()
                                                         {
                                                             Abbrev = "",
                                                             Caption = "",
                                                             IsNoCov = "",
                                                             Value = "",
                                                             SortOrder = ""
                                                         } :
                                                         (from l in c.Element("Limits").Elements("Limit")
                                                          select new Limit()
                                                          {
                                                              Abbrev = l.Element("Abbrev").Value,
                                                              Caption = l.Element("Caption").Value,
                                                              Value = l.Element("Value").Value,
                                                              SortOrder = l.Element("SortOrder").Value,
                                                              IsNoCov = l.Element("IsNoCov").Value,
                                                          })
                                                          .DefaultIfEmpty(
                                                              new Limit()
                                                              {
                                                                  Abbrev = "",
                                                                  Caption = c.Element("CovInputType").Value == "label" ? c.Element("LabelDescription").Value : "",
                                                                  Value = c.Element("CovInputType").Value == "label" ? c.Element("LabelValue").Value : "",
                                                                  SortOrder = "",
                                                                  IsNoCov = "",
                                                              }
                                                          )
                                                          .First(lim => lim.Value == c.Element("Limits").Element("SelectedLimitValue").Value),
                                         Limits = c.Element("Limits") == null ? new List<Limit>() : 
                                                  (from l in c.Element("Limits").Elements("Limit")
                                                   select new Limit()
                                                   {
                                                       Abbrev = l.Element("Abbrev").Value,
                                                       Caption = l.Element("Caption").Value,
                                                       Value = l.Element("Value").Value,
                                                       SortOrder = l.Element("SortOrder").Value,
                                                       IsNoCov = l.Element("IsNoCov").Value,
                                                   }).ToList(),
                                         Premium = (from vp in coveragesXml.Element("VehiclePremiums").Elements("Vehicle")
                                                    where vp.Element("VehIndex").Value == v.Element("VehIndex").Value
                                                    select (from p in vp.Elements("Premium")
                                                            where p.Element("CovCode").Value == c.Element("CovCode").Value
                                                            select (p.Element("Amount").Value == "" ? "" : string.Format("{0:c}", decimal.Parse(p.Element("Amount").Value)))
                                                                    ).First()
                                                            ).First(),
                                         PremiumNumeric = decimal.Parse((from vp in coveragesXml.Element("VehiclePremiums").Elements("Vehicle")
                                                                         where vp.Element("VehIndex").Value == v.Element("VehIndex").Value
                                                                         select (from p in vp.Elements("Premium")
                                                                                 where p.Element("CovCode").Value == c.Element("CovCode").Value
                                                                                 select (p.Element("Amount").Value == "" ? "0" : p.Element("Amount").Value)
                                                                                         ).First()
                                                            ).First())

                                     }).ToList()

                    }).ToList();
        }

        public static List<EnhancedCoverage> DeserializeEnhancedCoverages(XElement coveragesXml)
        {
            return (from c in coveragesXml.Element("EnhancedCoverages").Elements("EnhancedCoverage")
                    select new EnhancedCoverage()
                    {
                        CovCode = c.Element("CovCode").Value,
                        Name = c.Element("Name").Value.Replace("iMingle", ""),
                        CovInputType = c.Element("CovInputType").Value,
                        WebQuestionID = c.Element("WebQuestionID").Value,
                        Desc = c.Element("Name").Value == "Protect Your Sanity" ?
                            "When the unexpected happens, get back on the road with Roadside Assistance, <br>Rental Car Reimbursement and Trip Interruption."
                            : c.Element("Name").Value == "Protect Your Family" ?
                            "Keep your loved ones better protected with Enhanced Car Seat Replacement, <br>Pet Protection and Dependent Protection."
                            : c.Element("Desc").Value,
                        Premium = (from p in coveragesXml.Element("EnhancedPremiums").Elements("Premium")
                                   where p.Element("Name").Value == c.Element("Name").Value
                                   select (p.Element("Amount").Value == "" ? "" : string.Format("{0:c}", decimal.Parse(p.Element("Amount").Value)))).DefaultIfEmpty("").First(),
                        PremiumNumeric = decimal.Parse((from p in coveragesXml.Element("EnhancedPremiums").Elements("Premium")
                                                        where p.Element("Name").Value == c.Element("Name").Value
                                                        select (p.Element("Amount").Value == "" ? "0" : p.Element("Amount").Value)).DefaultIfEmpty("0").First()),
                        Bundle = Utilities.GetBundles(c.Element("Name").Value)
                    }).ToList();


        }

        public static List<Discount> DeserializeCoverageDiscounts(XElement coveragesXml, WebSessionDRC session)
        {
            return (from d in coveragesXml.Element("Discounts").Element("DiscountCoverages").Elements()
                    where !(d.Element("Name").Value.Contains("Multi-Policy") &&
                    coveragesXml.Element("GeneralInfo").Element("Customer").Element("IsHOOfferedInDRC").Value.ToLower() == "false")
                    select new Discount()
                    {
                        Name = Utilities.FormatDiscountDescription(d.Element("Name").Value, session.Quote.getPolicyInfo().getMarketBrand(), session.AddInfo.Referral),
                        ShortDescription = Utilities.FormatDiscountDescription(d.Element("Description").Value, session.Quote.getPolicyInfo().getMarketBrand(), session.AddInfo.Referral),
                        ExpandedDesc = Utilities.FormatDiscountDescription(d.Element("ExpandedDesc").Value, session.Quote.getPolicyInfo().getMarketBrand(), session.AddInfo.Referral),
                        Purchased = d.Element("Purchased").Value.ToLower() == "true" || d.Element("Purchased").Value == "1",
                        CanBeDeleted = d.Element("CanBeDeleted").Value.ToLower() == "true",
                        ID = Utilities.GetDiscountID(d.Element("Name").Value),
                        Amount = (from p in coveragesXml.Element("Discounts").Element("DiscountPremiums").Elements()
                                  where p.Element("Name").Value == d.Element("Name").Value
                                  select (p.Element("Amount").Value == "" ? "" : string.Format("{0:c}", decimal.Parse(p.Element("Amount").Value)))).First(),
                        AmountNumeric = decimal.Parse((from p in coveragesXml.Element("Discounts").Element("DiscountPremiums").Elements()
                                                       where p.Element("Name").Value == d.Element("Name").Value
                                                       select (p.Element("Amount").Value == "" ? "0" : p.Element("Amount").Value)).First())
                    }).ToList();
        }

        //TODO convert this for loop to Linq query
        public static void GetDiscounts(WebSessionDRC session, XElement response)
        {
            session.Discounts = new List<Discount>();
            if ((response.Element("Discounts") != null) &&
                (response.Element("Discounts").Element("DiscountPremiums") != null))
            {
                foreach (XElement e in response.Element("Discounts").Element("DiscountPremiums").Elements())
                {
                    Discount newDisc = new Discount() { Name = e.Element("Name").Value.FormatDiscountDescription(session), Amount = e.Element("Amount").Value };
                    if (newDisc.Name.Contains("Preferred"))
                        newDisc.ID = "PreferredPayerDiscount";
                    if (newDisc.Name.Contains("Multi-Car"))
                        newDisc.ID = "MultiCarDiscount";
                    if (newDisc.Name.Contains("Homeownership"))
                        newDisc.ID = "HomeownershipDiscount";
                    if (newDisc.Name.Contains("Network"))
                    {
                        newDisc.ID = "NetworkDiscount";
                        newDisc.Name = newDisc.Name.Replace("iMingle", "");
                    }
                    if (newDisc.Name.Contains("Anti-Theft"))
                        newDisc.ID = "AntiTheftDiscount";
                    if (newDisc.Name.Contains("Free-A-Tree"))
                        newDisc.ID = "FreeATree";
                    if (newDisc.Name.Contains("Focused Driver"))
                        newDisc.ID = "FocusedDriver";
                    if (newDisc.Name.Contains("Multi-Policy"))
                        newDisc.ID = "MultiPolicy";
                    if (newDisc.Name.Contains("Happily Married"))
                        newDisc.ID = "HappilyMarried";
                    if (newDisc.Name.Contains("Safe"))
                        newDisc.ID = "SafeAndSound";
                    if (newDisc.Name.Contains("Passive Restraint"))
                        newDisc.ID = "PassiveRestraint";
                    if (newDisc.Name.Contains("Retro Loyalty"))
                        newDisc.ID = "RetroLoyaltyDiscount";
                    if (response.Element("Discounts").Element("DiscountCoverages") != null)
                    {
                        foreach (XElement e2 in response.Element("Discounts").Element("DiscountCoverages").Elements())
                        {
                            if ((e2.Element("Name") != null) &&
                                (e2.Element("Name").Value.FormatDiscountDescription(session) == newDisc.Name))
                            {
                                if (e2.Element("Description") != null)
                                    newDisc.Description = e2.Element("Description").Value;
                                if (e2.Element("ExpandedDesc") != null)
                                    newDisc.Description += e2.Element("ExpandedDesc").Value;
                                newDisc.Description.FormatDiscountDescription(session);
                                break;
                            }
                        }
                    }
                    session.Discounts.Add(newDisc);
                }
            }
        }
        //TODO
        //public static List<PayPlan> DeserializePayPlans(XElement payPlansXml, WebSession session)
        //{
        //    string termText = (session.Quote.getPolicyInfo().getTermFactor() == .5) ? " total" : "/year";
        //    return (from p in payPlansXml.Elements("PayPlan")
        //            select new PayPlan()
        //            {
        //                Name = p.Element("Name").Value,
        //                ID = p.Attribute("id").Value,
        //                Discount = p.Element("PreferredPayerDiscount") == null ? 0 : decimal.Parse(p.Element("PreferredPayerDiscount").Value),
        //                Yearly = session.TotalPremium - session.TotalDiscountSavingsWithoutPreferredPayer - (p.Element("PreferredPayerDiscount") == null ? 0 : decimal.Parse(p.Element("PreferredPayerDiscount").Value),
        //                Downpayment = GetDownPayment(p.Element("Name").Value),
        //                Installment = GetInstallment(p.Element("Name").Value),
        //            }).ToList();
        //}
    }
}
