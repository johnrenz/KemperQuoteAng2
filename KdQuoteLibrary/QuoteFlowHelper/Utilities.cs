using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;

namespace KdQuoteLibrary.QuoteFlowHelper
{
    public static class Utilities
    {
        public static int CalculateAge(DateTime birthDate, DateTime now)
        {
            int age = now.Year - birthDate.Year;
            if (now.Month < birthDate.Month || (now.Month == birthDate.Month && now.Day < birthDate.Day)) age--;
            return age;
        }

        public static string CheckSum(string sQuotePart)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(sQuotePart.Substring(1, 1));
                sb.Append(sQuotePart.Substring(4, 1));
                int iTemp = 0;
                int.TryParse(sQuotePart.Substring(3, 1), out iTemp);
                iTemp = iTemp * 3;
                sb.Append(iTemp.ToString());
                sb.Append(sQuotePart.Substring(sQuotePart.Length - 1, 1));
                sb.Append(sQuotePart.Substring(2, 1));
                sb.Append(sQuotePart.Substring(5, 1));
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("  { KdQuoteLibrary.Utilities.CheckSum - " + ex.Message.ToString() + " } ");
            }
        }
        //rules taken from Flex UDQuoteLibrary DiscountServices.as
        public static string FormatDiscountDescription(string input, int marketBrand, Referral referral)
        {
            if (input == null)
                return null;

            string result;

            result = input.Replace("{iMingleDescription}", getiMingleDescription(marketBrand, referral));
            result = result.Replace("{CompanyName}", GetCompanyName(marketBrand));
            result = result.Replace("{Group}", GetGroupName(marketBrand));
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
            result = result.Replace("[Kemper Direct/Teachers/Partnership Direct]", GetGroupName(marketBrand));
            if (result == "iMingle Network<sup>&trade;</sup> Discount" || result == "iMingle Network<sup>&reg;</sup> Discount" || result == "iMingle Network™")
            {
                if (marketBrand == 1 || marketBrand == 0)
                    result = "Network Discount";
                else if (marketBrand == 3)
                    result = "Teachers' Network™ Discount";
            }
            return result;
        }
        public static string GetDiscountID(string name)
        {
            if (name.Contains("Preferred"))
                return "PreferredPayerDiscount";
            if (name.Contains("Multi-Car"))
                return "MultiCarDiscount";
            if (name.Contains("Homeownership"))
                return "HomeownershipDiscount";
            if (name.Contains("Network"))
            {
                return "NetworkDiscount";
            }
            if (name.Contains("Anti-Theft"))
                return "AntiTheftDiscount";
            if (name.Contains("Free-A-Tree"))
                return "FreeATree";
            if (name.Contains("Multi-Policy"))
                return "MultiPolicy";
            if (name.Contains("Happily Married"))
                return "HappilyMarried";
            if (name.Contains("Safe"))
                return "SafeAndSound";
            if (name.Contains("Passive Restraint"))
                return "PassiveRestraint";
            if (name.Contains("Focused Driver"))
                return "FocusedDriver";
            if (name.Contains("Esignature"))
                return "EsignatureDiscount";
            if (name.Contains("Retro Loyalty"))
                return "RetroLoyaltyDiscount";
            return name;
        }
        public static List<BundledCoverage> GetBundles(string name)
        {
            switch (name)
            {
                case "Protect Your Wallet":
                    return new List<BundledCoverage> {
                                new BundledCoverage() { 
                                    header = "Accident Forgiveness",
                                    description = "We won’t raise your rates just because you have an accident – even if it’s your fault." },
                                new BundledCoverage() { 
                                    header = "Renewal Assurance",
                                    description = "Don’t worry about getting dropped by your insurance company. As long as you continue to drive safely (for example, no DUIs) and meet a few other easy requirements, we won’t “break up” with you. That’s peace of mind you deserve." },
                                new BundledCoverage() { 
                                    header = "Disappearing Deductible",
                                    description = "We’ll take $100 off your Collision deductible right now.  Then, we’ll keep lowering it by $100 each year you’re accident free. (Note: Pennsylvania law does not allow this deductible to be less than $100.)" }
                            };
                case "Protect Your Sanity" :
                    return new List<BundledCoverage>  {
                                new BundledCoverage() {
                                    header = "Roadside Assistance",
                                    description = "Getting stuck on the side of the road isn’t any fun. We’ll send someone to change your tire, bring you gas, jumpstart your vehicle, help if you’re locked out, or give you a tow."
                                },
                                new BundledCoverage() {
                                    header = "Rental Car Reimbursement",
                                    description = "If your car is out of commission due to a covered loss, get back on the road with a rental car. We’ll reimburse you for up to $30 per day with a $600 maximum benefit."
                                },
                                new BundledCoverage() {
                                    header = "Trip Interruption",
                                    description = "When you’re on a road trip, an accident or mechanical breakdown can really slow you down. If you’re stuck more than 100 miles from home, we’ll provide up to $600 to use for things like transportation and lodging."
                                }
                            };
                case "Protect Your Family" :
                     return new List<BundledCoverage> {
                                new BundledCoverage() {
                                    header = "Enhanced Car Seat Replacement",
                                    description = "We know that a car seat’s safety can be compromised after an accident. That’s why we’ll replace your child car seats after a crash regardless of damage and without requiring you to pay a deductible."
                                },
                                new BundledCoverage() {
                                    header = "Pet Protection",
                                    description = "Your pets mean a lot to you. That’s why we’ll cover up to $2,000 for injury or death to your furry family members due to an accident. Dogs and cats apply."
                                },
                                new BundledCoverage() {
                                    header = "Dependent Protection",
                                    description = "Taking care of your children is your most important duty. That’s why we’ll provide up to $1,000 in daycare costs if an accident leaves you unable to care for your child. We’ll also provide $10,000 to your dependants if you die due to an accident."
                                }
                            };
                default: return new List<BundledCoverage>();                   
            }
        }
        public static string getiMingleDescription(int marketBrand, Referral referral)
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

        public static string GetCompanyName(int marketBrand)
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
        public static string GetGroupName(int marketBrand)
        {
            switch (marketBrand)
            {
                case 3:
                    return "Teachers";
                default:
                    return "[Kemper Direct/Teachers/Partnership Direct]";
            }
        }
        public static bool IsVibe2bState(string state)
        {
            return "IN,IA,MO,MN,NV,WI,AZ,OH,TX,PA,NJ,OR,CT,KS,TN,MI,CO,VA,MD,IL,ID,LA,UT".Contains(state);
        }
        
    }
}
