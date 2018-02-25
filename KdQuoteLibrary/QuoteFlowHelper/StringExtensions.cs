using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace KdQuoteLibrary.QuoteFlowHelper
{
    public static class StringExtensions
    {
        public static string CapitalizeFirstLetter(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";
            if (input.Length > 1)
                return input.Substring(0, 1).ToUpper() + input.Substring(1);
            else
                return input.ToUpper();
        }
        public static bool ValidYear(this string year)
        {
            int iYear = 0;
            int.TryParse(year, out iYear);
            if (iYear >= 0)
                return true;
            else
                return false;
        }
        public static bool ValidMonth(this string month)
        {
            return ("01,02,03,04,05,06,07,08,09,10,11,12".Contains(month.PadLeft(2,'0')));
        }
        public static bool ValidDayInMonth(this string day, string month)
        {
            int iDay = 0;
            int.TryParse(day, out iDay);
            int iMonth = 0;
            int.TryParse(month, out iMonth);
            switch (iMonth)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    if ((iDay > 0) &&
                        (iDay < 32))
                        return true;
                    else
                        return false;
                case 2:
                    if ((iDay > 0) &&
                        (iDay < 30))
                        return true;
                    else
                        return false;
                case 4:
                case 6:
                case 9:
                case 11:
                    if ((iDay > 0) &&
                        (iDay < 31))
                        return true;
                    else
                        return false;
                default:
                    return false;
            }
        }
        public static string ConvertState(this string sPass)
        {
            string states = "~AL~AZ~AR~CA~CO~CT~DE~DC~FL~GA~ID~IL~IN~IA~KS~KY~LA~ME~MD~MA~MI~MN~MS~MO~MT~NE~NV~NH~NJ~NM~NY~NC~ND~OH~OK~OR~PA~RI~SC~SD~TN~TX~UT~VT~VA~WA~WV~WI~WY~HI~AK";
            string scodes = "~01~02~03~04~05~06~07~08~09~10~11~12~13~14~15~16~17~18~19~20~21~22~23~24~25~26~27~28~29~30~31~32~33~34~35~36~37~38~39~40~41~42~43~44~45~46~47~48~49~52~54";

            if (IsNumeric(sPass))
            {
                if (sPass.ToString().Length == 1)
                    sPass = "0" + sPass;

                if (scodes.IndexOf("~" + sPass) == -1)
                    return "";

                return states.Substring(scodes.IndexOf("~" + sPass) + 1, 2);
            }
            else
            {
                if (states.IndexOf("~" + sPass) == -1)
                    return "";

                return scodes.Substring(states.IndexOf("~" + sPass) + 1, 2);
            }
        }
        public static bool IsNumeric(this string sPass)
        {
            if (sPass != null)
            {
                double result = 0;
                return (double.TryParse(sPass, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.CurrentInfo, out result));
            }
            else
            {
                return false;
            }
        }

        public static string formatFPB(this string caption)
        {
            return caption.Replace("Loss, ", "Loss\n");
        }
        public static string formatPIPCombined(this string caption)
        {
            return caption.Replace(" +", "\n+");
        }
        //rules taken from Flex UDQuoteLibrary DiscountServices.as
        public static string FormatDiscountName(this string input)
        {
            string result;
            int end = input.Length;
            if (input.IndexOf("Discount") > 0)
                end = input.IndexOf("Discount") - 1;
            result = input.Substring(0, end);
            result = result.Replace("iMingle Network", "Network");
            return result;
        }

        //rules taken from Flex UDQuoteLibrary DiscountServices.as
        public static string FormatDiscountDescription(this string input, WebSessionDRC websession)
        {
            string result;

            result = input.Replace("{iMingleDescription}", websession.Quote.getPolicyInfo().getMarketBrand().getiMingleDescription(websession.AddInfo.Referral));
            result = result.Replace("{CompanyName}", websession.Quote.getPolicyInfo().getMarketBrand().GetCompanyName());
            result = result.Replace("{Group}", GetGroupName(websession.Quote.getPolicyInfo().getMarketBrand()));
            result = result.Replace("&lt;sup&gt;&amp;trade;&lt;/sup&gt;", "™");
            result = result.Replace("&lt;sup&gt;&amp;reg;&lt;/sup&gt;", "™");
            result = result.Replace("<sup>&trade;</sup>", "™");
            result = result.Replace("<sup>&reg;</sup>", "™");
            result = result.Replace("&lt;span style='font-weight:bold;'&gt;", "");
            result = result.Replace("<span style='font-weight:bold;'>", "");
            result = result.Replace("&lt;/span&gt;", "");
            result = result.Replace("</span>", "");
            result = result.Replace("&amp;nbsp;", " ");
            result = result.Replace("&nbsp;", " ");
            result = result.Replace("[Kemper Direct/Teachers/Partnership Direct]", GetGroupName(websession.Quote.getPolicyInfo().getMarketBrand()));
            if (result == "iMingle Network<sup>&trade;</sup> Discount" || result == "iMingle Network<sup>&reg;</sup> Discount" || result == "iMingle Network™")
            {
                int marketBrand = websession.Quote.getPolicyInfo().getMarketBrand();
                if (marketBrand == 1 || marketBrand == 0)
                    result = "Network Discount";
                else if (marketBrand == 3)
                    result = "Teachers' Network™ Discount";
            }
            return result;
        }
        public static string getiMingleDescription(this int marketBrand, Referral referral)
        {
            string discountText = "";
            if (marketBrand == 1 || marketBrand == 0)
            {
                discountText = "Kemper Direct";
            }
            else if (marketBrand == 2)
            {
                discountText = "iMingle Network Discount";
            }
            else if (marketBrand == 3)
            {
                discountText = "Teachers' Network™ Discount";
            }
            if (referral != null)
            {
                string firstName = StringExtensions.CapitalizeFirstLetter(referral.FirstName);
                string lastName = StringExtensions.CapitalizeFirstLetter(referral.LastName);
                if (marketBrand == 2)
                    return "You have already been connected to " + firstName + " " + lastName + "'s policy.  Once you buy and agree to the Safety Pledge, you get a 10% discount and " + firstName + "'s 10% discount is locked in.  High Five’s all around!";
                else
                    return "When you buy your policy, " + firstName + " " + lastName + "’s discount will be locked in as well. We'll give you a tool to help more friends join the savings too!";

            }
            if (marketBrand == 2)
                return "Bring a friend to iMingle (you’ll have 40 days), and if they purchase a policy and take the Safety Pledge … Bam!  You get a 10% discount and they get a 10% discount.  High-fives all around!";
            else
                return "You will have 40 days from the day you buy your policy to establish your " + discountText + " by bringing another policyholder to " + GetCompanyName(marketBrand) + ". Don’t worry. It’s easy, and we’ll help you.";
        }
        public static string GetCompanyName(this int marketBrand)
        {
            switch (marketBrand)
            {
                case 2:
                    return "iMingle";
                case 3:
                    return "teachers.com®";
                default:
                    return "Kemper Direct";
            }
        }
        public static string GetGroupName(this int marketBrand)
        {
            switch (marketBrand)
            {
                case 3:
                    return "Teachers";
                default:
                    return "[Kemper Direct/Teachers/Partnership Direct]";
            }
        }

        public static bool DiscountApply(this string discountID, WebSessionDRC websession)
        {
            string state = websession.AddInfo.RiskState;
            DateTime systemDate = DateTime.Now;
            DateTime.TryParse(websession.AddInfo.SystemDate, out systemDate);

            switch (discountID)
            {
                case "MulticarDiscount":
                    if (websession.Quote.getVehicles().count() > 1)
                        return true;
                    else
                        return false;
                case "AirBagDiscount":
                    return false;
                case "PassiveRestraintDiscount":
                    if (state == "NV")
                    {
                        for (int i = 0; i < websession.Quote.getVehicles().count(); i++)
                            if (websession.Quote.getVehicles().item(i).getAirBagTest() > 0)
                                return true;
                    }
                    if (state == "PA")
                    {
                        for (int j = 0; j < websession.Quote.getVehicles().count(); j++)
                        {
                            if (websession.Quote.getVehicles().item(j).getAirBagTest() > 0)
                                return true;
                            if (websession.Quote.getVehicles().item(j).getPassiveRestraint() > 0)
                                return true;
                        }
                    }
                    return false;

                case "ESsignatureDiscount":
                    return false;
                case "WebDiscount":
                    return false;
                case "MingleMateDiscount":
                    if ((websession.AddInfo.Referral != null) ||
                        (websession.Quote.getPolicyInfo().getMingleMateDis() == 1))
                        return true;
                    break;
                case "MultPolicyDiscount":
                    //if (((websession.AddInfo.ClickThruPartnerInfo.RenterAndAuto != null) &&
                    //     (websession.AddInfo.ClickThruPartnerInfo.RenterAndAuto == "YES"))
                    //      || 
                    //    ((websession.AddInfo.HOPolicy.Policy != null) && 
                    //     (websession.AddInfo.HOPolicy.Policy != "")))
                    //    return true;		
                    //else
                    return false;
                case "PaperlessDiscount":
                    if (websession.Quote.getPolicyInfo().getPaperlessDis() == 1)
                        return true;
                    else
                        return false;
                case "ADisabledvcDiscount":
                    if ("IL,PA,MN,IA,NJ".Contains(state))
                        for (int j = 0; j < websession.Quote.getVehicles().count(); j++)
                            if (websession.Quote.getVehicles().item(j).getDisablingDevice() > 0)
                                return true;
                            else
                                if (state == "NJ" && (websession.Quote.getVehicles().item(j).getWindowEtchingTest() > 0))
                                    return true;
                    return false;
                case "WelcomeBackDiscount":
                    if (websession.Quote.getPolicyInfo().getWelcomeBackDis() > 0)
                        return true;
                    else
                        return false;
                case "ComeBackAndSaveDiscount":
                    if (websession.Quote.getPolicyInfo().getComeBackDis() > 0)
                        return true;
                    else
                        return false;
                case "MarriedDiscount":
                    if (state != "MI")
                    {
                        if (websession.Quote.getDrivers().count() > 0)
                        {
                            if ((websession.Quote.getDrivers().item(0).getDrivMarriedSingle() == 1) ||
                                (websession.Quote.getDrivers().item(0).getDrivMarriedSingle() == 10))
                            {
                                websession.Quote.getPolicyInfo().setMarriedDis(1);
                                return true;
                            }
                            //ysang 6715 Widowed? add IA
                            //dmetz 11-04-2010 #6715 - Widowed? add Vibe 2B states
                            //dmetz 11-22-2010 #6845 - Vibe 2B - OH, AZ, TX
                            //PK SSR 7513 -- IN & MO -- Added MO
                            //jrenz SSR 7053, SSR 7548 3/23/2011 NV
                            //dmetz 07-12-2011 SSR6871 - CT, KS, TN
                            //jrenz 11/18/2011 SSR06872 - MD
                            //dmetz 04-19-2012 SSR6873 - ID,LA,UT
                            if ("IN,MN,OR,IL,CO,IA,SC,NJ,WI,VA,MO,NV,OH,AZ,TX,CT,KS,TN,MD,ID,LA,UT".Contains(state))
                            {
                                if (websession.Quote.getDrivers().item(0).getDrivMarriedSingle() == 6)
                                {
                                    websession.Quote.getPolicyInfo().setMarriedDis(1);
                                    return true;
                                }
                            }
                        }
                    }
                    websession.Quote.getPolicyInfo().setMarriedDis(0);
                    return false;
                case "MatureDriverDiscount":

                    if (state == "MI")
                    {
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if (websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(65) < systemDate)
                                return true;
                    }
                    if ("IN,MN,NV,IL,CO,VA,NJ,MD".Contains(state))
                    {
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if ((websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(65) < systemDate) &&
                                (websession.Quote.getDrivers().item(i).getDrivTrainingDis() > 0)) //AddInfo.Drivers.Driver[i].AiDrivCourse
                                return true;
                    }
                    if ("TN,ID".Contains(state))
                    {
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if ((websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(55) < systemDate) &&
                                (websession.Quote.getDrivers().item(i).getDrivTrainingDis() > 0) && //AddInfo.Drivers.Driver[i].AiDrivCourse
                                (!chargeableAccidentsOrViolations(websession.Quote, i)))
                                return true;
                    }
                    if (state == "OR" || state == "SC")
                    {
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if ((websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(25) < systemDate) &&
                            (!chargeableAccidentsOrViolations(websession.Quote, i)))
                                return true;
                    }
                    if (state == "OH" || state == "CT")
                    {
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if (websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(60) < systemDate)
                                return true;
                    }

                    return false;
                case "RetroLoyaltyDiscount":
                    if ((websession.Quote.getPolicyInfo().getRetroLoyaltyLevel() == 2) ||
                        (websession.Quote.getPolicyInfo().getRetroLoyaltyLevel() == 3) ||
                        (websession.Quote.getPolicyInfo().getRetroLoyaltyLevel() == 4))
                        return true;
                    else
                        return false;
                case "SafeSoundDiscount":
                    websession.Quote.getPolicyInfo().setSafeSoundDis(0);
                    if ("MI,ID,LA".Contains(state))
                    {
                        if (websession.Quote.getDrivers().count() > 1)
                            if (websession.Quote.getPolicyInfo().getNoOfDaysLapsed() != 2) //no lapse=2
                                for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                                    if (websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(19) < systemDate) //a driver over 19
                                    {
                                        for (int j = 0; j < websession.Quote.getAccidents().count(); j++)
                                            if (websession.Quote.getAccidents().item(i).getSdipAccComp() > 0)
                                                return false;
                                        websession.Quote.getPolicyInfo().setSafeSoundDis(1);
                                        return true;
                                    }
                    }
                    else if (state == "UT")
                    {
                        if (websession.Quote.getDrivers().count() > 1)
                            if (websession.Quote.getPolicyInfo().getNoOfDaysLapsed() != 8) //innocent no prior=8
                                for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                                    if (websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(19) < systemDate) //a driver over 19
                                    {
                                        for (int j = 0; j < websession.Quote.getAccidents().count(); j++)
                                            if (websession.Quote.getAccidents().item(i).getSdipAccComp() > 0)
                                                return false;
                                        websession.Quote.getPolicyInfo().setSafeSoundDis(1);
                                        return true;
                                    }
                    }
                    else
                    {
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if (websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(19) < systemDate) //a driver over 19
                            {
                                for (int j = 0; j < websession.Quote.getAccidents().count(); j++)
                                    if (websession.Quote.getAccidents().item(i).getSdipAccComp() > 0)
                                        return false;
                                if (websession.Quote.getPolicyInfo().getNoOfDaysLapsed() == 7)
                                {
                                    websession.Quote.getPolicyInfo().setSafeSoundDis(1);
                                    return true;
                                }
                                else
                                    return false;
                            }
                    }
                    return false;
                case "PreferPayerDiscount":
                    if (websession.Quote.getCoverages().count() > 0)
                        if (websession.Quote.getCoverages().item(0).getSixMonthPremiums().getSmTPrefPayPrem() > 0)
                            return true;
                    return false;
                case "GroupDiscount":
                    if (websession.Quote.getDrivers().count() > 0)
                        if (websession.Quote.getCoverages().count() > 0)
                            if (websession.Quote.getCoverages().item(0).getSixMonthPremiums().getSmTGroupdiscPrem() > 0)
                                return true;
                    return false;
                case "DdcDiscount":
                    if (state == "KS")
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if (websession.Quote.getDrivers().item(i).getDrivTrainingDis() > 0) //AddInfo.Drivers.Driver[i].AiDrivCourse
                                return true;
                    if (state == "SC")
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if (websession.Quote.getDrivers().item(i).getDrivTrainingDis() > 0) //AddInfo.Drivers.Driver[i].AiDrivCourse
                                if (websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(25) < systemDate)
                                    if (chargeableAccidentsOrViolations(websession.Quote, i))
                                        return true;
                    if (state == "UT")
                        for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                            if (websession.Quote.getDrivers().item(i).getDrivTrainingDis() > 0) //AddInfo.Drivers.Driver[i].AiDrivCourse
                                if (websession.Quote.getDrivers().item(i).getBirthDateOfDriv().AddYears(25) < systemDate)
                                    return true;
                    return false;
                case "VehicleRecoveryDiscount":
                    for (int i = 0; i < websession.Quote.getVehicles().count(); i++)
                        if (websession.Quote.getVehicles().item(i).getVehicleRecoveryTest() > 0)
                            return true;
                    return false;
                case "ContinuousDiscount":
                    if ("IA,SC,NJ,WI,VA".Contains(state))
                        if (websession.Quote.getPolicyInfo().getNoOfDaysLapsed() > 1)
                            return true;
                    return false;
                case "GoodDriverDiscount":
                    for (int i = 0; i < websession.Quote.getDrivers().count(); i++)
                        if (websession.Quote.getDrivers().item(i).getGoodDriverDis() > 0)
                            return true;
                    return false;
                case "HomeownershipDiscount":
                    if (websession.Quote.getCustomer().getRentOwnTest() == 1)
                        return true;
                    else
                        return false;
                case "GoodStudentDiscount":
                    if (websession.Quote.getCoverages().count() > 0)
                        if (websession.Quote.getCoverages().item(0).getSixMonthPremiums().getSmTGoodStudentPrem() > 0)
                            return true;
                    return false;
                default:
                    return false;
            }
            return false;
        }

        public static bool chargeableAccidentsOrViolations(this AutoQuote.Autoquote quote, int driver)
        {
            for (int i = 0; i < quote.getAccidents().count(); i++)
            {
                if (quote.getAccidents().item(i).getAccCompChargedTest() == 1)
                    if (quote.getAccidents().item(i).getDrivNoOfAccComp() == driver)
                        return true;
            }
            for (int i = 0; i < quote.getViolations().count(); i++)
            {
                if (quote.getViolations().item(i).getViolChargedTest() == 1)
                    if (quote.getViolations().item(i).getDrivNoOfViol() == driver)
                        return true;
            }
            return false;
        }

        public static bool IsVibeState(this string state)
        {
            return ConfigurationManager.AppSettings["VibeState"].Contains(state);
        }
        public static bool IsWebModelState(this string state)
        {
            return !ConfigurationManager.AppSettings["WebModelState"].Contains(state);
        }
        public static bool IsNewBindState(this string state)
        {
            return ConfigurationManager.AppSettings["NewBindState"].Contains(state);
        }
    }
}
