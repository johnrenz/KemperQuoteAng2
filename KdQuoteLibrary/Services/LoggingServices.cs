using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using UDILibrary.Exceptions;
using UDILibrary.Log;
using UDILibrary.UDIExtensions.XMLSerialization;

namespace KdQuoteLibrary.Services
{
    public class LoggingServices
    {
        private static LoggingServices _loggingServices;

        public static LoggingServices Instance
        {
            get
            {
                if (_loggingServices == null)
                    _loggingServices = new LoggingServices();
                return _loggingServices;
            }
        }

        public void logError(string message, string source, LogSeverity severity)
        {
            APPLOG.Log("KdQuoteLibrary", source, message, severity);
            
        }
    }
}
