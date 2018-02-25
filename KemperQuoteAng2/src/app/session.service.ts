import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Headers, RequestOptions } from '@angular/http';
import { URLSearchParams } from "@angular/http"
import { Observable } from 'rxjs';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/toPromise';

@Injectable()
export class SessionService {
    url = "http://udiplysvcwt1/SynchronousPluginArchitecture/ProcessWCF.svc/ws";
    constructor(private http: Http) {}
    getSession() {
        return this.http.get(this.url)
           .toPromise()
		   .then(this.extractData)
           .catch(this.handleErrorPromise);
    }
    getVehicleMakes(year) {
        var url1 = "../Ajax/GetVehicleMakes";
        let urlParams = new URLSearchParams();
        urlParams.append("year", year);
        return this.http.post(url1, urlParams)
            .toPromise()
            .then(this.extractData) //res => alert("getVehicleMakes success" + res.toString()))
            .catch(this.handleErrorPromise);
    }
    getWebVehicleModels(year, makeNo) {
        var url1 = "../Ajax/GetWebVehicleModels";
        let urlParams = new URLSearchParams();
        urlParams.append("year", year);
        urlParams.append("makeNo", makeNo);
        return this.http.post(url1, urlParams)
            .toPromise()
            .then(this.extractData) //res => alert("getWebVehicleModels success" + res.toString()))
            .catch(this.handleErrorPromise);
    }
    getWebVehicleVin(year, makeNo, model) {
        var url1 = "../Ajax/GetVehicleByYearMakeModel";
        let urlParams = new URLSearchParams();
        urlParams.append("year", year);
        urlParams.append("makeNo", makeNo);
        urlParams.append("model", model);
        urlParams.append("webmodel", '1');
        return this.http.post(url1, urlParams)
            .toPromise()
            .then(this.extractData) //res => alert("GetVehicleByYearMakeModel success" + res.toString()))
            .catch(this.handleErrorPromise);
    }
    getYearMakeWebModel(vin) {
        var url1 = "../Ajax/GetVehicleYearMakeWebModel";
        let urlParams = new URLSearchParams();
        urlParams.append("vin", vin);
        return this.http.post(url1, urlParams)
            .toPromise()
            .then(this.extractData) 
            .catch(this.handleErrorPromise);
    }
    loadCoveragesAndDiscounts(webSession) {
        var url1 = "../Ajax/LoadCoveragesAndDiscounts";
        let urlSaveSessionParams = new URLSearchParams();
        urlSaveSessionParams.append("session", "{ \"WebSessionDRC\": " + JSON.stringify(webSession));
        return this.http.post(url1, urlSaveSessionParams)
            .toPromise()
            .then(this.extractData)
            .catch(this.handleErrorPromise);
    }
    ratedSave(webSession) {
        var url1 = "../Ajax/RatedSave";
        let urlSaveSessionParams = new URLSearchParams();
        urlSaveSessionParams.append("session", "{ \"WebSessionDRC\": " + JSON.stringify(webSession));
        return this.http.post(url1, urlSaveSessionParams)
            .toPromise()
            .then((response) => {
                let res = response.text();
                console.log("ratedsave response=" + res);
                return res;
            })
            .catch(this.handleErrorPromise);
    }
    recalculate(webSession) {
        var url1 = "../Ajax/Recalculate";
        let urlSaveSessionParams = new URLSearchParams();
        urlSaveSessionParams.append("session", "{ \"WebSessionDRC\": " + JSON.stringify(webSession));
        return this.http.post(url1, urlSaveSessionParams)
            .toPromise()
            .then(this.extractData)
            .catch(this.handleErrorPromise);
    }
    saveSession(webSession) {
        var url1 = "../Ajax/SaveSession";
        let urlSaveSessionParams = new URLSearchParams();
        //alert("saveSession(" + "{ \"WebSessionDRC\": " + JSON.stringify(webSession));
        urlSaveSessionParams.append("session", "{ \"WebSessionDRC\": " + JSON.stringify(webSession));
        return this.http.post(url1, urlSaveSessionParams) // { guid : guid, zip: zip, ctid: ctid }, { headers: headers})
            .toPromise()
            .then(this.extractString) 
            .catch(this.handleErrorPromise);
    }
    loadSession(guid, zipcode, ctid) {
        var url1 = "../Ajax/LoadSession";
        //var guid = "4AAF7071-60FC-4246-B854-1141CFD043D5";
        //var zip = "60103";
        //var ctid = "80001";
        let urlLoadSessionParams = new URLSearchParams();
        urlLoadSessionParams.append('guid', guid);
        urlLoadSessionParams.append('zip', zipcode);
        urlLoadSessionParams.append('ctid', ctid);
        //var headers = new Headers();
        //headers.append("Content-type", "application/x-www-form-urlencoded");
        return this.http.post(url1, urlLoadSessionParams) // { guid : guid, zip: zip, ctid: ctid }, { headers: headers})
            .toPromise()
            .then(this.extractData)
            //.then(this.extractXml)
            .catch(this.handleErrorPromise);
    }
    createNewSession(zipcode, ctid) {
        var url1 = "../Ajax/CreateNewSession";
        let urlLoadSessionParams = new URLSearchParams();
        urlLoadSessionParams.append('zip', zipcode);
        urlLoadSessionParams.append('ctid', ctid);
        return this.http.post(url1, urlLoadSessionParams) 
            .toPromise()
            .then(this.extractData)
            .catch(this.handleErrorPromise);
    }
    postGuidewire() {
        var url = "http://10.200.131.193/GuidewireFakeAPI/api/GuidewireRetrieve";
        var json = JSON.stringify({"id":"29","method":"retrieve","parms":[{"effectiveDate":"2012-07-28T16:03:54.853Z","postalCode":"19446","quoteID":"0300195917"}],"jsonrpc":"4.0"} );
        var params = "json=" + json;
        var headers = new Headers();
        headers.append("Content-type", "application/json");
        return this.http.post(url, json, { headers: headers})
           .toPromise()
		   .then(this.extractData)
           .catch(this.handleErrorPromise);
    }
    postValidate() {
        var url2 = "http://validate.jsontest.com";
        var json = JSON.stringify({"id":"29","method":"retrieve","parms":[{"effectiveDate":"2012-07-28T16:03:54.853Z","postalCode":"19446","quoteID":"0300195917"}],"jsonrpc":"4.0"} );
        var params = "json=" + json;
        var headers = new Headers();
        headers.append("Content-type", "application/x-www-form-urlencoded");
        return this.http.post(url2, params, { headers: headers})
           .toPromise()
		   .then(this.extractData)
           .catch(this.handleErrorPromise);
    }
    getDate() {
        var url = "http://date.jsontest.com";
        // var json = JSON.stringify({"id":"29","method":"retrieve","parms":[{"effectiveDate":"2012-07-28T16:03:54.853Z","postalCode":"19446","quoteID":"0300195917"}],"jsonrpc":"4.0"} );
        // var params = "json=" + json;
        // var headers = new Headers();
        // headers.append("Content-type", "application/x-www-form-urlencoded");
        return this.http.get(url)
           .toPromise()
		   .then(this.extractData)
           .catch(this.handleErrorPromise);
    }
    //getSessionWithObservable(): Observable<Session> {
    //    return this.http.get(this.url)
		  // .map(this.extractData)
    //       .catch(this.handleErrorObservable);
    //}
    private extractData(res: Response) {
        let body = res.json();
            return body;
            //return body.data || {};
    }
    private extractXml(res: Response) {
        let body = res.text();
        return body;
    }
    private extractString(res: Response) {
        let body = res.text();
        console.log("extractString returns: " + body);
        return body;
    }
    private handleErrorObservable (error: Response | any) {
        console.error(error.message || error);
        return Observable.throw(error.message || error);
    }
    private handleErrorPromise (error: Response | any) {
        console.error(error.message || error);
        return Promise.reject(error.message || error);
    }	
}