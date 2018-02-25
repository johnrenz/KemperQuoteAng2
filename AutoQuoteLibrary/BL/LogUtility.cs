using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using System.Configuration;

namespace AutoQuoteLibrary.BL
{
    public static class LogUtility
    {
        public static void LogError(string error, string app, string module, string func)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(ConfigurationManager.AppSettings["ErrorfilePath"] + "\\ErrorLog.txt", true);
                sw.WriteLine("-------------------------------");
                sw.WriteLine(DateTime.Now.ToString() + " Error in " + app + "." + module + "." + func + "-");
                sw.WriteLine(error);
                sw.WriteLine("-------------------------------");
            }
            catch (Exception  ex)
            {
                
            }
            finally
            {
                sw.Close();
            }
        }
    }
}