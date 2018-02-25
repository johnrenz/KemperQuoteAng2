import { Component, Input, OnChanges, OnInit, trigger, state, transition, style, animate } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Address } from './address';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { states } from './states';
import { HomeRentOptions } from '../applicant/HomeRentOptions';
import { HomeownerShipOptions } from '../applicant/HomeownerShipOptions';
import { HowDidYouHearOptions } from '../applicant/HowDidYouHearOptions';
import { WebSessionDRC } from '../WebSessionDRC/WebSessionDRC';
import { PolicyCoverage } from '../WebSessionDRC/PolicyCoverage';
import { SessionService } from '../session.service';
import { Http } from '@angular/http';
import * as xml2js from 'xml2js';
import { HeaderComponent } from '../header.component';
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

@Component({
  selector: 'app-applicant',
  templateUrl: './applicant.component.html',
  styleUrls: ['./applicant.component.css'],
  providers: [SessionService],
  animations: [
      trigger('visibilityChanged', [
          state('true', style({ opacity: 1, height: '40px' })),
          state('false', style({ opacity: 0, height: '0px', 'margin-bottom': '0px' })),
          transition('* => *', animate('1s'))
      ])
    ]
})
export class ApplicantComponent implements OnChanges, OnInit {
    
    sessionObject: WebSessionDRC; //this is the data model!!

    //binding vars for testing:
    getData: string;
    postData: string;
    postSessionData: string;
    postSessionXmlData: string;

  //@Input() applicant: Applicant;

  applicantForm : FormGroup;

  states = states;
  HomeRentOptions = HomeRentOptions;
  HomeownerShipOptions = HomeownerShipOptions;
  HowDidYouHearOptions = HowDidYouHearOptions;
  JSON = JSON;

    //HomeRentType
  IsHomeowner = true;
  IsRenter = false;
    
  private sub: any;
  guid: string;
  zipcode: string;
  ctid: string;

  LoadComplete = false;

  get FirstName() { return this.applicantForm.get('FirstName'); }
  get LastName() { return this.applicantForm.get('LastName'); }
  get AddressLine1() { return this.applicantForm.get('CurrentAddress.AddressLine1'); }
  get AddressLine2() { return this.applicantForm.get('CurrentAddress.AddressLine2'); }
  get City() { return this.applicantForm.get('CurrentAddress.City'); }
  get State() { return this.applicantForm.get('CurrentAddress.State'); }
  get ZipCode() { return this.applicantForm.get('CurrentAddress.ZipCode'); }
  //get AddressVerified() { return this.applicantForm.get('CurrentAddress.AddressVerified'); }
  get HomeRentType() { return this.applicantForm.get('HomeRentType'); }
  get HomeownerShipType() { return this.applicantForm.get('HomeownerShipType'); }
  get NoOfTimesMoved() { return this.applicantForm.get('NoOfTimesMoved'); }
  get UnitsInBuilding() { return this.applicantForm.get('UnitsInBuilding'); }
  get NoOfLicensedInHousehold() { return this.applicantForm.get('NoOfLicensedInHousehold'); }
  get HowDidYouHear() { return this.applicantForm.get('HowDidYouHear'); }
  get Email() { return this.applicantForm.get('Email'); }
  get SSN4() { return this.applicantForm.get('SSN4'); }

  submitted = false;

  maritalStatusOptions = ['Single', 'Married', 'Domestic Partnership', 'Divorced', 'Widowed'];

  constructor(private fb: FormBuilder,
      private sessionService: SessionService,
      private route: ActivatedRoute,
      private router: Router) {
    this.createForm();
  }

  createForm() {
      this.applicantForm = this.fb.group({
          FirstName: ['', Validators.required ],
          LastName: ['', Validators.required ],
          CurrentAddress: this.fb.group({
            AddressLine1: ['', Validators.required ],
            AddressLine2: '',
            City: ['', Validators.required ],
            State: ['', Validators.required],
            ZipCode: ['', Validators.required],
            AddressVerified: ['', Validators.required]
          }),
          HomeownerShipType: ['', Validators.required],
          HomeRentType: [''],
          HowDidYouHear: ['', Validators.required],
          NoOfLicensedInHousehold: ['', Validators.required],
          UnitsInBuilding: ['', Validators.required],
          NoOfTimesMoved: ['', Validators.required],
          SSN4: [''],
          Email: ['', Validators.required]
    });
  }

