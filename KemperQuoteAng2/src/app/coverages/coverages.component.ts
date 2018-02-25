import { Component, Input, OnChanges, OnInit, trigger, state, transition, style, animate } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormControl, FormGroup, FormArray, AbstractControl, Validators } from '@angular/forms';
import { HomeRentOptions } from '../applicant/HomeRentOptions';
import { HomeownerShipOptions } from '../applicant/HomeownerShipOptions';
import { HowDidYouHearOptions } from '../applicant/HowDidYouHearOptions';
import { WebSessionDRC } from '../WebSessionDRC/WebSessionDRC';
import { SessionService } from '../session.service';
import { Http } from '@angular/http';
import * as xml2js from 'xml2js';
import { HeaderComponent } from '../header.component';
import { coverageItem } from './coverageItem';
import { VehicleCoverages, VehicleCoverage, Coverage } from '../WebSessionDRC/SessionCoverage';
import { Limit } from './Limit';
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
    MatStepperModule,
    MatFormFieldModule
} from '@angular/material';

import { MatDialog, MatDialogRef } from '@angular/material';
import { DBOperation } from './enum';
import { CdkTableModule } from '@angular/cdk/table';
import { ManageCoverages } from './ManageCoverages.component';

@Component({
    selector: 'app-coverages',
    providers: [SessionService],
    templateUrl: './coverages.component.html',
    styleUrls: ['./coverages.component.css']
})
export class CoveragesComponent implements OnInit {

    constructor(private fb: FormBuilder,
        private sessionService: SessionService,
        private route: ActivatedRoute,
        private router: Router,
        private dialog: MatDialog) {

        this.createForm();
    }

    msg: string;
    dbops: DBOperation;
    modalTitle: string;
    modalBtnTitle: string;
    private sub: any;
    guid: string;
    zipcode: string;
    ctid: string;
    ratedSaveResult: string;
    ratedSaveSuccess: boolean;

    sessionObject: WebSessionDRC; //this is the data model!!

    nTotalPremium: number;
    nTotalDiscountSavings: number;
    nDiscountedPremium: number;
    TotalPremium: string;
    TotalDiscountSavings: string;
    DiscountedPremium: string;
    NumberOfInstallments: string;
    InstallmentType: string;
    InstallmentAmount: string;
    DownPaymentAmount: string;
    preferredPaymentAmount: string;
    nPreferredPaymentAmount: number;

    policyCoverages: Coverage[];
    //vehicleCoverages: VehicleCoverage[];
    vehicleCoverage: VehicleCoverage;
    vehicleNumber: number;

    private policyCoverageControl: FormArray;
    private vehicleControl: FormArray;
    private vehicleCoverageControl: FormArray;
    private limitControl: FormArray;
    private vehicleLimitControl: FormArray;
    private payPlanControl: FormArray;
    private discountControl: FormArray;

    selectedPayPlanValue: string;
    //get TotalDiscountSavings() { return this.coverageForm.get('TotalDiscountSavings'); }

