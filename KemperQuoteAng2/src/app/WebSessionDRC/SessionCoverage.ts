import { Limit } from './Limit';
export class Limits {
    Limit: Limit[];
}
export class PolicyCoverages {
    Coverage: Coverage[];
}
export class VehicleCoverages {
    VehicleCoverage: VehicleCoverage[];
}
export class VehicleCoverage {
    Coverages: Coverages;
    VehicleNumber: string;
    Year: string;
    Make: string;
    Model: string;
    StateFeeOption1: string;
}
export class Coverages {
    Coverage: Coverage[];
}
export class Coverage {
    CovId: string;
    CovCode: string;
    Name: string;
    Caption: string;
    Premium: string;
    CovInputType: string;
    Limits: Limit[];
    SelectedLimit: Limit;
    FAQText: string;
    SuppressRendering: string;
}