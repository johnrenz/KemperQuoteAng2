using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UDILibrary.Log;
using AutoQuoteLibrary.BL;
using AutoQuoteLibrary.AbstractServices;

namespace AutoQuoteLibrary.Services
{
    public class LoggingServices : ILoggingServices
    {
        public void logError(string error, string app, string module, string function)
        {
            LogUtility.LogError(error, app, module, function);
        }
    }
}