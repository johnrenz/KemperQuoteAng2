import { Component, OnInit, Input } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { WebSessionDRC } from './WebSessionDRC/WebSessionDRC';
import { MatTooltipModule } from '@angular/material';

@Component({
  selector: 'app-header',
  template: `
 
<div class="navbar navbar-default navbar-fixed-top" >
      <div class="container">
        <div class="navbar-header">
          <a class="navbar-brand" href="http://wwwint.kemperi.com/wps/portal/KemperDirect/Home" target="_blank" >
                <img src="/assets/images/hdr_logoNew.gif" alt="Kemper Direct"/>
          </a>
          <button class="navbar-toggle" type="button" data-toggle="collapse" data-target="#navbar-main">
            <span class="icon-bar"></span>
            <span class="icon-bar"></span>
            <span class="icon-bar"></span>
          </button>
        </div>
        <div class="navbar-collapse collapse" id="navbar-main" >
          <ul class="nav navbar-nav">            
            <li>
                <a [routerLink]="['/applicant', this.sessionObject.Guid, this.zipcode, this.ctid]" >Applicant</a>
            </li>               
            <li>
                <a [routerLink]="['/vehicles', this.sessionObject.Guid, this.zipcode, this.ctid]" >Vehicles</a>
            </li>             
            <li>
                <a [routerLink]="['/drivers', this.sessionObject.Guid, this.zipcode, this.ctid]" >Drivers</a>
            </li>           
            <li>
              <a [routerLink]="['/policyInfo', this.sessionObject.Guid, this.zipcode, this.ctid]">Policy Information</a>
            </li>     
            <li>
                <a *ngIf="!AllComplete" matTooltip="{{Tooltip}}" [routerLink]="['/coverages', this.guid, this.zipcode, this.ctid]" class="disabled" >Coverages</a>
                <a *ngIf="AllComplete" [routerLink]="['/coverages', this.guid, this.zipcode, this.ctid]" >Coverages</a>
            </li>
        </ul>
        </div>
      </div>
    </div>

  `,
  styleUrls: []
})
export class HeaderComponent implements OnInit {
    @Input() sessionObject: WebSessionDRC;
    sub: any;

     guid: string;
     zipcode: string;
    ctid: string;

    ApplicantComplete: string;
    DriversComplete: string;
    VehiclesComplete: string;
    PolicyInfoComplete: string;
    AllComplete: boolean;
    Tooltip = "";

    constructor(private route: ActivatedRoute) { }

    ngOnInit() {
        this.sub = this.route.params.subscribe(params => {
            this.guid = params['guid'];
            this.zipcode = params['zipcode'];
            this.ctid = params['ctid'];
        });
        console.log("header this.guid=" + this.guid);
        console.log("header this.zipcode=" + this.zipcode);
        console.log("header this.ctid=" + this.ctid);
        console.log("header this.sessionObject.Guid=" + this.sessionObject.Guid);
        console.log("header this.sessionObject.AddInfo.ApplicantComplete=" + this.sessionObject.AddInfo.ApplicantComplete);
        console.log("this.sessionObject.Quote.Customer.IpFirstNameOfCustomer=" + this.sessionObject.Quote.Customer.IpFirstNameOfCustomer);
        console.log("this.sessionObject.ApplicantComplete=" + this.sessionObject.AddInfo.ApplicantComplete);
        console.log("this.sessionObject.VehiclesComplete=" + this.sessionObject.AddInfo.VehiclesComplete);
        console.log("this.sessionObject.DriversComplete=" + this.sessionObject.AddInfo.DriversComplete);
        console.log("this.sessionObject.PolicyInfoComplete=" + this.sessionObject.AddInfo.PolicyInfoComplete);
        this.ApplicantComplete = this.sessionObject.AddInfo.ApplicantComplete;
        this.DriversComplete = this.sessionObject.AddInfo.DriversComplete;
        this.VehiclesComplete = this.sessionObject.AddInfo.VehiclesComplete;
        this.PolicyInfoComplete = this.sessionObject.AddInfo.PolicyInfoComplete;
        this.AllComplete = this.PolicyInfoComplete == "true" && this.DriversComplete == "true" && this.VehiclesComplete == "true" && this.ApplicantComplete == "true";
        console.log("this.AllComplete=" + this.AllComplete);
        if (this.ApplicantComplete != "true")
            this.Tooltip = "Please complete Applicant before proceeding to Coverages!";
        else if (this.VehiclesComplete != "true")
            this.Tooltip = "Please complete Vehicles before proceeding to Coverages!";
        else if (this.DriversComplete != "true")
            this.Tooltip = "Please complete Drivers before proceeding to Coverages!";
        else if (this.PolicyInfoComplete != "true")
            this.Tooltip = "Please complete Policy Information before proceeding to Coverages!";
  }

}