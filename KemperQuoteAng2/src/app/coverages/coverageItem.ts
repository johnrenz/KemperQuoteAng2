import { Limit } from './Limit';
export class coverageItem {
    CovId: string;
    CovCode: string;
    Name: string;
    Desc: string;
    Caption: string;
    Premium: string;
    Limits: Limit[];
    SelectedLimit: Limit;
    CovInputType: string;
}