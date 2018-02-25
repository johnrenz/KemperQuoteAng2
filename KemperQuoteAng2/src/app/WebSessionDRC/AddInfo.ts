import { ErrorMessage } from './ErrorMessage';
export class AddInfo {
    public AiCurrInsLapse: string;
    public AiNoCoverageReason: string;
    public SystemDate: string;
    public ErrorMessages: ErrorMessage[];

    public ApplicantComplete: string;
    public DriversComplete: string;
    public VehiclesComplete: string;
    public PolicyInfoComplete: string;
}