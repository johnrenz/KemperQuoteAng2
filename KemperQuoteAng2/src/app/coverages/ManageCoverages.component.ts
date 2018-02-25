import { Component, OnInit, trigger, state, animate, transition, style } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HeaderComponent } from '../header.component';
import { FormBuilder, FormControl, FormGroup, Validators, FormArray, AbstractControl } from '@angular/forms';
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
import { Vehicle } from '../WebSessionDRC/Vehicle';
import { SessionService } from '../session.service';
import { Utilities } from '../Utilities';
import { Http } from '@angular/http';
import 'hammerjs';
import { VehicleCoverage, PolicyCoverages } from '../WebSessionDRC/SessionCoverage';

@Component({
    templateUrl: './ManageCoverages.component.html',
    providers: [SessionService],
    animations: [
        trigger('visibilityChanged', [
            state('true', style({ opacity: 1, height: 'auto', display: 'block' })),
            state('false', style({ opacity: 0, height: '0px', 'margin-bottom': '0px', display: 'none' })),
            transition('* => *', animate('1s'))
        ])
    ]
})
export class ManageCoverages implements OnInit {

    constructor(private fb: FormBuilder,
        private sessionService: SessionService,
        private route: ActivatedRoute,
        private router: Router,
        public dialogRef: MatDialogRef<ManageCoverages>) {

        this.createForm()
    }

    dbops: DBOperation;
    modalTitle: string;
    modalBtnTitle: string;

    coveragesForm: FormGroup;
    private sub: any;
    guid: string;
    zipcode: string;
    ctid: string;
    sessionObject: WebSessionDRC; //this is the data model!!
    vehicleCoverage: VehicleCoverage;
    vehicleNumber: number;
    policyCoverages: PolicyCoverages;
    private coveragesControl: FormArray;
    private limitControl: FormArray;

    coverageOptions = [{ Value: "Please Select", Description: "Please Select" }];
    selectedCoverageOption: string;

    coverageErrors: string[];
    
    RecalculateComplete = true;

    get Year() { return this.coveragesForm.get('Year'); }
    get Make() { return this.coveragesForm.get('Make'); }
    get Model() { return this.coveragesForm.get('Model'); }
    get DBOperation() { return this.dbops; }

