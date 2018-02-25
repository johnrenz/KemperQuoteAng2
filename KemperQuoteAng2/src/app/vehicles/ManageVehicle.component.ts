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
import { Vehicle } from '../WebSessionDRC/Vehicle';
import { SessionService } from '../session.service';
import { Utilities } from '../Utilities';
import { YesNoOptions } from './YesNoOptions';
import { vehicleYearOptions } from './vehicleYearOptions';
import { primaryUseOptions } from './primaryUseOptions';
import { airbagOptions } from './airbagOptions';
import { antiTheftOptions } from './antiTheftOptions';
import { antiLockBrakesOptions } from './antiLockBrakesOptions';
import { daytimeRunningLightsOptions } from './daytimeRunningLightsOptions';
import { seatBeltOptions } from './seatBeltOptions';
import { Http } from '@angular/http';
import 'hammerjs';
import { VehicleItem } from './VehicleItem';

@Component({
    templateUrl: './ManageVehicle.component.html',
  providers: [SessionService],
  animations: [
      trigger('visibilityChanged', [
          state('true', style({ opacity: 1, height: 'auto', display: 'block' })),
          state('false', style({ opacity: 0, height: '0px', 'margin-bottom': '0px', display: 'none' })),
          transition('* => *', animate('1s'))
      ])
  ]
})
export class ManageVehicle implements OnInit {

    constructor(private fb: FormBuilder,
        private sessionService: SessionService,
        private route: ActivatedRoute,
        private router: Router,
        public dialogRef: MatDialogRef<ManageVehicle>) {

        this.createForm()
    }

    dbops: DBOperation;
    modalTitle: string;
    modalBtnTitle: string;

    vehiclesForm: FormGroup;
    private sub: any;
    guid: string;
    zipcode: string;
    ctid: string;
    sessionObject: WebSessionDRC; //this is the data model!!
    vehicle: VehicleItem;

    YesNoOptions = YesNoOptions;
    vehicleYearOptions = vehicleYearOptions;
    primaryUseOptions = primaryUseOptions;
    airbagOptions = airbagOptions;
    antiTheftOptions = antiTheftOptions;
    antiLockBrakesOptions = antiLockBrakesOptions;
    daytimeRunningLightsOptions = daytimeRunningLightsOptions;
    seatBeltOptions = seatBeltOptions;

    vehicle1MakeOptions = [{ Value: "0000", Description: "Please Select" }];
    vehicle1ModelOptions = [{ Value: "Please Select", Description: "Please Select" }];
    selectedVehicle1MakeOption: string;
    selectedVehicle1ModelOption: string;    

