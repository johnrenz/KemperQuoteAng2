﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KdQuoteLibrary.QuoteFlowHelper
{
    [Serializable]
    public class Drivers
    {
        public Driver[] Driver { get; set; }
        public int IpDriversCt { get; set; }
    }
}