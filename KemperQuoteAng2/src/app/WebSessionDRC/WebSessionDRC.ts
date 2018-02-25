////import { Discounts } from './Discounts';
import { Quote } from './Quote';
import { AddInfo } from './AddInfo';
import { PolicyCoverages, VehicleCoverages } from './SessionCoverage';
//import { PolicyPremiums } from './PolicyPremiums';
//import { VehiclePremiums } from './VehiclePremiums';
import { CoverageError } from './CoverageError';
import { PolicyCoverageErrors } from './PolicyCoverageErrors';
import { VehicleCoverageErrors } from './VehicleCoverageErrors';
import { PayPlans } from './PayPlans';
import { CoveragePageDiscounts } from './CoveragePageDiscounts';

export class WebSessionDRC {
    public Guid: string;
    public TotalPremium: string;
    public Quote: Quote;
    public AddInfo: AddInfo;
    public PolicyCoverages: PolicyCoverages;
    public VehicleCoverages: VehicleCoverages;
    public PayPlans: PayPlans;
    public CoveragePageDiscounts: CoveragePageDiscounts;
    public TotalDiscountSavings: string;
    public TotalDiscountSavingsWithoutPreferredPayer: string;
    //public PolicyPremiums: PolicyPremiums;
    //public VehiclePremiums: VehiclePremiums;
    public PolicyCoverageErrors: PolicyCoverageErrors;
    public VehicleCoverageErrors: VehicleCoverageErrors;

    constructor() {
    }
} 