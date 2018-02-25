using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace AutoQuoteLibrary.AutoQuoteHelper
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
                throw new Exception("  { KdQuoteFlow.Utilities.CheckSum - " + ex.Message.ToString() + " } ");
            }
        }

        public static bool IsVibeState(string state)
        {
            return ConfigurationManager.AppSettings["VibeState"].Contains(state);
        }
        public static bool IsWebModelState(string state)
        {
            return !ConfigurationManager.AppSettings["WebModelState"].Contains(state);
        }
        public static bool IsNewBindState(string state)
        {
            return ConfigurationManager.AppSettings["NewBindState"].Contains(state);
        }
        
    }
}
