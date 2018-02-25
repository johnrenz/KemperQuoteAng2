using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KdQuoteLibrary.QuoteFlowHelper
{
    public enum IncidentType
    {
        Accident,
        Loss,
        Violation
    }
    public class Incident
    {
        public IncidentType Type { get; set; }
        public DateTime Date { get; set; }
        public string ID { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }

        public static IncidentType ToIncidentType(string type)
        {
            switch (type)
            {
                case "accident":
                    return IncidentType.Accident;
                case "loss":
                    return IncidentType.Loss;
                default:
                    return IncidentType.Violation;
            }
        }
    }
}
