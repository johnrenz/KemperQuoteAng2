using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using AutoQuote;

namespace AutoQuoteLibrary.AutoQuoteHelper
{
    [Serializable]
    public class WebSession
    {
        public AddInfo AddInfo;
        public Guid Guid;
        public Autoquote Quote;
        public List<Discount> Discounts;
        public decimal TotalDiscountSavingsWithoutPreferredPayer;
        public decimal TotalDiscountSavings;
        public decimal SelectedPreferredPayerDiscount;

        public List<Coverage> PolicyCoverages;
        public List<CoverageError> PolicyCoverageErrors;
        public List<VehicleCoverage> VehicleCoverages;
        public List<CoverageError> VehicleCoverageErrors;
        public List<EnhancedCoverage> EnhancedCoverages;
        public List<Discount> CoveragePageDiscounts;
        public decimal TotalPremium;
        public List<PayPlan> PayPlans;
        public PayPlan SelectedPayPlan;
        public decimal AnnualPayPlanDiscountSavings;
        public List<InstallmentFee> InstallmentFees;
        public bool HasInstallmentFees;
        public bool IsVibeState;
        public bool IsWebModelState;
        public List<Question> Questions;
        public List<Incident> IncidentSelections { get; set; }
        public bool HasAccidentsViolations;
        public List<Option> RentersPropertySelections { get; set; }
        public List<Option> RentersLiabilitySelections { get; set; }
        public List<Option> RentersDeductibleSelections { get; set; }
        public List<RentersRateScenario> RentersRates { get; set; }
        public XElement xRentersRates { get; set; }

        public WebSession()
        {
        }

        public XElement SerializedQuote { get; set; }

        public int FindPolicyCoverageIndex(string covCode)
        {
            for (int i = 0; i < PolicyCoverages.Count; i++)
            {
                Coverage cov = PolicyCoverages[i];
                if (covCode == cov.CovCode)
                    return i;
            }
            return -1;
        }
        public int FindVehicleIndex(string vehIndex)
        {
            for (int i = 0; i < VehicleCoverages.Count; i++)
            {
                if (VehicleCoverages[i].VehicleNumber == vehIndex)
                    return i;
            }
            return -1;
        }
        public int FindVehicleCoverageIndex(string covCode, int vehIndex)
        {
            for (int i = 0; i < VehicleCoverages[vehIndex].Coverages.Count; i++)
            {
                Coverage cov = VehicleCoverages[vehIndex].Coverages[i];
                if (covCode == cov.CovCode)
                    return i;
            }
            return -1;
        }
        public void AddErrorMessage(string func,string page,string module,string message)
        {
            if (AddInfo == null)
                AddInfo = new AddInfo();
            if (AddInfo.ErrorMessages == null)
                AddInfo.ErrorMessages = new List<ErrorMessage>();
            AddInfo.ErrorMessages.Add(new ErrorMessage() { Function = func, Page = page, Module = module, Error = message });
                    
        }
        public bool HasErrors()
        {
            if (AddInfo != null)
                if (AddInfo.ErrorMessages != null)
                    if (AddInfo.ErrorMessages.Count > 0)
                        return true;
            return false;
        }
    }
}
