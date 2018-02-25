import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { routes } from './app.router';
import { FormsModule }   from '@angular/forms';     
import { ReactiveFormsModule } from '@angular/forms';  
import { HttpModule } from '@angular/http';
import { HashLocationStrategy, LocationStrategy } from '@angular/common';
import {
    MatAutocompleteModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatCardModule,
    MatCheckboxModule,
    MatChipsModule,
    MatDatepickerModule,
    MatDialogModule,
    MatExpansionModule,
    MatGridListModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatMenuModule,
    MatNativeDateModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatRippleModule,
    MatSelectModule,
    MatSidenavModule,
    MatSliderModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatSortModule,
    MatTableModule,
    MatTabsModule,
    MatToolbarModule,
    MatTooltipModule,
    MatStepperModule
    

} from '@angular/material';
import { CdkTableModule } from '@angular/cdk/table';

import { AppComponent } from './app.component';
import { HeaderComponent } from './header.component';
import { FooterComponent } from './footer.component';
import { ApplicantComponent } from './applicant/applicant.component';
import { PolicyInfoComponent } from './policyInfo/policyInfo.component';
import { DriversComponent } from './drivers/drivers.component';
//import { VehiclesComponent } from './vehicles/vehicles.component';
import { VehiclesListComponent } from './vehicles/vehiclesList.component';
import { ManageVehicle } from './vehicles/ManageVehicle.component';
import { ManageDriver } from './drivers/ManageDriver.component';
import { ManageCoverages } from './coverages/ManageCoverages.component';
import { CoveragesComponent } from './coverages/coverages.component';
import { SummaryComponent } from './summary/summary.component';
import 'hammerjs';

@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    FooterComponent,
    ApplicantComponent,
    PolicyInfoComponent,
    DriversComponent,
    //VehiclesComponent,
    VehiclesListComponent,
    ManageVehicle,
    ManageDriver,
    CoveragesComponent,
    ManageCoverages,
    SummaryComponent
    ],
  exports: [
      CdkTableModule,
      MatAutocompleteModule,
      MatButtonModule,
      MatButtonToggleModule,
      MatCardModule,
      MatCheckboxModule,
      MatChipsModule,
      MatStepperModule,
      MatDatepickerModule,
      MatDialogModule,
      MatExpansionModule,
      MatGridListModule,
      MatIconModule,
      MatInputModule,
      MatListModule,
      MatMenuModule,
      MatNativeDateModule,
      MatPaginatorModule,
      MatProgressBarModule,
      MatProgressSpinnerModule,
      MatRadioModule,
      MatRippleModule,
      MatSelectModule,
      MatSidenavModule,
      MatSliderModule,
      MatSlideToggleModule,
      MatSnackBarModule,
      MatSortModule,
      MatTableModule,
      MatTabsModule,
      MatToolbarModule,
      MatTooltipModule
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    routes,
    ReactiveFormsModule,
    HttpModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatDialogModule,
    MatTooltipModule,
    MatRadioModule,
    MatExpansionModule,
    MatCardModule
  ],
  providers: [{ provide: LocationStrategy, useClass: HashLocationStrategy }],
  bootstrap: [AppComponent],
  entryComponents: [
      ManageVehicle,
      ManageCoverages,
      ManageDriver
    ]
})
export class AppModule { }
