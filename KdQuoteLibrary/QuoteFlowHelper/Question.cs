﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KdQuoteLibrary.QuoteFlowHelper
{
    public class Question
    {
        public string ID { get; set; }
        public List<Option> Options { get; set; }
    }
}