using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KdQuoteLibrary.Services
{
    public class BaseServices
    {
        protected CacheServices CacheManager = CacheServices.Instance;
        protected LoggingServices Log = LoggingServices.Instance;
    }
}
