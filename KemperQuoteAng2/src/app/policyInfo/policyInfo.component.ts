import { Component, OnInit, trigger, state, animate, transition, style } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HeaderComponent } from '../header.component';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import {
    MatAutocompleteModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatCardModule,
    MatCheckboxModule,
    MatChipsModule,
    MatDatepickerModule,
    MatDatepickerInputEvent,
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
import { CdkTableModule } from '@angular/cdk/table';
import { WebSessionDRC } from '../WebSessionDRC/WebSessionDRC';
import { SessionService } from '../session.service';
import { Utilities } from '../Utilities';
import { HowLongInsuredOptions } from './HowLongInsuredOptions';
import { MonthsInsuredOptions } from './MonthsInsuredOptions';
import { BiLimitOptions } from './BiLimitOptions';
import { YesNoOptions } from './YesNoOptions';
import { Http } from '@angular/http';
import 'hammerjs';

@Component({
  selector: 'app-policyInfo',
  templateUrl: './policyInfo.component.html',
  styleUrls: ['./policyInfo.component.css'],
  providers: [SessionService],
  animations: [
      trigger('visibilityChanged', [
          state('true', style({ opacity: 1, height: 'auto', display: 'block' })),
          state('false', style({ opacity: 0, height: '0px', 'margin-bottom': '0px', display: 'none' })),
          transition('* => *', animate('1s'))
      ])
  ]
})
export class PolicyInfoComponent implements OnInit {

    constructor(private fb: FormBuilder,
        private sessionService: SessionService,
        private route: ActivatedRoute,
        private router: Router) {

        //console.log("systemDate=" + this.systemDate);
        //this.serializedDate = new FormControl(this.getTomorrowsDate(this.systemDate).toISOString());
        //this.minExpDate = this.getTomorrowsDate(this.systemDate);
        //this.maxExpDate = new Date();
        //this.maxExpDate.setFullYear(this.maxExpDate.getFullYear() + 1);
        this.createForm();
    }

  policyInfoForm: FormGroup;
  private sub: any;
  guid: string;
  zipcode: string;
  ctid: string;
  sessionObject: WebSessionDRC; //this is the data model!!
  CurrentlyInsuredYes = false;
  CurrentlyInsuredNo = true;
  HowLongInsuredOptions = HowLongInsuredOptions;
  BiLimitOptions = BiLimitOptions;
  MonthsInsuredOptions = MonthsInsuredOptions;
  YesNoOptions = YesNoOptions;  
  //serializedDate = new FormControl(new Date().toISOString());
  minExpDate: Date;
  maxExpDate: Date;
  LoadComplete = false;
  systemDate = new Date().toDateString();
  serializedDate: FormControl;

  get CurrentlyInsured() { return this.policyInfoForm.get('CurrentlyInsured'); }
  set CurrentlyInsured(val) { this.policyInfoForm.get('CurrentlyInsured').setValue(val); }
  get HowLongInsured() { return this.policyInfoForm.get('HowLongInsured'); }
  get Last30() { return this.policyInfoForm.get('Last30'); }
  get MonthsInsured() { return this.policyInfoForm.get('MonthsInsured'); }
  get CurrentLimits() { return this.policyInfoForm.get('CurrentLimits'); }
  get PriorLimits() { return this.policyInfoForm.get('PriorLimits'); }
  get LapseLessThanOneYear() { return this.policyInfoForm.get('LapseLessThanOneYear'); }
  get ReasonNoCoverage() { return this.policyInfoForm.get('ReasonNoCoverage'); }
  get ExpireDate() { return this.policyInfoForm.get('ExpireDate'); }
  get ExpireDateValid() { return this.policyInfoForm.get('ExpireDate').valid; }

  getTomorrowsDate(systemDate) {
      console.log("systemDate=" + systemDate);
      var tomorrow = new Date(systemDate);
      console.log("tomrrow1 = " + tomorrow);
      tomorrow.setDate(tomorrow.getDate() + 1);
      console.log("tomrrow2 = " + tomorrow);
      return tomorrow;
  }
  createForm() {
      this.policyInfoForm = this.fb.group({
          CurrentlyInsured: ['0'],
          HowLongInsured: ['0', Validators.required],
          Last30: ['', Validators.required],
          MonthsInsured: ['0', Validators.required],
          CurrentLimits: ['0', Validators.required],
          PriorLimits: ['0', Validators.required],
          LapseLessThanOneYear: ['0', Validators.required],
          ReasonNoCoverage: ['0', Validators.required],
          ExpireDate: ['', Validators.required]
      });
  }
  ngOnInit() {
      this.sub = this.route.params.subscribe(params => {
          this.guid = params['guid'];
          this.zipcode = params['zipcode'];
          this.ctid = params['ctid'];
      });
      this.loadSession()
          .then(() => {
              //console.log('PolicyExpDate:' + this.sessionObject.Quote.QuoteInfo.IpPolicyExpDate);
              //if (isNaN(Date.parse(this.sessionObject.Quote.QuoteInfo.IpPolicyExpDate)))
              //    console.log("this.sessionObject.Quote.QuoteInfo.IpPolicyExpDate is NOT a date");
              //else
              //    console.log("this.sessionObject.Quote.QuoteInfo.IpPolicyExpDate IS a date");
              //if (isNaN(Date.parse("01/01/1980")))
              //    console.log("01/01/1980 is NOT a date");
              //else
              //    console.log("01/01/1980 IS a date");

              //moved from ctr
              this.systemDate = this.sessionObject.AddInfo.SystemDate;
              console.log("systemDate=" + this.systemDate);
              this.serializedDate = new FormControl(this.getTomorrowsDate(this.systemDate).toISOString());
              this.minExpDate = this.getTomorrowsDate(this.systemDate);
              this.maxExpDate = new Date();
              this.maxExpDate.setFullYear(this.maxExpDate.getFullYear() + 1);

              if (isNaN(Date.parse(this.sessionObject.Quote.QuoteInfo.IpPolicyExpDate)))
                  this.sessionObject.Quote.QuoteInfo.IpPolicyExpDate = this.getTomorrowsDate(this.sessionObject.AddInfo.SystemDate).toLocaleDateString();
              console.log("this.sessionObject.AddInfo.SystemDate=" + this.sessionObject.AddInfo.SystemDate);
              this.serializedDate = new FormControl((new Date(this.sessionObject.Quote.QuoteInfo.IpPolicyExpDate)).toISOString());
              this.policyInfoForm.setValue({
                  //1=no prior, 2=no lapse, 3=1-30, 4=30+, 6=lapse 2 year, 7=lapse 3 year, 10=no but last 30
                  CurrentlyInsured: "2,6,7".includes(this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed) ? '1' : '0',
                  Last30: "3,10".includes(this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed) ? '1' : '0',
                  HowLongInsured: "2,6,7".includes(this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed) ? '2' : '0',
                  MonthsInsured: "2,6,7".includes(this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed) ? '4' : '0',
                  CurrentLimits: "2,6,7".includes(this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed) ? this.sessionObject.Quote.Customer.IpCurrentLimits : '0',
                  PriorLimits: "3,10".includes(this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed) ? this.sessionObject.Quote.Customer.IpCurrentLimits : '0',
                  LapseLessThanOneYear: '0',
                  ExpireDate: this.serializedDate,
                  ReasonNoCoverage: '0',
              })
              this.LoadComplete = true;
          });
  }
  loadSession(): Promise<void> {
      return this.sessionService.loadSession(this.guid, this.zipcode, this.ctid)
          .then(data => {

              this.sessionObject = data.WebSessionDRC;
              console.log("loadsess this.sessionObject.AddInfo.SystemDate=" + this.sessionObject.AddInfo.SystemDate);

          },
          error => alert(error));
  }


  onSubmit() {
      //this.submitted = true; //used to display secoind version of form if already submitted.
      this.saveSession()
          .then((result) => {
              console.log("policyInfo save result=" + result);
              if (this.sessionObject.AddInfo.ApplicantComplete != "true")
                  this.router.navigate(['/applicant', this.guid, this.zipcode, this.ctid]);
              else if (this.sessionObject.AddInfo.VehiclesComplete != "true")
                  this.router.navigate(['/vehiclesList', this.guid, this.zipcode, this.ctid]);
              else if (this.sessionObject.AddInfo.DriversComplete != "true")
                  this.router.navigate(['/drivers', this.guid, this.zipcode, this.ctid]);
              this.router.navigate(['/coverages', this.guid, this.zipcode, this.ctid])
          });  
  }

  saveSession() {
      if (this.CurrentlyInsured.value == "1") //yes 
      {
          this.sessionObject.Quote.Customer.IpCurrentCarrierNo = '19';
          this.sessionObject.Quote.Customer.IpCurrentCarrierType = '3';
          this.sessionObject.AddInfo.AiCurrInsLapse = '10';
          this.sessionObject.Quote.Customer.IpCurrentLimits = this.CurrentLimits.value;
          if (this.sessionObject.Quote.Customer.IpAddressStateCode == 'CA') {
              if (this.HowLongInsured.value == '1') //no insurance last 12 months
              {
                  this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed = '1'; //Not insured
                  this.sessionObject.Quote.Customer.IpCurrentCarrierNo = '20'; //None
                  this.sessionObject.Quote.Customer.IpCurrentCarrierType = '1'; //None
                  this.sessionObject.Quote.PolicyInfo.IpRetroLoyaltyLevel = '0'; //None
                  this.sessionObject.AddInfo.AiCurrInsLapse = '1'; //Set  NO-OF-DAYS-LAPSED to 1. (Not insured)
                  this.sessionObject.Quote.Customer.IpCurrentLimits = '0';
              }
              else if (this.HowLongInsured.value == '99') //less than 1 year
              {
                  this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed = '9'; //1-11 MONTHS
                  this.sessionObject.Quote.PolicyInfo.IpRetroLoyaltyLevel = '1';
              }
              else {
                  this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed = '2'; //treat as no lapse
                  this.sessionObject.Quote.PolicyInfo.IpRetroLoyaltyLevel = '2';
              }
          }
          else //not CA
          {
              switch (this.HowLongInsured.value) {
                  case "1": //No insurance for last 12 months
                      this.sessionObject.AddInfo.AiNoCoverageReason = this.ReasonNoCoverage.value;
                      this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed = '1'; //Set  NO-OF-DAYS-LAPSED to 1. (Not insured)
                      this.sessionObject.Quote.Customer.IpCurrentCarrierNo = '20'; //No Prior Insurance
                      this.sessionObject.Quote.Customer.IpCurrentCarrierType = '1'; //None
                      this.sessionObject.Quote.PolicyInfo.IpRetroLoyaltyLevel = '1';
                      this.sessionObject.Quote.Customer.IpCurrentLimits = '6';
                      break;
                  case "99": // Less than 1 year
                      this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed = this.LapseLessThanOneYear.value;
                      this.sessionObject.Quote.PolicyInfo.IpRetroLoyaltyLevel = '1';
                      this.sessionObject.Quote.Customer.IpCurrentLimits = '6';
                      break;
                  case "2": //12 or more
                  case "6":
                  case "7":
                      if (this.MonthsInsured.value == "1") //1 to 11 months
                      {
                          this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed = '9'; //1-11 months
                          this.sessionObject.Quote.PolicyInfo.IpRetroLoyaltyLevel = '1';
                      }
                      if ((this.MonthsInsured.value == "2") ||
                          (this.MonthsInsured.value == "2X")) //12-23
                      {
                          this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed = '2'; //treat as no lapse
                          this.sessionObject.Quote.PolicyInfo.IpRetroLoyaltyLevel = '2';
                      }
                      if ((this.MonthsInsured.value == "3") ||
                          (this.MonthsInsured.value == "3X")) //24-35
                      {
                          this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed = '2'; //treat as no lapse
                          this.sessionObject.Quote.PolicyInfo.IpRetroLoyaltyLevel = '3';
                      }
                      if (this.MonthsInsured.value == "4") //36 or more
                      {
                          this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed = '2'; //treat as no lapse
                          this.sessionObject.Quote.PolicyInfo.IpRetroLoyaltyLevel = '4';
                      }
                      if (!Utilities.IsVibeState(this.sessionObject.Quote.Customer.IpAddressStateCode))
                          this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed = this.HowLongInsured.value;
                      break;
              }
          }
      }
      else //CurrentlyInsured == '0' no
      {
          if (this.Last30.value == "1") {
              if (Utilities.IsVibeState(this.sessionObject.Quote.Customer.IpAddressStateCode))
              {
                  this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed = '10';
                  this.sessionObject.AddInfo.AiCurrInsLapse = "10";
              }
              else
              {
                  this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed = '3';
                  this.sessionObject.AddInfo.AiCurrInsLapse = "3"; //DAYS-1-30
              }
              this.sessionObject.Quote.Customer.IpCurrentLimits = this.PriorLimits.value;
              this.sessionObject.Quote.Customer.IpCurrentCarrierNo = '19';

              this.sessionObject.Quote.Customer.IpCurrentCarrierType = '3';
              this.sessionObject.Quote.PolicyInfo.IpRetroLoyaltyLevel = '0';

          }
          else {
              this.sessionObject.Quote.PolicyInfo.IpNoOfDaysLapsed = '1'; //Set  NO-OF-DAYS-LAPSED to 1. (Not insured)
              this.sessionObject.Quote.Customer.IpCurrentCarrierNo = '20'; //No Prior Insurance
              this.sessionObject.Quote.Customer.IpCurrentCarrierType = '1'; //None
              this.sessionObject.Quote.PolicyInfo.IpRetroLoyaltyLevel = '0'; //None
              this.sessionObject.AddInfo.AiCurrInsLapse = "1"; //Set  NO-OF-DAYS-LAPSED to 1. (Not insured)
              this.sessionObject.Quote.Customer.IpCurrentLimits = '0';
          }
      }
      this.sessionObject.Quote.PolicyInfo.IpEffDate = this.ExpireDate.value;
      this.sessionObject.Quote.QuoteInfo.IpPolicyEffDate = this.ExpireDate.value;
      this.sessionObject.Quote.PolicyInfo.IpExpDate = this.ExpireDate.value;
      this.sessionObject.Quote.QuoteInfo.IpPolicyExpDate = this.ExpireDate.value;
      this.sessionObject.Quote.QuoteInfo.IpOrigQuoteDate = this.ExpireDate.value;
      //var expDate = new Date(this.ExpireDate.value);
      //expDate.setFullYear(expDate.getFullYear() + 1);
      //console.log("expDate.toLocaleDateString()=" + expDate.toLocaleDateString());
      //this.sessionObject.Quote.QuoteInfo.IpPolicyExpDate = expDate.toLocaleDateString();
      //this.sessionObject.Quote.PolicyInfo.IpExpDate = expDate.toLocaleDateString();

      this.sessionObject.AddInfo.PolicyInfoComplete = "true";
      return this.sessionService.saveSession(this.sessionObject); //this.webSession);
  }
  SetCurrentlyInsured(val) {
      this.CurrentlyInsured = val;
      this.CurrentlyInsuredYes = this.CurrentlyInsured.value == '1';
      this.CurrentlyInsuredNo = this.CurrentlyInsured.value == '0';
  }
  updateExpireDateControl(event: MatDatepickerInputEvent<Date>) {
      this.ExpireDate.setValue(event.value); //.toLocaleDateString()
  }
}
