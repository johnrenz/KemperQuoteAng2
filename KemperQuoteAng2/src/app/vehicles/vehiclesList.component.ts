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
import { ManageVehicle } from './ManageVehicle.component';
import { VehicleItem } from './VehicleItem';

@Component({
  selector: 'app-vehicles',
  templateUrl: './VehiclesList.component.html',
  providers: [SessionService],
  animations: [
      trigger('visibilityChanged', [
          state('true', style({ opacity: 1, height: 'auto', display: 'block' })),
          state('false', style({ opacity: 0, height: '0px', 'margin-bottom': '0px', display: 'none' })),
          transition('* => *', animate('1s'))
      ])
  ]
})
export class VehiclesListComponent implements OnInit {

    constructor(private sessionService: SessionService,
        private route: ActivatedRoute,
        private router: Router,
        private dialog: MatDialog) {

        this.minVehicles = false;
        this.maxVehicles = false;
        this.oneVehicle = false;
    }

    msg: string;
    dbops: DBOperation;
    modalTitle: string;
    modalBtnTitle: string;

    vehicles: VehicleItem[];
    vehicle: VehicleItem;

    oneVehicle: boolean;
    minVehicles: boolean;
    maxVehicles: boolean;
    private sub: any;
    guid: string;
    zipcode: string;
    ctid: string;
    sessionObject: WebSessionDRC; //this is the data model!!
    