    coverageForm: FormGroup;
    LoadComplete = false;
    formatter = new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD',
        minimumFractionDigits: 2,
        // the default value for minimumFractionDigits depends on the currency
        // and is usually already 2
    });

  ngOnInit() {
      this.sub = this.route.params.subscribe(params => {
          this.guid = params['guid'];
          this.zipcode = params['zipcode'];
          this.ctid = params['ctid'];
      });
      this.loadSession()
          .then(() => this.ratedSave())
          .then(() => {
              if (this.ratedSaveResult == "Success") {
                  this.ratedSaveSuccess = true;
                  this.loadCoveragesAndDiscounts()
                      .then(() => {
                          if (this.ratedSaveResult == "Success") {
                              console.log("ngOnInit after loadCoveragesAndDiscounts");
                              //console.log("sessionobject=" + JSON.stringify(this.sessionObject));
                              //console.log("coverage 1" + this.sessionObject.PolicyCoverage[0].Desc);

                              console.log("this.sessionObject.TotalPremium " + this.sessionObject.TotalPremium);
                              this.nTotalDiscountSavings = parseFloat(this.sessionObject.TotalDiscountSavings);
                              this.TotalDiscountSavings = this.formatter.format(this.nTotalDiscountSavings);
                              this.nTotalPremium = parseFloat(this.sessionObject.TotalPremium);
                              this.TotalPremium = this.formatter.format(this.nTotalPremium);
                              this.nDiscountedPremium = this.nTotalPremium - this.nTotalDiscountSavings;
                              this.DiscountedPremium = this.formatter.format(this.nDiscountedPremium);
                              this.patchForm();
                              console.log("coverages.ngOnInit LoadComplete set complete");
                              this.LoadComplete = true;
                          }
                      });
              }
              else {
                  this.msg = "We are not able to quote you at this time. Please call customer service. " + this.ratedSaveResult
                  this.ratedSaveSuccess = false;
                  this.LoadComplete = true;
              }
          });
  }

  createForm() {
      this.coverageForm = this.fb.group({
          policyCoverageArray: this.fb.array([]),
          vehicleCoverageArray: this.fb.array([]),
          payPlanArray: this.fb.array([]),
          discountArray: this.fb.array([])
          //TotalDiscountSavings: ['']
      });
      this.policyCoverageControl = <FormArray>this.coverageForm.controls['policyCoverageArray'];
      this.vehicleCoverageControl = <FormArray>this.coverageForm.controls['vehicleCoverageArray'];
      this.payPlanControl = <FormArray>this.coverageForm.controls['payPlanArray'];
      this.discountControl = <FormArray>this.coverageForm.controls['discountArray'];
  }

  patchForm() {
      this.patchPolicyCoverage();      
      this.patchVehicles();
      this.patchDiscounts();
      this.patchPayPlans();
      //this.coverageForm.controls['TotalDiscountSavings'].setValue(this.sessionObject.TotalDiscountSavings);
      //debug info:
      const vehicleControl1 = (<FormArray>this.vehicleCoverageControl).at(0).value.Coverages;
  }
  patchDiscounts() {
      console.log("discounts");
      this.sessionObject.CoveragePageDiscounts.Discount.forEach((discItem) => {
          console.log("discItem.Amount" + discItem.Amount);
          console.log("discItem.AmountNumeric" + discItem.AmountNumeric);
          console.log("discItem.Description" + discItem.Description);
          console.log("discItem.Name" + discItem.Name);
          console.log("discItem.ID" + discItem.ID);
          console.log("discItem.ExpandedDesc" + discItem.ExpandedDesc);
          console.log("discItem.ShortDescription" + discItem.ShortDescription);
          if (discItem.ID == "PreferredPayerDiscount") {
              this.nPreferredPaymentAmount = discItem.AmountNumeric;
              this.preferredPaymentAmount = discItem.Amount;
              console.log("patchPayPlanValues preferredPaymentAmount=" + this.preferredPaymentAmount)
          }
          this.discountControl.push(this.patchDiscountValue(discItem));
      });
  }

  patchDiscountValue(discItem): AbstractControl {
      return this.fb.group({
          ID: [discItem.ID],
          Description: [discItem.Description],
          ShortDescription: [discItem.ShortDescription],
          ExpandedDesc: [discItem.ExpandedDesc],
          Name: [discItem.Name],
          Applied: [discItem.Applied],
          Purchased: [discItem.Purchased],
          CanBeDeleted: [discItem.CanBeDeleted],
          Amount: [discItem.Amount],
          AmountNumeric: [discItem.AmountNumeric]
      });
  }

  patchPayPlans() {
      var p: number = 0;
      
      this.sessionObject.PayPlans.PayPlan.forEach((planItem) => {
          this.payPlanControl.push(this.patchPayPlanValues(planItem));
          p++;
      });
      this.getSelectedPayPlanValue(this.sessionObject.PayPlans.PayPlan[0].Value); 
  }
  patchPayPlanValues(planItem): AbstractControl {
      
      return this.fb.group({
          Name: [planItem.Name],
          ID: [planItem.ID],
          Description: [planItem.Description],
          Discount: [planItem.Discount],
          Installments: [planItem.Installments],
          Payments: [planItem.Payments],
          Yearly: [planItem.Yearly],
          Downpayment: [planItem.Downpayment],
          Installment: [planItem.Installment],
          Order: [planItem.Order],
          Value: [planItem.Value],
          InstallmentType: [planItem.InstallmentType],
          DownDivisor: [planItem.DownDivisor]
      });
  }
  patchVehicles() {
      var v: number = 0;
      this.sessionObject.VehicleCoverages.VehicleCoverage.forEach((vehItem) => { //if multiple vehs
      //var vehItem = this.sessionObject.VehicleCoverages.VehicleCoverage;
          //console.log("veh vehItem.Make=" + vehItem.Make);
          this.vehicleCoverageControl.push(this.patchVehicleValues(vehItem));
          this.patchVehicleCoverages(vehItem, v);
          v++;
      });
  }

  patchVehicleCoverages(vehItem, v) {
      //const vehicleControl = (<FormArray>this.vehicleCoverageControl).at(v).value.Coverages; //this was a major bug! must use .get('Coverages')
      const vehicleControl = (<FormArray>(<FormArray>this.vehicleCoverageControl).at(v).get('Coverages'));
      //console.log("patchVehicleCoverages make=" + (<FormArray>this.vehicleCoverageControl).at(v).value.Make);
      var c: number = 0;
      vehItem.Coverages.Coverage.forEach((covItem) => {
          vehicleControl.push(this.patchCoverageValues(covItem))
          if (covItem.CovInputType == "combo")
              this.patchVehicleLimit(covItem, v, c);
          c++;
      });
  }
  patchVehicleLimit(covItem, v, c) {
      const covControl = (<FormArray>(<FormArray>this.vehicleCoverageControl).at(v).get('Coverages'));
      const vehLimitControl = (<FormArray>covControl.at(c).get('Limits'));
      //this.limitControl = (<FormArray>(<FormArray>this.vehicleControl.controls).at(v).value.Coverages).at(c).value.Limits;
      covItem.Limits.Limit.forEach((limitItem) => {
          //console.log("veh " + v + " limitItem=" + limitItem);
          //console.log("veh " + v + " limitItem.Caption=" + limitItem.Caption);
          vehLimitControl.push(this.patchLimitValues(limitItem))
      });
  }
  private patchVehicleValues(vehItem): AbstractControl {
      return this.fb.group({
          Year: [vehItem.Year],
          Make: [vehItem.Make],
          Model: [vehItem.Model],
          VehicleNumber: [vehItem.VehicleNumber],
          StateFeeOption1: [vehItem.StateFeeOption1],
          Coverages: this.fb.array([])
      });
  }
  patchPolicyCoverage() {
      var k: number = 0;
      //this.policyCoverages.forEach((covItem) => {
      //console.log("this.sessionObject.PolicyCoverages = " + this.sessionObject.PolicyCoverages);
      //console.log("this.sessionObject.PolicyCoverages.length = " + this.sessionObject.PolicyCoverages.length);
      this.sessionObject.PolicyCoverages.Coverage.forEach((covItem) => {
          //console.log("covItem.Caption=" + covItem.Caption);
          this.policyCoverageControl.push(this.patchCoverageValues(covItem))
          if (covItem.CovInputType == "combo")
              this.patchLimit(covItem, k)
          k++;
      });
  }
  private patchCoverageValues(coverageItem): AbstractControl {
      console.log("coverageItem.Name:" + coverageItem.Name);
      console.log("coverageItem.HelpText:" + coverageItem.HelpText);
      //console.log("coverageItem.CovInputType:" + coverageItem.CovInputType);
      //console.log("coverageItem.LabelDescription:" + coverageItem.LabelDescription);
      if (coverageItem.Name == null) {
          //console.log("setting SuppressRendering true");
          coverageItem.SuppressRendering = 'true';
      }
      return this.fb.group({
          CovId: [coverageItem.CovId],
          CovCode: [coverageItem.CovCode],
          Name: [coverageItem.Name],
          Caption: [coverageItem.Caption],
          CovInputType: [coverageItem.CovInputType],
          SelectedLimit: coverageItem.CovInputType == "combo" ? this.fb.group({
              Caption: [coverageItem.SelectedLimit.Caption],
              Abbrev: [coverageItem.SelectedLimit.Abbrev],
              Value: [coverageItem.SelectedLimit.Value],
              SortOrder: [coverageItem.SelectedLimit.SortOrder],
              IsNoCov: [coverageItem.SelectedLimit.IsNoCov]
          }) : null,
          Premium: [coverageItem.Premium],
          Limits: coverageItem.CovInputType == "combo" ? this.fb.array([]) : null,
          SuppressRendering: [coverageItem.SuppressRendering],
          HelpText: [coverageItem.HelpText]
      });
  }
  patchLimit(covItem, k) {
      //this.limitControl = (<FormArray>this.coverageForm.controls['policyCoverageArray']).at(k).value.Limits; //this was a major bug!
      this.limitControl = (<FormArray>(<FormArray>this.policyCoverageControl).at(k).get('Limits'));
      
      covItem.Limits.Limit.forEach((limitItem) => {
          this.limitControl.push(this.patchLimitValues(limitItem))
      });
  }
  private patchLimitValues(limitItem): AbstractControl {
      return this.fb.group({
          Abbrev: [limitItem.Abbrev],
          Caption: [limitItem.Caption],
          Value: [limitItem.Value]
      })
  }

  loadSession(): Promise<void> {
      //console.log("top of loadSession");
      return this.sessionService.loadSession(this.guid, this.zipcode, this.ctid)
          .then(data => {
              this.sessionObject = data.WebSessionDRC;
              //console.log("end of loadSession");
              //console.log("coverages loadsess this.sessionObject.Quote.Customer.IpFirstNameOfCustomer=" + this.sessionObject.Quote.Customer.IpFirstNameOfCustomer);

          },
          error => alert(error));
  }
  ratedSave(): Promise<void> {
      //console.log("coveragees ratedSave!");
      //console.log("this.sessionObject.Quote.Customer.IpFirstNameOfCustomer = " + this.sessionObject.Quote.Customer.IpFirstNameOfCustomer);
      //console.log("this.sessionObject.Quote.Customer.IpAddressStateCode = " + this.sessionObject.Quote.Customer.IpAddressStateCode);
      return this.sessionService.ratedSave(this.sessionObject)
          .then(response => {
              console.log("coveragees ratedSave coveragees response=" + response);
              this.ratedSaveResult = response;
          },
          error => {
              this.ratedSaveResult = error;
              console.log(error);
              alert(error);
          });
  }
  loadCoveragesAndDiscounts(): Promise<void> {
      console.log("top of loadCoveragesAndDiscounts");

      return this.sessionService.loadCoveragesAndDiscounts(this.sessionObject)
          .then(data => {
              this.sessionObject = data.WebSessionDRC;
              console.log("end of loadCoveragesAndDiscounts");
          },
          error => alert(error));
  }
  openDialog() {
      let dialogRef = this.dialog.open(ManageCoverages);
      switch (this.dbops)
      {
          case DBOperation.vehicleCoverage:
              dialogRef.componentInstance.vehicleCoverage = this.vehicleCoverage;
              dialogRef.componentInstance.vehicleNumber = this.vehicleNumber;
              break;
          default:
              dialogRef.componentInstance.policyCoverages = this.sessionObject.PolicyCoverages;
              break;
      }
      dialogRef.componentInstance.dbops = this.dbops;
      dialogRef.componentInstance.modalTitle = this.modalTitle;
      dialogRef.componentInstance.modalBtnTitle = this.modalBtnTitle;
      dialogRef.componentInstance.sessionObject = this.sessionObject;
      dialogRef.updateSize('75%', '675px');
      dialogRef.updatePosition({ top: '100px' });

      dialogRef.afterClosed().subscribe(result => {
          if (result == "success") {
              this.loadSession()
              .then(() => {
                  this.loadCoveragesAndDiscounts()
                      .then(() => {
                          console.log("openDialog after loadCoveragesAndDiscounts");
                          console.log("this.sessionObject.TotalPremium " + this.sessionObject.TotalPremium);
                          this.TotalPremium = this.formatter.format(parseFloat(this.sessionObject.TotalPremium));
                          this.createForm();
                          this.patchForm();
                          this.LoadComplete = true;
                      });
              }
              );
              switch (this.dbops) {
                  case DBOperation.policyCoverage:
                      this.msg = "Policy coverages applied.";
                      break;
                  case DBOperation.vehicleCoverage:
                      this.msg = "Vehicle coverages applied.";
                      break;
                  case DBOperation.enhancedCoverages:
                      this.msg = "Enhanced coverages applied.";
                      break;
              }
          }
          else if (result == "error")
              this.msg = "There is some issue in saving records, please contact to system administrator!"
          else
              this.msg = result; 
      });
  }
  editVehicleCoverages(id: number) {
      this.dbops = DBOperation.vehicleCoverage;
      this.modalTitle = "Change Vehicle Coverages";
      this.modalBtnTitle = "Update and Recalculate";
      this.vehicleNumber = id;
      //console.log("id=" + id);
      //console.log("this.sessionObject.VehicleCoverages.VehicleCoverage[id].Year=" + this.sessionObject.VehicleCoverages.VehicleCoverage[id].Year);
      this.vehicleCoverage = this.sessionObject.VehicleCoverages.VehicleCoverage[id];
      //console.log("this.vehicleCoverage.Year=" + this.vehicleCoverage);
      this.openDialog();
  }
  editPolicyCoverages() {
      this.dbops = DBOperation.policyCoverage;
      this.modalTitle = "Change Liability Coverages";
      this.modalBtnTitle = "Update and Recalculate";
      this.openDialog();
  }
  getSelectedPayPlanValue(value) {
      console.log("getSelectedPayPlanValue value=" + value);
      this.selectedPayPlanValue = value;
      var arr = this.selectedPayPlanValue.split('~');
      this.nDiscountedPremium = parseFloat(arr[5]);
      this.DiscountedPremium = this.formatter.format(this.nDiscountedPremium);
      if (arr[0] == "0") {
          this.DownPaymentAmount = this.formatter.format(parseFloat(arr[2])) + " Pay-in-Full";
          this.NumberOfInstallments = "";
          this.InstallmentType = "";
          this.InstallmentAmount = "";
      }
      else {
          this.DownPaymentAmount = "Downpayment of " + this.formatter.format(parseFloat(arr[2]));
          this.NumberOfInstallments = arr[0];
          this.InstallmentType = arr[4] + " of";
          this.InstallmentAmount = this.formatter.format(parseFloat(arr[1]));
      }
      //update preferred payer discount, total discount based on new payplan selection:
      this.nPreferredPaymentAmount = parseFloat(arr[3]);
      this.preferredPaymentAmount = this.formatter.format(this.nPreferredPaymentAmount);
      this.nTotalDiscountSavings = parseFloat(this.sessionObject.TotalDiscountSavingsWithoutPreferredPayer) + this.nPreferredPaymentAmount;
      this.TotalDiscountSavings = this.formatter.format(this.nTotalDiscountSavings);
      console.log("getSelectedPayPlanValue this.DiscountedPremium=" + this.DiscountedPremium);
      console.log("getSelectedPayPlanValue this.NumberOfInstallments=" + this.NumberOfInstallments);
      console.log("getSelectedPayPlanValue this.InstallmentType=" + this.InstallmentType);
      console.log("getSelectedPayPlanValue this.InstallmentAmount=" + this.InstallmentAmount);
      console.log("getSelectedPayPlanValue this.DownPaymentAmount=" + this.DownPaymentAmount);
      console.log("getSelectedPayPlanValue this.preferredPaymentAmount=" + this.preferredPaymentAmount);
      console.log("getSelectedPayPlanValue this.TotalDiscountSavings=" + this.TotalDiscountSavings);
  }

  onSubmit() {
  }
}

