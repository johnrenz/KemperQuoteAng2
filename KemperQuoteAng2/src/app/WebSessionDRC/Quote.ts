import { Customer } from './Customer';
import { Drivers } from './Drivers';
import { Vehicles } from './Vehicles';
import { PolicyInfo } from './PolicyInfo';
import { QuoteInfo } from './QuoteInfo';
import { Coverages } from './Coverages';
import { AddInfo } from './AddInfo';
export class Quote {
    public Customer: Customer;
    public Drivers: Drivers;
    public Vehicles: Vehicles;
    public PolicyInfo: PolicyInfo;
    public QuoteInfo: QuoteInfo;
    public Coverages: Coverages; 
}