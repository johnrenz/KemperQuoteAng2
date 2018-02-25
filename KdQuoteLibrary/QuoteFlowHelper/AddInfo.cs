using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KdQuoteLibrary.QuoteFlowHelper
{
    [Serializable]
    public class AddInfo
    {
        public string Guid { get; set; }
        public string RiskState { get; set; }
        public string CurrentPage { get; set; }
        public string Application { get; set; }
        public string KemperCorpUrl { get; set; }
        public string Redirect { get; set; }
        public string SystemDate { get; set; }
        public string CSRQueue { get; set; }
        public string ReturnDiscount { get; set; }
        public ClickThruPartnerInfo ClickThruPartnerInfo { get; set; }
        public DNQ DNQ { get; set; }
        public string UseDefaultPayPlan { get; set; }
        public string HomeOfficeAccountName { get; set; }
        public string AccountPremierFlag { get; set; }
        public string PayRollDeductAcct { get; set; }
        public string SplitZip { get; set; }
        //public Discounts Discounts { get; set; }
        public List<Vehicle> Vehicles { get; set; }
        public List<Driver> Drivers { get; set; }
        public Referral Referral { get; set; }
        public string CurrentlyInsured { get; set; }
        public string AiCurrInsLapse { get; set; }
        public string AiNoCoverageReason { get; set; }
        public string paperlessdiscount { get; set; }
        public string multipolicydiscount { get; set; }
        public decimal multipolicydiscountNumeric { get; set; }
        public string iminglediscount { get; set; }
        //previous address
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressCity { get; set; }
        public string AddressZip { get; set; }
        public string AddressState { get; set; }
        public string CredRptID { get; set; }
        public List<ErrorMessage> ErrorMessages { get; set; }
        public string OwnRentOther { get; set; } //1=homeowner, 0=Renter
        public HOIRenterInfo HOIRenterInfo { get; set; }
        public HOIRenterProvide HOIRenterProvide { get; set; }
        public bool DegreedProf { get; set; }
        public bool AlumniCollege { get; set; }
        public string GroupClass { get; set; }
        public bool ApplicantComplete { get; set; }
        public bool VehiclesComplete { get; set; }
        public bool DriversComplete { get; set; }
        public bool PolicyInfoComplete { get; set; }

    }
}
