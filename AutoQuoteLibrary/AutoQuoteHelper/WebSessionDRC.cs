using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using AutoQuote;

namespace AutoQuoteLibrary.AutoQuoteHelper
{
    [Serializable]
    public class WebSessionDRC : WebSession
    {
        //public Autoquote Quote;

        public WebSessionDRC()
        {
        }
        public WebSessionDRC(WebSession websession)
        {
            this.AddInfo = websession.AddInfo;
            this.Guid = websession.Guid;
            this.Quote = websession.Quote;
            this.Discounts = websession.Discounts;
            this.TotalDiscountSavingsWithoutPreferredPayer = websession.TotalDiscountSavingsWithoutPreferredPayer;
            this.TotalDiscountSavings = websession.TotalDiscountSavings;
            this.SelectedPreferredPayerDiscount = websession.SelectedPreferredPayerDiscount;

            this.PolicyCoverages = websession.PolicyCoverages;
            this.PolicyCoverageErrors = websession.PolicyCoverageErrors;
            this.VehicleCoverages = websession.VehicleCoverages;
            this.VehicleCoverageErrors = websession.VehicleCoverageErrors;
            this.EnhancedCoverages = websession.EnhancedCoverages;
            this.CoveragePageDiscounts = websession.CoveragePageDiscounts;
            this.TotalPremium = websession.TotalPremium;
            this.PayPlans = websession.PayPlans;
            this.SelectedPayPlan = websession.SelectedPayPlan;
            this.AnnualPayPlanDiscountSavings = websession.AnnualPayPlanDiscountSavings;
            this.InstallmentFees = websession.InstallmentFees;
            this.HasInstallmentFees = websession.HasInstallmentFees;
            this.IsVibeState = websession.IsVibeState;
            this.IsWebModelState = websession.IsWebModelState;
            this.Questions = websession.Questions;
            this.IncidentSelections = websession.IncidentSelections;
            this.HasAccidentsViolations = websession.HasAccidentsViolations;
            this.RentersPropertySelections = websession.RentersPropertySelections;
            this.RentersLiabilitySelections = websession.RentersLiabilitySelections;

            this.RentersDeductibleSelections = websession.RentersDeductibleSelections;
            this.RentersRates = websession.RentersRates;
            this.xRentersRates = websession.xRentersRates;

        }
    }
}
