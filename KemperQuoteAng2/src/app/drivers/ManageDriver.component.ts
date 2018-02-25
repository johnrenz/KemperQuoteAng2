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
import { MatDialog, MatDialogRef } from '@angular/material';
import { DBOperation } from './enum';
import { CdkTableModule } from '@angular/cdk/table';
import { WebSessionDRC } from '../WebSessionDRC/WebSessionDRC';
import { Driver } from '../WebSessionDRC/Driver';
import { SessionService } from '../session.service';
import { Utilities } from '../Utilities';
import { YesNoOptions } from './YesNoOptions';
import { MaleFemaleOptions } from './MaleFemaleOptions';
import { RelationshipOptions } from './RelationshipOptions';
import { MaritalStatusOptions } from './MaritalStatusOptions';
import { Http } from '@angular/http';
import 'hammerjs';
import { DriverItem } from './DriverItem';

@Component({
    templateUrl: './ManageDriver.component.html',
  providers: [SessionService],
  animations: [
      trigger('visibilityChanged', [
          state('true', style({ opacity: 1, height: 'auto', display: 'block' })),
          state('false', style({ opacity: 0, height: '0px', 'margin-bottom': '0px', display: 'none' })),
          transition('* => *', animate('1s'))
      ])
  ]
})
export class ManageDriver implements OnInit {

    constructor(private fb: FormBuilder,
        private sessionService: SessionService,
        private route: ActivatedRoute,
        private router: Router,
        public dialogRef: MatDialogRef<ManageDriver>) {
        this.minDOB = new Date();
        this.minDOB.setFullYear(this.minDOB.getFullYear() - 90);
        this.maxDOB = new Date();
        this.maxDOB.setFullYear(this.maxDOB.getFullYear() - 15);
        this.createForm()
    }

    dbops: DBOperation;
    modalTitle: string;
    modalBtnTitle: string;

    driversForm: FormGroup;
    private sub: any;
    guid: string;
    zipcode: string;
    ctid: string;
    sessionObject: WebSessionDRC; //this is the data model!!
    driver: DriverItem;
    serializedDOB = new FormControl(this.getDefaultDOB().toISOString());

    YesNoOptions = YesNoOptions;
    MaleFemaleOptions = MaleFemaleOptions;
    RelationshipOptions = RelationshipOptions;
    MaritalStatusOptions = MaritalStatusOptions
    minDOB: Date;
    maxDOB: Date;

    get Driver1() { return this.driversForm.get('Driver1'); };
    get Driver1FirstName() { return this.driversForm.get('Driver1.IpDrivFirst'); };
    get Driver1LastName() { return this.driversForm.get('Driver1.IpDrivLast'); };
    get Driver1BirthDate() { return this.driversForm.get('Driver1.IpBirthDateOfDriv'); };
    get Driver1Relationship() { return this.driversForm.get('Driver1.IpRelationToResp'); };
    get Driver1MarriedSingle() { return this.driversForm.get('Driver1.IpDrivMarriedSingle'); };
    get Driver1Sex() { return this.driversForm.get('Driver1.IpDrivSex'); };

    getDefaultDOB(): Date {
        var dob = new Date();
        dob.getDate();
        dob.setFullYear(dob.getFullYear() - 20);
        return dob;
    }
    createForm() {
        this.driversForm = this.fb.group({
            Driver1: this.fb.group({
                id: [''],
                IpOrigDriverNo: [''],
                IpDrivFirst: ['', Validators.required],
                IpDrivLast: ['', Validators.required],
                IpBirthDateOfDriv: [''],
                IpRelationToResp: ['', Validators.required],
                IpDrivMarriedSingle: ['', Validators.required],
                IpDrivSex: ['', Validators.required],
                IpDrivLicStateNo: [''],
                IpDateLicensed: [''],
                IpOccupationCat: [''],
                IpMatureDriverDis: [''],
                IpOperatorType: [''],
                IpEducationTest: [''],
                IpGoodStudentDis: [''],
                IpDriverStatus: [''],
                IpVehOfDriv: ['']
            })
        });
    }
    ngOnInit() {
        
        this.driversForm.valueChanges.subscribe(data => this.onValueChanged(data));
        this.onValueChanged();
        if (this.dbops == DBOperation.create)
            this.driversForm.reset();
        else
        {
            if (!isNaN(Date.parse(this.driver.IpBirthDateOfDriv))) {
                this.serializedDOB = new FormControl((new Date(this.driver.IpBirthDateOfDriv)).toISOString());
            }
            this.driversForm.setValue({
                Driver1: this.driver               
            });
        }
            
        this.SetControlsState(this.dbops == DBOperation.delete ? false : true);
        //this.Driver1BirthDate.setValue("01/01/1980");
              
    }
    onValueChanged(data?: any) {
        if (!this.driversForm) { return; }
        const form = this.driversForm;
        for (const field in this.formErrors) {
            // clear previous error message (if any)
            this.formErrors[field] = '';
            const control = form.get(field);
            if (control && control.dirty && !control.valid) {
                const messages = this.validationMessages[field];
                for (const key in control.errors) {
                    console.log("messages[key]=" + messages[key]);
                    this.formErrors[field] += messages[key] + ' ';
                }
            }
        }
    }
    formErrors = {
        'IpDrivFirst': '', 
        'IpDrivLast': '', 
        'IpBirthDateOfDriv': '',
        'IpRelationToResp': '',
        'IpDrivMarriedSingle': '', 
        'IpDrivSex': ''  
    };
    validationMessages = {
        'IpDrivFirst': {
            'required': ' required.'
        },
        'IpDrivLast': {
            'required': ' required.'
        },
        'IpBirthDateOfDriv': {
            'required': ' required.'
        },
        'IpRelationToResp': {
            'required': ' required.'
        },
        'IpDrivMarriedSingle': {
            'required': ' required.'
        },
        'IpDrivSex': {
            'required': ' required.'
        }
    }

