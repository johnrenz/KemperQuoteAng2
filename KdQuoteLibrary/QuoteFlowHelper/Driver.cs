using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KdQuoteLibrary.QuoteFlowHelper
{
    [Serializable]
    public class Driver
    {
        public int AiAge { get; set; }
        public string AiDrivCourse { get; set; }
        public string AiAccOrVio { get; set; }
        public string AiAgeLicensed { get; set; }
        public string AiLicSusp3Yr { get; set; }
        public string AiLicReinstated { get; set; }
        public DateTime AiLicReinstDate3Yr { get; set; }
        public string AiConvicted { get; set; }
        public string AiFullTimeStudent { get; set; }
        public string AiGPA { get; set; }
        public string AiAwayAtSchool { get; set; }
        public bool HasAccidentsViolations { get; set; }
    }
}