    ngOnInit() {
        this.sub = this.route.params.subscribe(params => {
            this.guid = params['guid'];
            this.zipcode = params['zipcode'];
            this.ctid = params['ctid'];
        });
        this.loadSession()
            .then(() =>
                console.log("vehiclesList.ngOnInit this.sessionObject.Quote.Customer.IpFirstNameOfCustomer=" + this.sessionObject.Quote.Customer.IpFirstNameOfCustomer)
        );
    }
    loadSession(): Promise<void> {
        return this.sessionService.loadSession(this.guid, this.zipcode, this.ctid)
            .then(data => {
                this.sessionObject = data.WebSessionDRC;
                if (this.sessionObject.Quote.Vehicles.Vehicle == null)
                    this.sessionObject.Quote.Vehicles.Vehicle = [];
                this.vehicles = new Array();
                for (var i: number = 0; i < this.sessionObject.Quote.Vehicles.Vehicle.length;i++)
                {
                    var newVehicle: VehicleItem = new VehicleItem();
                    newVehicle.id = i;
                    newVehicle.Year = this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehYear;
                    newVehicle.Make = this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehMake;
                    
                    newVehicle.MakeNumber = this.sessionObject.Quote.Vehicles.Vehicle[i].IpMakeNumber.padStart(4, '0');
                    newVehicle.Model = this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehModel;
                    newVehicle.VehBodyStyle = this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehBodyStyle;
                    newVehicle.VehicleTrim = this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehBodyStyle;
                    newVehicle.vin = this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehVinNumber;
                    newVehicle.primaryUse = this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehUse;
                    newVehicle.commuteZip = this.sessionObject.Quote.Vehicles.Vehicle[i].IpAltGarZipCode1;
                    newVehicle.airBag = this.sessionObject.Quote.Vehicles.Vehicle[i].IpAirBagTest;
                    newVehicle.seatBelt = this.sessionObject.Quote.Vehicles.Vehicle[i].IpPassiveRestraint;
                    newVehicle.antiTheft = this.sessionObject.Quote.Vehicles.Vehicle[i].IpDisablingDevice;
                    newVehicle.antiLockBrakes = this.sessionObject.Quote.Vehicles.Vehicle[i].IpAntiLockBrakeTest;
                    newVehicle.daytimeRunningLights = this.sessionObject.Quote.Vehicles.Vehicle[i].IpDaytimeLightsTest;
                    newVehicle.AnnualMileage = this.sessionObject.Quote.Vehicles.Vehicle[i].IpAnnualMileage;
                    newVehicle.VehWebModel = this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehWebModel == null ? '' : this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehWebModel;
                    newVehicle.ModelNumber = this.sessionObject.Quote.Vehicles.Vehicle[i].IpModelNumber;
                    newVehicle.YearMakeModel = this.sessionObject.Quote.Vehicles.Vehicle[i].IpYearMakeModel;
                    newVehicle.VehSymbol = this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehSymbol;
                    newVehicle.VehHiPerfInd = this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehHiPerfInd;
                    newVehicle.AdjustToMakeModel = this.sessionObject.Quote.Vehicles.Vehicle[i].IpAdjustToMakeModel;
                    newVehicle.VehExposure = this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehExposure;
                    newVehicle.SafeVeh = this.sessionObject.Quote.Vehicles.Vehicle[i].IpSafeVeh;
                    newVehicle.Performance = this.sessionObject.Quote.Vehicles.Vehicle[i].IpPerformance;
                    newVehicle.VehSymbolLiab = this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehSymbolLiab;
                    newVehicle.VehSymbolIsoColl = this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehSymbolIsoColl;
                    newVehicle.VehSymbolIsoComp = this.sessionObject.Quote.Vehicles.Vehicle[0].IpVehSymbolIsoComp;
                    newVehicle.VehSymbolColl = this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehSymbolColl;
                    newVehicle.VehSymbolComp = this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehSymbolComp;
                    newVehicle.VehSymbolPip = this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehSymbolPip;
                    newVehicle.VehType = this.sessionObject.Quote.Vehicles.Vehicle[i].IpVehType;
                    this.vehicles.push(newVehicle);
                    
                }
                if (this.vehicles.length > 0)
                    this.minVehicles = true;
                else
                    this.minVehicles = false;
                if (this.vehicles.length > 3)
                    this.maxVehicles = true;
                else
                    this.maxVehicles = false;
                if (this.vehicles.length == 1)
                    this.oneVehicle = true;
                else
                    this.oneVehicle = false;
            },
            error => alert(error));
    }
    openDialog() {
        let dialogRef = this.dialog.open(ManageVehicle);
        dialogRef.componentInstance.dbops = this.dbops;
        dialogRef.componentInstance.modalTitle = this.modalTitle;
        dialogRef.componentInstance.modalBtnTitle = this.modalBtnTitle;
        dialogRef.componentInstance.vehicle = this.vehicle;
        dialogRef.componentInstance.sessionObject = this.sessionObject;
        dialogRef.updateSize('75%', '500px');
        dialogRef.updatePosition({ top: '100px'});
        
        dialogRef.afterClosed().subscribe(result => {
            if (result == "success") {
                this.loadSession();
                console.log("vehiclesList.afterClosed loadsess this.sessionObject.Quote.Customer.IpFirstNameOfCustomer=" + this.sessionObject.Quote.Customer.IpFirstNameOfCustomer);
                switch (this.dbops) {
                    case DBOperation.create:
                        this.msg = "Vehicle successfully added.";
                        break;
                    case DBOperation.update:
                        this.msg = "Vehicle successfully updated.";
                        break;
                    case DBOperation.delete:
                        this.msg = "Vehicle successfully deleted.";
                        break;
                }
            }
            else if (result == "error")
                this.msg = "There is some issue in saving records, please contact to system administrator!"
            else
                this.msg = result;
        });
    }

    addVehicle() {
        this.dbops = DBOperation.create;
        this.modalTitle = "Add New Vehicle";
        this.modalBtnTitle = "Add";
        this.openDialog();
    }
    editVehicle(id: number) {
        this.dbops = DBOperation.update;
        this.modalTitle = "Edit Vehicle";
        this.modalBtnTitle = "Update";
        this.vehicle = this.vehicles.filter(x => x.id == id)[0];
        this.openDialog();
    }
    deleteVehicle(id: number) {
        this.dbops = DBOperation.delete;
        this.modalTitle = "Confirm to Delete?";
        this.modalBtnTitle = "Delete";
        this.vehicle = this.vehicles.filter(x => x.id == id)[0];
        this.openDialog();
    }

    onContinue() {
        
        this.router.navigate(['/drivers', this.guid, this.zipcode, this.ctid]);
    }
}
