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

@Component({
  selector: 'app-vehicles',
  templateUrl: './vehicles.component.html',
  styleUrls: ['./vehicles.component.css'],
  providers: [SessionService],
  animations: [
      trigger('visibilityChanged', [
          state('true', style({ opacity: 1, height: 'auto', display: 'block' })),
          state('false', style({ opacity: 0, height: '0px', 'margin-bottom': '0px', display: 'none' })),
          transition('* => *', animate('1s'))
      ])
  ]
})
export class VehiclesComponent implements OnInit {

    constructor(private fb: FormBuilder,
        private sessionService: SessionService,
        private route: ActivatedRoute,
        private router: Router) {

        this.createForm()
    }


    vehiclesForm: FormGroup;
    private sub: any;
    guid: string;
    zipcode: string;
    ctid: string;
    sessionObject: WebSessionDRC; //this is the data model!!

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
        this.sub = this.route.params.subscribe(params => {
            this.guid = params['guid'];
            this.zipcode = params['zipcode'];
            this.ctid = params['ctid'];
        });
        this.loadSession()
            .then(() => {
                    //alert("IpVehUse: " + this.sessionObject.Quote.Vehicles.Vehicle[0].IpAntiLockBrakeTest);
                    this.vehiclesForm.setValue({
                        Vehicle1: {
                            Year: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehYear,
                            Make: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehMake,
                            MakeNumber: this.sessionObject.Quote.Vehicles.Vehicle[0].IpMakeNumber.padStart(4,'0'),
                            Model: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehModel,
                            BodyStyle: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehBodyStyle,
                            VehicleTrim: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehBodyStyle,
                            vin: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehVinNumber,
                            primaryUse: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehUse,
                            commuteZip: this.sessionObject.Quote.Vehicles.Vehicle[0].IpAltGarZipCode1,
                            airBag: this.sessionObject.Quote.Vehicles.Vehicle[0].IpAirBagTest,
                            seatBelt: this.sessionObject.Quote.Vehicles.Vehicle[0].IpPassiveRestraint,
                            antiTheft: this.sessionObject.Quote.Vehicles.Vehicle[0].IpDisablingDevice,
                            antiLockBrakes: this.sessionObject.Quote.Vehicles.Vehicle[0].IpAntiLockBrakeTest,
                            homingDevice: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehicleRecoveryTest,
                            daytimeRunningLights: this.sessionObject.Quote.Vehicles.Vehicle[0].IpDaytimeLightsTest,
                            windowEtching: this.sessionObject.Quote.Vehicles.Vehicle[0].IpWindowEtchingTest,
                            AnnualMileage: this.sessionObject.Quote.Vehicles.Vehicle[0].IpAnnualMileage,
                            VehWebModel: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehWebModel,
                            ModelNumber: this.sessionObject.Quote.Vehicles.Vehicle[0].IpModelNumber,
                            YearMakeModel: this.sessionObject.Quote.Vehicles.Vehicle[0].IpYearMakeModel,
                            VehSymbol: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehSymbol,
                            VehHiPerfInd: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehHiPerfInd,
                            AdjustToMakeModel: this.sessionObject.Quote.Vehicles.Vehicle[0].IpAdjustToMakeModel,
                            VehExposure: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehExposure,
                            SafeVeh: this.sessionObject.Quote.Vehicles.Vehicle[0].IpSafeVeh,
                            Performance: this.sessionObject.Quote.Vehicles.Vehicle[0].IpPerformance,
                            VehSymbolLiab: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehSymbolLiab,
                            VehSymbolIsoColl: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehSymbolIsoColl,
                            VehSymbolIsoComp: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehSymbolIsoComp,
                            VehSymbolColl: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehSymbolColl,
                            VehSymbolComp: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehSymbolComp,
                            VehSymbolPip: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehSymbolPip,
                            VehType: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehType
                        }
                });
                this.vehicle1MakeOptions.push({
                    Value: this.sessionObject.Quote.Vehicles.Vehicle[0].IpMakeNumber.padStart(4, '0'),
                    Description: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehMake
                });
                this.selectedVehicle1MakeOption = this.sessionObject.Quote.Vehicles.Vehicle[0].IpMakeNumber.padStart(4, '0');
                this.vehicle1ModelOptions.push({
                    Value: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehModel,
                    Description: this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehModel
                });
                this.selectedVehicle1ModelOption = this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehModel;
            });
    }
    loadSession(): Promise<void> {
        return this.sessionService.loadSession(this.guid, this.zipcode, this.ctid)
            .then(data => {
                this.sessionObject = data.WebSessionDRC;
            },
            error => alert(error));
    }

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
        //alert("getVehicleMakes() year: " + year.value + ", makeNo: " + makeNo);
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
                },
                error => alert(error));
    }
    getVehicle1YearMakeWebModel(vin) {
        //alert('vin.value: ' + vin.value);
        if (vin != "")
            return this.sessionService.getYearMakeWebModel(vin.value)
                .then(data => {
                    //alert('year: ' + data.Year + ',MakeNo: ' + data.MakeNo + ',model: ' + data.Model);
                    this.Vehicle1Year.setValue(data.Year);
                    this.getVehicle1Makes(data.Year);
                    this.Vehicle1Make.setValue(data.MakeNo);
                    this.getWebVehicle1Models(data.Year, data.MakeNo);
                    this.Vehicle1MakeNumber.setValue(data.MakeNo);
                    this.Vehicle1Model.setValue(data.Model);

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
                },
                error => alert(error));
    }
}
