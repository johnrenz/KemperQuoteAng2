import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import {AppComponent} from './app.component';
import {ApplicantComponent} from './applicant/applicant.component';
import {PolicyInfoComponent} from './policyInfo/policyInfo.component';
import { DriversComponent } from './drivers/drivers.component';
//import { VehiclesComponent } from './vehicles/vehicles.component';
import { VehiclesListComponent } from './vehicles/vehiclesList.component';
import {CoveragesComponent} from './coverages/coverages.component';
import {SummaryComponent} from './summary/summary.component';

export const router: Routes = [
    { path: '', redirectTo: 'applicant/4AAF7071-60FC-4246-B854-1141CFD043D5/60103/80001', pathMatch: 'full' },
    { path: ':zipcode/:ctid', redirectTo: 'applicant/new/:zipcode/:ctid', pathMatch: 'full' },
    { path: 'applicant/:guid/:zipcode/:ctid', component: ApplicantComponent },
    { path: 'policyInfo/:guid/:zipcode/:ctid', component: PolicyInfoComponent },
    { path: 'drivers/:guid/:zipcode/:ctid', component: DriversComponent },
    { path: 'vehicles/:guid/:zipcode/:ctid', component: VehiclesListComponent },
    { path: 'coverages/:guid/:zipcode/:ctid', component: CoveragesComponent },
    { path: 'summary/:guid/:zipcode/:ctid', component: SummaryComponent }
];

export const routes: ModuleWithProviders = RouterModule.forRoot(router);