    get Vehicle1() { return this.vehiclesForm.get('Vehicle1'); };
    get Vehicle1Year() { return this.vehiclesForm.get('Vehicle1.Year'); };
    get Vehicle1Make() { return this.vehiclesForm.get('Vehicle1.Make'); };
    get Vehicle1MakeNumber() { return this.vehiclesForm.get('Vehicle1.MakeNumber'); };
    get Vehicle1Model() { return this.vehiclesForm.get('Vehicle1.Model'); };
    get Vehicle1Vin() { return this.vehiclesForm.get('Vehicle1.vin'); };
    get Vehicle1PrimaryUse() { return this.vehiclesForm.get('Vehicle1.primaryUse'); };
    get Vehicle1commuteZip() { return this.vehiclesForm.get('Vehicle1.commuteZip'); };
    get Vehicle1airBag() { return this.vehiclesForm.get('Vehicle1.airBag'); };
    get Vehicle1seatBelt() { return this.vehiclesForm.get('Vehicle1.seatBelt'); };
    get Vehicle1antiTheft() { return this.vehiclesForm.get('Vehicle1.antiTheft'); };
    get Vehicle1antiLockBrakes() { return this.vehiclesForm.get('Vehicle1.antiLockBrakes'); };
    get Vehicle1daytimeRunningLights() { return this.vehiclesForm.get('Vehicle1.daytimeRunningLights'); };
    get Vehicle1VehWebModel() { return this.vehiclesForm.get('Vehicle1.VehWebModel'); };
    get Vehicle1ModelNumber() { return this.vehiclesForm.get('Vehicle1.ModelNumber'); };
    get Vehicle1YearMakeModel() { return this.vehiclesForm.get('Vehicle1.YearMakeModel'); };
    get Vehicle1VehSymbol() { return this.vehiclesForm.get('Vehicle1.VehSymbol'); };
    get Vehicle1VehHiPerfInd() { return this.vehiclesForm.get('Vehicle1.VehHiPerfInd'); }; 
    get Vehicle1VehBodyStyle() { return this.vehiclesForm.get('Vehicle1.VehBodyStyle'); };
    get Vehicle1Trim() { return this.vehiclesForm.get('Vehicle1.VehicleTrim'); };
    get Vehicle1AdjustToMakeModel() { return this.vehiclesForm.get('Vehicle1.AdjustToMakeModel'); };
    get Vehicle1VehExposure() { return this.vehiclesForm.get('Vehicle1.VehExposure'); };
    get Vehicle1SafeVeh() { return this.vehiclesForm.get('Vehicle1.SafeVeh'); };
    get Vehicle1Performance() { return this.vehiclesForm.get('Vehicle1.Performance'); };
    get Vehicle1VehSymbolLiab() { return this.vehiclesForm.get('Vehicle1.VehSymbolLiab'); };
    get Vehicle1VehSymbolIsoColl() { return this.vehiclesForm.get('Vehicle1.VehSymbolIsoColl'); };
    get Vehicle1VehSymbolIsoComp() { return this.vehiclesForm.get('Vehicle1.VehSymbolIsoComp'); };
    get Vehicle1VehSymbolColl() { return this.vehiclesForm.get('Vehicle1.VehSymbolColl'); };
    get Vehicle1VehSymbolComp() { return this.vehiclesForm.get('Vehicle1.VehSymbolComp'); };
    get Vehicle1VehSymbolPip() { return this.vehiclesForm.get('Vehicle1.VehSymbolPip'); };
    get Vehicle1VehType() { return this.vehiclesForm.get('Vehicle1.VehType'); };

    createForm() {
        this.vehiclesForm = this.fb.group({
            Vehicle1: this.fb.group({
                id: [''],
                Year: ['', Validators.required],
                Make: ['', Validators.required],
                MakeNumber: [''],
                Model: ['', Validators.required],
                VehBodyStyle: ['', Validators.required], //vibe
                VehicleTrim: [''], //nonvibe
                vin: ['', Validators.required],
                primaryUse: ['', Validators.pattern('[1-9]')],
                commuteZip: [''],
                airBag: [''],
                seatBelt: [''],
                antiTheft: [''],
                antiLockBrakes: [''],
                daytimeRunningLights: [''],
                AnnualMileage: [''],
                VehWebModel: [''],
                ModelNumber: [''],
                YearMakeModel: [''],
                VehSymbol: [''],
                VehHiPerfInd: [''],
                AdjustToMakeModel: [''],
                VehExposure: [''],
                SafeVeh: [''],
                Performance: [''],
                VehSymbolLiab: [''],
                VehSymbolIsoColl: [''],
                VehSymbolIsoComp: [''],
                VehSymbolColl: [''],
                VehSymbolComp: [''],
                VehSymbolPip: [''],
                VehType: ['']
            })
        });
    }
    ngOnInit() {
        console.log("Manageveh.ngOnInit this.sessionObject.Quote.Customer.IpFirstNameOfCustomer=" + this.sessionObject.Quote.Customer.IpFirstNameOfCustomer);
        this.vehiclesForm.valueChanges.subscribe(data => this.onValueChanged(data));
        this.onValueChanged();
        if (this.dbops == DBOperation.create)
            this.vehiclesForm.reset();
        else
        {
            this.vehiclesForm.setValue({
                Vehicle1: this.vehicle               
            });
            this.vehicle1MakeOptions.push({
                Value: this.vehicle.MakeNumber.padStart(4, '0'),
                Description: this.vehicle.Make
            });
            this.selectedVehicle1MakeOption = this.vehicle.MakeNumber.padStart(4, '0');
            this.vehicle1ModelOptions.push({
                Value: this.vehicle.Model,
                Description: this.vehicle.Model
            });
            this.selectedVehicle1ModelOption = this.vehicle.Model;      
        }
            
        this.SetControlsState(this.dbops == DBOperation.delete ? false : true);

              
    }
    onValueChanged(data?: any) {
        if (!this.vehiclesForm) { return; }
        const form = this.vehiclesForm;
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
        'Year': '',
        'Make': '',
        'MakeNumber': '',
        'Model': '',
        'VehBodyStyle': '', //vibe
        'VehicleTrim': '', //nonvibe
        'vin': '',
        'primaryUse': ''  
    };
    validationMessages = {
        'Year': {
            'required': ' required.'
        },
        'Make': {
            'required': ' required.'
        },
        'MakeNumber': {
            'required': ' required.'
        },
        'Model': {
            'required': ' required.'
        },
        'VehBodyStyle': {
            'required': ' required.'
        }, //vibe
        'VehicleTrim': {
            'required': ' required.'
        }, //nonvibe
        'vin': {
            'required': ' required.'
        },
        'primaryUse': {
            'required': ' required.'
        }
    }