    createForm() {
        this.coveragesForm = this.fb.group({
            Year: [''],
            Make: [''],
            Model: [''],
            VehicleNumber: [''],
            StateFeeOption1: [''],
            CoveragesArray: this.fb.array([])
        });
        this.coveragesControl = <FormArray>this.coveragesForm.controls['CoveragesArray'];
    }
    ngOnInit() {        
        this.coveragesForm.valueChanges.subscribe(data => this.onValueChanged(data));
        this.onValueChanged();
        if (this.dbops == DBOperation.policyCoverage) 
            this.patchPolicyCoverage();
        if (this.dbops == DBOperation.vehicleCoverage) 
            this.patchVehicleCoverages();            
        //this.SetControlsState(this.dbops == DBOperation.delete ? false : true);        
    }
    patchPolicyCoverage() {
        var k: number = 0;
        this.policyCoverages.Coverage.forEach((covItem) => {
            this.coveragesControl.push(this.patchCoverageValues(covItem))
            if (covItem.CovInputType == "combo")
                this.patchLimit(covItem, k)
            k++;
        });
    }
    patchVehicleCoverages() {
        console.log("vehicleCoverage.Year=" + this.vehicleCoverage.Year);
        console.log("this.coveragesForm.get(Year).value = " + this.coveragesForm.get('Year').value);
        this.coveragesForm.get('Year').setValue(this.vehicleCoverage.Year);
        console.log("this.coveragesForm.get(Year).value after set = " + this.coveragesForm.get('Year').value);
        this.coveragesForm.get('Make').setValue(this.vehicleCoverage.Make);
        this.coveragesForm.get('Model').setValue(this.vehicleCoverage.Model);
        this.coveragesForm.get('VehicleNumber').setValue(this.vehicleCoverage.VehicleNumber);
        this.coveragesForm.get('StateFeeOption1').setValue(this.vehicleCoverage.StateFeeOption1);
        var k: number = 0;
        console.log("this.vehicleCoverage.Coverages.Coverage.length=" + this.vehicleCoverage.Coverages.Coverage.length);
        this.vehicleCoverage.Coverages.Coverage.forEach((covItem) => {
            console.log("covItem.Name=" + covItem.Name);
            this.coveragesControl.push(this.patchCoverageValues(covItem))
            if (covItem.CovInputType == "combo")
                this.patchLimit(covItem, k)
            k++;
        });
    }
    private patchCoverageValues(coverageItem): AbstractControl {
        if (coverageItem.Name == null) {
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
        this.limitControl = (<FormArray>(<FormArray>this.coveragesControl).at(k).get('Limits'));
        //console.log("covItem.Limits.Limit.length = " + covItem.Limits.Limit.length);
        covItem.Limits.Limit.forEach((limitItem) => {
            this.limitControl.push(this.patchLimitValues(limitItem))
        });
    }
    private patchLimitValues(limitItem): AbstractControl {
        //console.log("limitItem.Caption= " + limitItem.Caption);
        return this.fb.group({
            Abbrev: [limitItem.Abbrev],
            Caption: [limitItem.Caption],
            Value: [limitItem.Value]
        })
    }
    onValueChanged(data?: any) {
            if (!this.coveragesForm) { return; }
        const form = this.coveragesForm;
        for (const field in this.formErrors) {
            // clear previous error message (if any)
            this.formErrors[field] = '';
            const control = form.get(field);
            if (control && control.dirty && !control.valid) {
                const messages = this.validationMessages[field];
                for (const key in control.errors) {
                    this.formErrors[field] += messages[key] + ' ';
                }
            }
        }
    }
    formErrors = {
        'CoveragesError1': '',
        'CoveragesError2': '',
        'CoveragesError3': ''
    }
    validationMessages = {
        'CoveragesError1': {
            'required': ' required.'
        },
        'CoveragesError2': {
            'required': ' required.'
        },
        'CoveragesError3': {
            'required': ' required.'
        }
    }

    SetControlsState(isEnable: boolean) {
            isEnable ? this.coveragesForm.enable() : this.coveragesForm.disable();
    }

    //showSelectedValue(value) {
    //    console.log("showSelectedValue value=" + value);
    //}

    onSubmit(formData: any) {
        this.RecalculateComplete = false;
        switch (this.dbops) {
            case DBOperation.policyCoverage:
                this.savePolicyCoverages()
                    .then(() => {
                        console.log("after savePolicyCoverages ");
                        if (this.coverageErrors == null || this.coverageErrors.length == 0) 
                            this.dialogRef.close("success");
                        console.log("after savePolicyCoverages dialognotclosed ");

                    });
                break;
            case DBOperation.vehicleCoverage:
                this.saveVehicleCoverages()
                    .then(() => {
                        console.log("after saveVehicleCoverages ");
                        if (this.coverageErrors == null || this.coverageErrors.length == 0)
                            this.dialogRef.close("success");
                        console.log("after saveVehicleCoverages dialognotclosed ");

                    });
                break;
            case DBOperation.enhancedCoverages:
                //this.saveVehicle(this.vehicle.id);
                this.dialogRef.close("success");
                break;
        }
    }
    saveVehicleCoverages(): Promise<void> {
        for (var i = 0; i < this.sessionObject.VehicleCoverages.VehicleCoverage[this.vehicleNumber].Coverages.Coverage.length; i++) {
            console.log("before: this.sessionObject.VehicleCoverages.VehicleCoverage[this.vehicleNumber].Coverages.Coverage[i].SelectedLimit.Value=" + this.sessionObject.VehicleCoverages.VehicleCoverage[this.vehicleNumber].Coverages.Coverage[i].SelectedLimit.Value);
            if ((<FormArray>this.coveragesForm.controls["CoveragesArray"]).at(i).get("SelectedLimit") != null) {
                if ((<FormArray>this.coveragesForm.controls["CoveragesArray"]).at(i).get("SelectedLimit").get("Value") != null) {
                    this.sessionObject.VehicleCoverages.VehicleCoverage[this.vehicleNumber].Coverages.Coverage[i].SelectedLimit.Value = (<FormArray>this.coveragesForm.controls["CoveragesArray"]).at(i).get("SelectedLimit").get("Value").value;
                }
                console.log("after: this.sessionObject.VehicleCoverages.VehicleCoverage[this.vehicleNumber].Coverages.Coverage[i].SelectedLimit.Value=" + this.sessionObject.VehicleCoverages.VehicleCoverage[this.vehicleNumber].Coverages.Coverage[i].SelectedLimit.Value);
            }
        }
        return this.recalculate()
            .then(() => {
                console.log("saveVehicleCoverages check recalculate errors!");
                this.coverageErrors = new Array();
                if (this.sessionObject.VehicleCoverageErrors != null && this.sessionObject.VehicleCoverageErrors.CoverageError != null) {
                    console.log("this.sessionObject.VehicleCoverageErrors.CoverageError.length=" + this.sessionObject.VehicleCoverageErrors.CoverageError.length);
                    for (var k = 0; k < this.sessionObject.VehicleCoverageErrors.CoverageError.length; k++) {
                        console.log("cov error: " + this.sessionObject.VehicleCoverageErrors.CoverageError[k].Message);
                        this.coverageErrors.push(this.sessionObject.VehicleCoverageErrors.CoverageError[k].Message);
                    }
                }
                if (this.sessionObject.AddInfo.ErrorMessages != null) {
                    for (var k = 0; k < this.sessionObject.AddInfo.ErrorMessages.length; k++) {
                        console.log("Error: " + this.sessionObject.AddInfo.ErrorMessages[k].Error);
                        this.coverageErrors.push(this.sessionObject.AddInfo.ErrorMessages[k].Error);
                    }
                }
                this.RecalculateComplete = true;
            });
    }
    savePolicyCoverages(): Promise<void> {
        for (var i = 0; i < this.sessionObject.PolicyCoverages.Coverage.length; i++) {
            if ((<FormArray>this.coveragesForm.controls["CoveragesArray"]).at(i).get("SelectedLimit") != null) {
                if ((<FormArray>this.coveragesForm.controls["CoveragesArray"]).at(i).get("SelectedLimit").get("Value") != null) {
                    this.sessionObject.PolicyCoverages.Coverage[i].SelectedLimit.Value = (<FormArray>this.coveragesForm.controls["CoveragesArray"]).at(i).get("SelectedLimit").get("Value").value;
                }
            }
        }
        
        return this.recalculate() 
            .then(() => {
                console.log("savePolicyCoverages check recalculate errors!");
                this.coverageErrors = new Array();
                if (this.sessionObject.PolicyCoverageErrors != null && this.sessionObject.PolicyCoverageErrors.CoverageError != null) {
                    //console.log("this.sessionObject.PolicyCoverageErrors.CoverageError.length=" + this.sessionObject.PolicyCoverageErrors.CoverageError.length);
                    for (var k = 0; k < this.sessionObject.PolicyCoverageErrors.CoverageError.length; k++) {
                        //console.log("cov error: " + this.sessionObject.PolicyCoverageErrors.CoverageError[k].Message);
                        this.coverageErrors.push(this.sessionObject.PolicyCoverageErrors.CoverageError[k].Message);
                    }
                }
                if (this.sessionObject.AddInfo.ErrorMessages != null) {
                    for (var k = 0; k < this.sessionObject.AddInfo.ErrorMessages.length; k++) {
                        //console.log("Error: " + this.sessionObject.AddInfo.ErrorMessages[k].Error);
                        this.coverageErrors.push(this.sessionObject.AddInfo.ErrorMessages[k].Error);
                    }     
                }
                this.RecalculateComplete = true;
            });
    }
    recalculate(): Promise<void> {
        console.log("coveragees recalculate!");
        return this.sessionService.recalculate(this.sessionObject)
            .then(response => {
                this.sessionObject = response.WebSessionDRC;
                console.log("recalculate got response!");
                console.log("this.sessionObject=" + JSON.stringify(this.sessionObject));
            },
            error => {
                console.log("recalculate " + error);
                alert("recalculate " + error);
                this.coverageErrors = error;
            });
    }
    //loadCoveragesAndDiscounts(): Promise<void> {
    //    console.log("top of loadCoveragesAndDiscounts ManageCovs");

    //    return this.sessionService.loadCoveragesAndDiscounts(this.sessionObject)
    //        .then(data => {
    //            this.sessionObject = data.WebSessionDRC;
    //            console.log("end of loadCoveragesAndDiscounts");
    //        },
    //        error => alert(error));
    //}
    //loadSession(): Promise<void> {
    //    return this.sessionService.loadSession(this.guid, this.zipcode, this.ctid)
    //        .then(data => {
    //            this.sessionObject = data.WebSessionDRC;
    //        },
    //        error => alert(error));
    //}
    
}