    SetControlsState(isEnable: boolean) {
        isEnable ? this.driversForm.enable() : this.driversForm.disable();
    }
    
    onSubmit(formData: any) {
        switch (this.dbops) {
            case DBOperation.create:
                var count: number = +this.sessionObject.Quote.Drivers.IpDriverCt;
                //alert("count: " + count);
                count++;
                //alert("count: " + count);
                this.sessionObject.Quote.Drivers.IpDriverCt = "" + count++;
                this.sessionObject.Quote.Drivers.Driver.length++;
                this.sessionObject.Quote.Drivers.Driver[count - 1] = new Driver();
                this.sessionObject.Quote.Drivers.Driver[count - 1]['@SLICE'] = this.sessionObject.Quote.Drivers.IpDriverCt
                this.saveDriver(count - 1);
                this.dialogRef.close("success");
                break;
            case DBOperation.update:
                this.saveDriver(this.driver.id);
                this.dialogRef.close("success");
                break;
            case DBOperation.delete:
                this.deleteDriver(this.driver.id);
                this.dialogRef.close("success");
                break;
        }
    }
    deleteDriver(i: number) {
        this.sessionObject.Quote.Drivers.Driver.splice(i, 1);
        this.sessionService.saveSession(this.sessionObject)
            .then((result) => {
                console.log("deleteDriver save result=" + result);
            });  
    }
    saveDriver(i: number) {
        this.sessionObject.Quote.Drivers.Driver[i].IpDrivFirst = this.Driver1FirstName.value;
        this.sessionObject.Quote.Drivers.Driver[i].IpDrivLast = this.Driver1LastName.value;
        this.sessionObject.Quote.Drivers.Driver[i].IpBirthDateOfDriv = this.Driver1BirthDate.value;
        this.sessionObject.Quote.Drivers.Driver[i].IpRelationToResp = this.Driver1Relationship.value;
        this.sessionObject.Quote.Drivers.Driver[i].IpDrivSex = this.Driver1Sex.value;
        this.sessionObject.Quote.Drivers.Driver[i].IpDrivMarriedSingle = this.Driver1MarriedSingle.value;
        this.sessionObject.Quote.Drivers.Driver[i].IpOrigDriverNo = (i+1).toString();
        this.sessionObject.Quote.Drivers.Driver[i].IpOperatorType = "1";
        this.sessionObject.Quote.Drivers.Driver[i].IpDrivLicStateNo = this.ConvertState(this.sessionObject.Quote.Customer.IpAddressStateCode);
        if (this.sessionObject.Quote.Vehicles.Vehicle.length > i)
            this.sessionObject.Quote.Drivers.Driver[i].IpVehOfDriv = (i + 1).toString();
        else
            this.sessionObject.Quote.Drivers.Driver[i].IpVehOfDriv = "1";

        this.sessionObject.AddInfo.DriversComplete = "true";
        this.sessionService.saveSession(this.sessionObject)
            .then((result) => {
                console.log("saveDriver save result=" + result);
            });  
    }
    updateBirthDateControl(event: MatDatepickerInputEvent<Date>) {
        console.log(" value=" + event.value);
        this.Driver1BirthDate.setValue(event.value); //.toLocaleDateString()
    }
    ConvertState(findNumber: string): string {
        const states = [
            { number: "54", code: "AK" },
            { number: "01", code: "AL" },
            { number: "03", code: "AZ" },
            { number: "02", code: "CA" },
            { number: "05", code: "CO" },
            { number: "06", code: "CT" },
            { number: "08", code: "DC" },
            { number: "07", code: "DE" },
            { number: "09", code: "FL" },
            { number: "10", code: "GA" },
            { number: "52", code: "HI" },
            { number: "14", code: "IA" },
            { number: "11", code: "ID" },
            { number: "12", code: "IL" },
            { number: "13", code: "IN" },
            { number: "15", code: "KS" },
            { number: "16", code: "KY" },
            { number: "17", code: "LA" },
            { number: "20", code: "MA" },
            { number: "19", code: "MD" },
            { number: "18", code: "ME" },
            { number: "21", code: "MI" },
            { number: "22", code: "MN" },
            { number: "24", code: "MO" },
            { number: "23", code: "MS" },
            { number: "25", code: "MT" },
            { number: "32", code: "NC" },
            { number: "33", code: "ND" },
            { number: "26", code: "NE" },
            { number: "28", code: "NH" },
            { number: "29", code: "NJ" },
            { number: "30", code: "NM" },
            { number: "27", code: "NV" },
            { number: "31", code: "NY" },
            { number: "34", code: "OH" },
            { number: "36", code: "OR" },
            { number: "37", code: "PA" },
            { number: "38", code: "RI" },
            { number: "39", code: "SC" },
            { number: "40", code: "SD" },
            { number: "41", code: "TN" },
            { number: "42", code: "TX" },
            { number: "43", code: "UT" },
            { number: "45", code: "VA" },
            { number: "44", code: "VT" },
            { number: "46", code: "WA" },
            { number: "48", code: "WI" },
            { number: "47", code: "WV" },
            { number: "49", code: "WY" }
        ];
        var result = states.find((state) => { return state.number == findNumber });
        if (result)
            return result.code;
        result = states.find((state) => { return state.code == findNumber });
        if (result)
            return result.number;
        else
            return "";
    }
}