    SetControlsState(isEnable: boolean) {
        isEnable ? this.vehiclesForm.enable() : this.vehiclesForm.disable();
    }

    //loadSession(): Promise<void> {
    //    return this.sessionService.loadSession(this.guid, this.zipcode, this.ctid)
    //        .then(data => {
    //            this.sessionObject = data.WebSessionDRC;
    //        },
    //        error => alert(error));
    //}

    getVehicle1Makes(year) {
        //alert("getVehicleMakes() year: " + year.value);
        //alert('first vehicleMakeOptions- ' + this.vehicleMakeOptions);
        return this.sessionService.getVehicleMakes(year)
            .then(data => {
                data.unshift({ Value: "0000", Description: "Please Select" });
                this.vehicle1MakeOptions = data;
            },
            error => alert(error));
    }
    getWebVehicle1Models(year, makeNo) {
        //alert("makeNo:" + makeNo);
        //alert("vehicle1MakeOptions[1].Value):" + this.vehicle1MakeOptions[1].Value);
       // for (var x: number = 0; x < this.vehicle1MakeOptions.length; x++)
        //   console.log("vehicle1MakeOptions[" + x + "].Value):" + this.vehicle1MakeOptions[x].Value);
        var option = this.vehicle1MakeOptions.filter(x => x.Value == makeNo)[0];
        if (option == null)
            console.log("makeNo " + makeNo + " not found in vehicle1MakeOptions, continue.");
        else {
            var make = this.vehicle1MakeOptions.filter(x => x.Value == makeNo)[0].Description;
            this.Vehicle1Make.setValue(make);
            console.log("Vehicle1Make set to " + make);
        }
        if (makeNo.value == "0000")
            this.vehicle1ModelOptions = [{ Value: "Please Select", Description: "Please Select" }];
        else
            return this.sessionService.getWebVehicleModels(year, makeNo)
                .then(data => {
                    data.unshift({ Value: "Please Select", Description: "Please Select" });
                    this.vehicle1ModelOptions = data;
                },
                error => alert(error));
    }
    getWebVehicle1Vin(year, makeNo, model) {
        //alert("getVehicleMakes() year: " + year.value + ", makeNo: " + makeNo + ", model: " + model);
        if (model.value == "Please Select")
            this.Vehicle1Vin.setValue('');
        else
            return this.sessionService.getWebVehicleVin(year, makeNo, model)
                .then(data => {
                    this.Vehicle1Vin.setValue(data.Vin);
                    this.Vehicle1VehBodyStyle.setValue(data.BodyStyle);
                    this.Vehicle1Trim.setValue(data.VehicleTrim);
                    this.Vehicle1Model.setValue(model);
                    this.Vehicle1ModelNumber.setValue(data.ModelNo);
                    this.Vehicle1VehType.setValue(data.VehType);
                    this.Vehicle1Performance.setValue(data.Performance);
                    this.Vehicle1VehSymbol.setValue(data.VehSymbol);
                    this.Vehicle1VehSymbolLiab.setValue(data.VehSymbolLiab);
                    this.Vehicle1VehSymbolComp.setValue(data.VehSymbolComp);
                    this.Vehicle1VehSymbolColl.setValue(data.VehSymbolColl);
                    this.Vehicle1VehSymbolPip.setValue(data.VehSymbolPip);
                    this.Vehicle1VehSymbolIsoColl.setValue(data.VehSymbolIsoComp);
                    this.Vehicle1VehSymbolIsoColl.setValue(data.VehSymbolIsoColl);
                    this.Vehicle1AdjustToMakeModel.setValue(data.AdjustToMakeModel);
                    this.Vehicle1SafeVeh.setValue(data.SafeVeh);
                    this.Vehicle1VehExposure.setValue(data.VehExposure);
                    this.Vehicle1YearMakeModel.setValue(data.YearMakeModel);
                    this.Vehicle1VehHiPerfInd.setValue(data.VehHiPerfInd);
                    this.Vehicle1VehWebModel.setValue(data.VehWebModel);
                    //vin lookup to get airBags, antilock, restraint, seatbelt
                    this.getVehicle1YearMakeWebModel(this.Vehicle1Vin);
                },
                error => alert(error));
    }
    getVehicle1YearMakeWebModel(vin) {
        console.log('vin.value: ' + vin.value);
        if (vin != "")
            return this.sessionService.getYearMakeWebModel(vin.value) //todo nonvibe call getYearMakeModel(vin)
                .then(data => {
                    //alert('year: ' + data.Year + ',MakeNo: ' + data.MakeNo + ',model: ' + data.Model);
                    //this.Vehicle1Year.setValue(data.Year);
                    this.getVehicle1Makes(data.Year);
                    //this.Vehicle1Make.setValue(data.Make);
                    //this.getWebVehicle1Models(data.Year, data.MakeNo);
                    //this.Vehicle1MakeNumber.setValue(data.MakeNo);
                    //this.Vehicle1Model.setValue(data.Model);
                    this.Vehicle1VehWebModel.setValue(data.VehWebModel);

                    this.Vehicle1VehBodyStyle.setValue(data.BodyStyle);
                    this.Vehicle1Trim.setValue(data.VehicleTrim);
                    this.Vehicle1ModelNumber.setValue(data.ModelNumber);
                    this.Vehicle1VehType.setValue(data.VehType);
                    this.Vehicle1Performance.setValue(data.Performance);
                    this.Vehicle1VehSymbol.setValue(data.VehSymbol);
                    this.Vehicle1VehSymbolLiab.setValue(data.VehSymbolLiab);
                    this.Vehicle1VehSymbolComp.setValue(data.VehSymbolComp);
                    this.Vehicle1VehSymbolColl.setValue(data.VehSymbolColl);
                    this.Vehicle1VehSymbolPip.setValue(data.VehSymbolPip);
                    this.Vehicle1VehSymbolIsoColl.setValue(data.VehSymbolIsoComp);
                    this.Vehicle1VehSymbolIsoColl.setValue(data.VehSymbolIsoColl);
                    this.Vehicle1AdjustToMakeModel.setValue(data.AdjustToMakeModel);
                    this.Vehicle1SafeVeh.setValue(data.SafeVeh);
                    this.Vehicle1VehExposure.setValue(data.VehExposure);
                    this.Vehicle1YearMakeModel.setValue(data.YearMakeModel);
                    this.Vehicle1VehHiPerfInd.setValue(data.VehHiPerfInd);
                    this.Vehicle1antiLockBrakes.setValue(data.AntiLockBrake);
                    this.Vehicle1antiTheft.setValue(data.AntiTheftInfo);
                    this.Vehicle1seatBelt.setValue(data.Restraint);
                    this.Vehicle1airBag.setValue(data.SafeVeh);
                },
                error => alert(error));
    }
    onSubmit(formData: any) {
        switch (this.dbops) {
            case DBOperation.create:
                var count: number = +this.sessionObject.Quote.Vehicles.IpVehicleCt;
                //alert("count: " + count);
                count++;
                //alert("count: " + count);
                this.sessionObject.Quote.Vehicles.IpVehicleCt = "" + count++;
                //alert("IpVehicleCt: " + this.sessionObject.Quote.Vehicles.IpVehicleCt);
                //alert("this.sessionObject.Quote.Vehicles.Vehicle.length: " + this.sessionObject.Quote.Vehicles.Vehicle.length);
                this.sessionObject.Quote.Vehicles.Vehicle.length++;
                //alert("this.sessionObject.Quote.Vehicles.Vehicle.length: " + this.sessionObject.Quote.Vehicles.Vehicle.length);
                this.sessionObject.Quote.Vehicles.Vehicle[count - 1] = new Vehicle();
                this.sessionObject.Quote.Vehicles.Vehicle[count - 1]['@SLICE'] = this.sessionObject.Quote.Vehicles.IpVehicleCt
                this.saveVehicle(count - 1);
                this.dialogRef.close("success");
                break;
            case DBOperation.update:
                this.saveVehicle(this.vehicle.id);
                this.dialogRef.close("success");
                break;
            case DBOperation.delete:
                this.deleteVehicle(this.vehicle.id);
                this.dialogRef.close("success");
                break;
        }
    }
    deleteVehicle(i: number) {
        this.sessionObject.Quote.Vehicles.Vehicle.splice(i, 1);
        //for (var j: number = 0; j < this.sessionObject.Quote.Vehicles.Vehicle.length; j++)
        //    alert("IpVehMake - " + this.sessionObject.Quote.Vehicles.Vehicle[j].IpVehMake);
        //alert("saving session after delete!");
        this.sessionService.saveSession(this.sessionObject)
            .then((result) => {
                console.log("deleteVehicle save result=" + result);
            });  
    }
    saveVehicle(i: number) {
        //TODO call vilookup(vin0 again to get airbag, etc..
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehYear = this.Vehicle1Year.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehMake = this.Vehicle1Make.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpMakeNumber = this.Vehicle1MakeNumber.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehModel = this.Vehicle1Model.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehWebModel = this.Vehicle1VehWebModel.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehBodyStyle = this.Vehicle1VehBodyStyle.value; //vibe
        //this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehBodyStyle = this.Vehicle1Trim.value; //nonvibe
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehVinNumber = this.Vehicle1Vin.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehUse = this.Vehicle1PrimaryUse.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpAltGarZipCode1 = this.Vehicle1commuteZip.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpAirBagTest = this.Vehicle1airBag.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpPassiveRestraint = this.Vehicle1seatBelt.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpDisablingDevice = this.Vehicle1antiTheft.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpAntiLockBrakeTest = this.Vehicle1antiLockBrakes.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpDaytimeLightsTest = this.Vehicle1daytimeRunningLights.value;
        //this.sessionObject.Quote.Vehicles.Vehicle[i].IpAnnualMileage = this.Vehicle1AnnualMileage.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehWebModel = this.Vehicle1VehWebModel.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpModelNumber = this.Vehicle1ModelNumber.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpYearMakeModel = this.Vehicle1YearMakeModel.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehSymbol = this.Vehicle1VehSymbol.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehHiPerfInd = this.Vehicle1VehHiPerfInd.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpAdjustToMakeModel = this.Vehicle1AdjustToMakeModel.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehExposure = this.Vehicle1VehExposure.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpSafeVeh = this.Vehicle1SafeVeh.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpPerformance = this.Vehicle1Performance.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehSymbolLiab = this.Vehicle1VehSymbolLiab.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehSymbolIsoColl = this.Vehicle1VehSymbolIsoColl.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehSymbolIsoComp = this.Vehicle1VehSymbolIsoComp.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehSymbolColl = this.Vehicle1VehSymbolColl.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehSymbolComp = this.Vehicle1VehSymbolComp.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehSymbolPip = this.Vehicle1VehSymbolPip.value;
        this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehType = this.Vehicle1VehType.value;

        this.sessionObject.AddInfo.VehiclesComplete = "true";
        this.sessionService.saveSession(this.sessionObject)
            .then((result) => {
                console.log("saveVehicle save result=" + result);
            });  
        console.log("ManageVeh endof saveveh this.sessionObject.Quote.Customer.IpFirstNameOfCustomer=" + this.sessionObject.Quote.Customer.IpFirstNameOfCustomer);

    }
}
