using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoQuoteLibrary.AbstractServices
{
    public interface ILoggingServices
    {
        void logError(string error, string app, string module, string function);
    }
}
