using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using UDILibrary.UDIExtensions.Enumerator;
using System.ComponentModel;

namespace AutoQuoteLibrary.AutoQuoteHelper
{
    public enum Brand
    {
        [Description("Kemper Direct")]
        KD = 0,
        [Description("Kemper Select")]
        KS = 1,
        [Description("iMingle")]
        iMingle = 2,
        [Description("Teachers")]
        Teachers = 3
    }
}