  ngOnInit() {
      this.sub = this.route.params.subscribe(params => {
          console.log("applicant.ngOnInit");
          this.guid = params['guid'];
          this.zipcode = params['zipcode'];
          this.ctid = params['ctid'];
          console.log("applicant.ngOnInit guid=" + this.guid);
          console.log("applicant.ngOnInit zipcode=" + this.zipcode);
          console.log("applicant.ngOnInit ctid=" + this.ctid);
      });
      //alert("ngOnINit 2 guid=" + this.guid);
      this.loadSession()
          .then(() => {
              if (this.sessionObject.Quote.Coverages.Coverage == null)
                  this.sessionObject.Quote.Coverages.Coverage = [
                      {
                          PolicyCoverage: new PolicyCoverage()
                      }];
                          
              this.applicantForm.setValue({
                  FirstName: this.sessionObject.Quote.Customer.IpFirstNameOfCustomer || '',
                  LastName: this.sessionObject.Quote.Customer.IpLastNameOfCustomer || '',
                  CurrentAddress: {
                      AddressLine1: this.sessionObject.Quote.Customer.IpAddressLine1 || '',
                      AddressLine2: this.sessionObject.Quote.Customer.IpAddressLine2 || '',
                      City: this.sessionObject.Quote.Customer.IpAddressCity || '',
                      State: this.sessionObject.Quote.Customer.IpAddressStateCode || '',
                      ZipCode: this.sessionObject.Quote.Customer.IpZipCode1 == null ? '' : this.zeroPad(this.sessionObject.Quote.Customer.IpZipCode1, 5),
                      AddressVerified: this.sessionObject.Quote.Customer.IpAddressVerificationTest || ''
                  },
                  HomeownerShipType: this.sessionObject.Quote.Coverages.Coverage[0].PolicyCoverage.IpHomeownershipType || '',
                  HomeRentType: this.sessionObject.Quote.Customer.IpRentOwnTest || '',
                  HowDidYouHear: '',
                  UnitsInBuilding: this.sessionObject.Quote.PolicyInfo.IpHoUnitsInBldg || '',
                  NoOfTimesMoved: this.sessionObject.Quote.Customer.IpNoOfAdd3Yrs || '',
                  SSN4: this.sessionObject.Quote.Customer.IpSocialSecurityNo || '',
                  Email: this.sessionObject.Quote.Customer.IpEMailAddress || '',
                  NoOfLicensedInHousehold: this.sessionObject.Quote.PolicyInfo.IpNoOfHhDrivers || ''
              });

              this.RentOrHome();
              this.LoadComplete = true;
          });
      this.subscribeHomeRentTypeChange();
  }

  subscribeHomeRentTypeChange() {
      const changes$ = this.HomeRentType.valueChanges;
      changes$.subscribe(homeRentValue => {
          console.log("subscribeHomeRentTypeChange homeRentValue=" + homeRentValue);
          if (homeRentValue == "1") {
              this.HomeownerShipType.setValidators(Validators.required);
              this.UnitsInBuilding.clearValidators();
              console.log("subscribeHomeRentTypeChange homeRentValue==1");
          }
          else {
              this.HomeownerShipType.clearValidators();
              this.UnitsInBuilding.setValidators(Validators.required);
              console.log("subscribeHomeRentTypeChange homeRentValue not=1");
          }
      });
  }

  ngOnChanges() {
    //   this.applicantForm.reset({
    //   FirstName : this.applicant.FirstName,
    //   LastName: this.applicant.LastName,
    //   CurrentAddress: this.applicant.currentAdress,
    //   ThreeYearsAtCurrentAddress: this.applicant.ThreeYearsAtCurrentAddress
    // });
  }
   
  onSubmit() {
      this.submitted = true; //needed?
      this.saveSession()
          .then((result) => {
              console.log("applicant save result=" + result);
              this.router.navigate(['/vehicles', this.guid, this.zipcode, this.ctid])
          });  
  }

  RentOrHome() {
      if (this.HomeRentType.value == "1") {
          this.IsHomeowner = true;
          this.IsRenter = false;
      }
      else {
          this.IsHomeowner = false;
          this.IsRenter = true;
      }

  }

