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
import { Http } from '@angular/http';
import 'hammerjs';
import { ManageDriver } from './ManageDriver.component';
import { DriverItem } from './DriverItem';

@Component({
    selector: 'app-drivers',
    templateUrl: './drivers.component.html',
    providers: [SessionService],
    animations: [
        trigger('visibilityChanged', [
            state('true', style({ opacity: 1, height: 'auto', display: 'block' })),
            state('false', style({ opacity: 0, height: '0px', 'margin-bottom': '0px', display: 'none' })),
            transition('* => *', animate('1s'))
        ])
    ]
})
export class DriversComponent implements OnInit {

    constructor(private sessionService: SessionService,
        private route: ActivatedRoute,
        private router: Router,
        private dialog: MatDialog) {

        this.minDrivers = false;
        this.maxDrivers = false;
        this.oneDriver = false;
    }

    msg: string;
    dbops: DBOperation;
    modalTitle: string;
    modalBtnTitle: string;

    drivers: DriverItem[];
    driver: DriverItem;

    oneDriver: boolean;
    minDrivers: boolean;
    maxDrivers: boolean;
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
        this.loadSession();
    }
    loadSession(): Promise<void> {
        return this.sessionService.loadSession(this.guid, this.zipcode, this.ctid)
            .then(data => {
                this.sessionObject = data.WebSessionDRC;
                if (this.sessionObject.Quote.Drivers.Driver == null)
                    this.sessionObject.Quote.Drivers.Driver = [];
                this.drivers = new Array();
                for (var i: number = 0; i < this.sessionObject.Quote.Drivers.Driver.length; i++) {
                    var newDriver: DriverItem = new DriverItem();
                    newDriver.id = i;
                    newDriver.IpDrivFirst = this.sessionObject.Quote.Drivers.Driver[i].IpDrivFirst;
                    newDriver.IpDrivLast = this.sessionObject.Quote.Drivers.Driver[i].IpDrivLast;
                    newDriver.IpBirthDateOfDriv = this.sessionObject.Quote.Drivers.Driver[i].IpBirthDateOfDriv;
                    newDriver.IpDateLicensed = this.sessionObject.Quote.Drivers.Driver[i].IpDateLicensed;
                    newDriver.IpDriverStatus = this.sessionObject.Quote.Drivers.Driver[i].IpDriverStatus;
                    newDriver.IpDrivMarriedSingle = this.sessionObject.Quote.Drivers.Driver[i].IpDrivMarriedSingle;
                    newDriver.IpDrivLicStateNo = this.sessionObject.Quote.Drivers.Driver[i].IpDrivLicStateNo;
                    newDriver.IpDrivSex = this.sessionObject.Quote.Drivers.Driver[i].IpDrivSex;
                    newDriver.IpEducationTest = this.sessionObject.Quote.Drivers.Driver[i].IpEducationTest;
                    newDriver.IpGoodStudentDis = this.sessionObject.Quote.Drivers.Driver[i].IpGoodStudentDis;
                    newDriver.IpMatureDriverDis = this.sessionObject.Quote.Drivers.Driver[i].IpMatureDriverDis;
                    newDriver.IpOccupationCat = this.sessionObject.Quote.Drivers.Driver[i].IpOccupationCat;
                    newDriver.IpOperatorType = this.sessionObject.Quote.Drivers.Driver[i].IpOperatorType;
                    newDriver.IpOrigDriverNo = this.sessionObject.Quote.Drivers.Driver[i].IpOrigDriverNo;
                    newDriver.IpRelationToResp = this.sessionObject.Quote.Drivers.Driver[i].IpRelationToResp;
                    newDriver.IpVehOfDriv = this.sessionObject.Quote.Drivers.Driver[i].IpVehOfDriv;
                    
                    this.drivers.push(newDriver);

                }
                if (this.drivers.length > 0)
                    this.minDrivers = true;
                else
                    this.minDrivers = false;
                if (this.drivers.length > 5)
                    this.maxDrivers = true;
                else
                    this.maxDrivers = false;
                if (this.drivers.length == 1)
                    this.oneDriver = true;
                else
                    this.oneDriver = false;
            },
            error => alert(error));
    }
    openDialog() {
        let dialogRef = this.dialog.open(ManageDriver);
        dialogRef.componentInstance.dbops = this.dbops;
        dialogRef.componentInstance.modalTitle = this.modalTitle;
        dialogRef.componentInstance.modalBtnTitle = this.modalBtnTitle;
        dialogRef.componentInstance.driver = this.driver;
        dialogRef.componentInstance.sessionObject = this.sessionObject;
        dialogRef.updateSize('75%', '650px');
        dialogRef.updatePosition({ top: '100px' });

        dialogRef.afterClosed().subscribe(result => {
            if (result == "success") {
                this.loadSession();
                console.log("drivers.afterClosed loadsess this.sessionObject.Quote.Customer.IpFirstNameOfCustomer=" + this.sessionObject.Quote.Customer.IpFirstNameOfCustomer);
                switch (this.dbops) {
                    case DBOperation.create:
                        this.msg = "Driver successfully added.";
                        break;
                    case DBOperation.update:
                        this.msg = "Driver successfully updated.";
                        break;
                    case DBOperation.delete:
                        this.msg = "Driver successfully deleted.";
                        break;
                }
            }
            else if (result == "error")
                this.msg = "There is some issue in saving records, please contact to system administrator!"
            else
                this.msg = result;
        });
    }

    addDriver() {
        this.dbops = DBOperation.create;
        this.modalTitle = "Add New Driver";
        this.modalBtnTitle = "Add";
        this.openDialog();
    }
    editDriver(id: number) {
        this.dbops = DBOperation.update;
        this.modalTitle = "Edit Driver";
        this.modalBtnTitle = "Update";
        this.driver = this.drivers.filter(x => x.id == id)[0];
        this.openDialog();
    }
    deleteDriver(id: number) {
        this.dbops = DBOperation.delete;
        this.modalTitle = "Confirm to Delete?";
        this.modalBtnTitle = "Delete";
        this.driver = this.drivers.filter(x => x.id == id)[0];
        this.openDialog();
    }

    onContinue() {

        this.router.navigate(['/policyInfo', this.guid, this.zipcode, this.ctid]);
    }
}
