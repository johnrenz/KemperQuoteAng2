using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoQuoteLibrary.AutoQuoteHelper
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
        public static string FormatDiscountDescription(this string input, WebSession websession)
        {
            string result;

            result = input.Replace("{iMingleDescription}", getiMingleDescription(websession.Quote.getPolicyInfo().getMarketBrand(), websession.AddInfo.Referral));
            result = result.Replace("{CompanyName}", GetCompanyName(websession.Quote.getPolicyInfo().getMarketBrand()));
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

        private static string getiMingleDescription(int marketBrand, Referral referral)
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

        private static string GetCompanyName(int marketBrand)
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
        private static string GetGroupName(int marketBrand)
        {
            switch (marketBrand)
            {
                case 3:
                    return "Teachers";
                default:
                    return "[Kemper Direct/Teachers/Partnership Direct]";
            }
        }
    }
}