  saveSession(): Promise<string> {
      this.sessionObject.Quote.Customer.IpFirstNameOfCustomer = this.FirstName.value;
      //alert("IPFirstName: " + this.sessionObject.Quote.Customer.IpFirstNameOfCustomer);
      //alert("sessionObject: " + JSON.stringify(this.sessionObject));
      this.sessionObject.Quote.Customer.IpFirstNameOfCustomer = this.FirstName.value;
      this.sessionObject.Quote.Customer.IpLastNameOfCustomer = this.LastName.value;
      this.sessionObject.Quote.Customer.IpAddressLine1 = this.AddressLine1.value;
      this.sessionObject.Quote.Customer.IpAddressLine2 = this.AddressLine2.value || '';
      this.sessionObject.Quote.Customer.IpAddressCity = this.City.value;
      this.sessionObject.Quote.Customer.IpAddressStateCode = this.State.value;
      this.sessionObject.Quote.Customer.IpZipCode1 = this.ZipCode.value;
      this.sessionObject.Quote.Customer.IpSocialSecurityNo = this.SSN4.value || '';
      this.sessionObject.Quote.Customer.IpEMailAddress = this.Email.value;
      this.sessionObject.Quote.Customer.IpRentOwnTest = this.HomeRentType.value;
      this.sessionObject.Quote.Coverages.Coverage[0].PolicyCoverage.IpHomeownershipType = this.HomeownerShipType.value;
      this.sessionObject.Quote.PolicyInfo.IpNoOfHhDrivers = this.NoOfLicensedInHousehold.value;
      this.sessionObject.Quote.PolicyInfo.IpHoUnitsInBldg = this.UnitsInBuilding.value;
      this.sessionObject.Quote.Customer.IpNoOfAdd3Yrs = this.NoOfTimesMoved.value;
      //TODO dont just default these:
      this.sessionObject.Quote.Customer.IpProductVersion = "4";
      this.sessionObject.Quote.Customer.IpCreditModel = "R96";
      this.sessionObject.Quote.Customer.IpCreditScore = "400";
      this.sessionObject.Quote.Customer.IpSpecialCorresNo = "5";

      this.sessionObject.AddInfo.ApplicantComplete = "true";
      return this.sessionService.saveSession(this.sessionObject); //this.webSession);
  }

  loadSession(): Promise<void> {
      //alert("loadSession guid=" + this.guid);


      if (this.guid == "new")
          return this.sessionService.createNewSession(this.zipcode, this.ctid)
              .then(data => {
                  this.sessionObject = data.WebSessionDRC;
                  this.guid = this.sessionObject.Guid;
                  console.log("new guid is " + this.guid);
              },
              error => alert(error));
      else
          return this.sessionService.loadSession(this.guid, this.zipcode, this.ctid)
              .then(data => {
                  this.sessionObject = data.WebSessionDRC;           
            },
            error => alert(error));
  }

  
  getSession() {
    this.getData = 'Here is data!';
    this.sessionService.getDate()
    .then(data => { this.getData = JSON.stringify(data)},
          error => alert(error))

    // .subscribe(
    //   data => this.getData = JSON.stringify(data),
    //   error => alert(error),
    //   () => console.log("got session")
    // );
  }
  postSession() {
      var xml = `<?xml version="1.0" encoding="UTF-8" ?><page><title>W3Schools Home Page</title></page>`;
      this.postSessionXmlData = 'Here is post session  xml!';
      this.postSessionData = 'Here is post session  data!';
      //var parseString = require('xml2js').parseString;
      var guid = this.guid; //"4AAF7071-60FC-4246-B854-1141CFD043D5";
      var zip = this.zipcode; // "60103";
      var ctid = this.ctid; //"80001";
      this.sessionService.loadSession(guid, zip, ctid)
          .then(data => this.postSessionData = JSON.stringify(data.WebSessionDRC),
          //.then(data => { this.postSessionXmlData = data.toString(); xml2js.parseString(data, (err, result) => this.postSessionData = JSON.stringify(result)); },
          error => alert(error))

    // .subscribe(
    //   data => this.getData = JSON.stringify(data),
    //   error => alert(error),
    //   () => console.log("got session")
    // );
  }
  postGuidewire() {
    this.postData = 'Here is post data!';
    this.sessionService.postGuidewire()
    .then(data => { this.postData = JSON.stringify(data)},
          error => alert(error))

    // .subscribe(
    //   data => this.getData = JSON.stringify(data),
    //   error => alert(error),
    //   () => console.log("got session")
    // );
  }
  zeroPad(num, places) {
    var zero = places - num.toString().length + 1;
    return Array(+(zero > 0 && zero)).join("0") + num;
  }
}
