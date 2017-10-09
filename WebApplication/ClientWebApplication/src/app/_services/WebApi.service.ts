import { Injectable } from '@angular/core';
import { Http, Headers, RequestOptions, Response } from '@angular/http';
import { Observable ,BehaviorSubject} from 'rxjs';

import {Router} from "@angular/router";
import 'rxjs/add/operator/map'
import {Device} from "../_models/device";

@Injectable()
export class WebApiService {


    private _devicesMap:Map<string,Device> = new Map<string,Device>();
    private _devices:BehaviorSubject<any> = new BehaviorSubject(null);

    public devices:Observable<any> = this._devices.asObservable();

    public token:string;
    private  url = "http://sharp.kst.fri.uniza.sk/remote/Service1.svc/";
    private  fileUploadUrl = "http://sharp.kst.fri.uniza.sk/remote/FileTransferService.svc/";
    public username = "";
    private usageMap:Map<string, number> = new Map();
    private screenMap:Map<string, number> = new Map();
    private deviceListTimer:number = 0;

    private role:string;

    constructor(private http:Http, private router:Router) {
        // set token if saved in local storage
        var currentUser = JSON.parse(localStorage.getItem('currentUser'));
        this.username = currentUser && currentUser.username;
        this.token = currentUser && currentUser.token;
        if(currentUser){
            this.startRefreshingList();
        }



    }

    private authorizedPost(url:string, body:any):Promise<any> {
        var headers = new Headers({'Content-Type': 'application/json', 'Authorization': this.token,});
        var options = new RequestOptions({headers: headers});
        return this.http.post(this.url + url, body, options).map((response:Response) => {
            if (response.json()[url + 'Result']) {
                return response.json()[url + 'Result'];
            }
            return response.json();
        }).toPromise().catch((error) => {
            console.log("Error while Post");
        });
    }

    private authorizedGet(url:string):Promise<any> {
        var headers = new Headers({'Content-Type': 'application/json', 'Authorization': this.token,});
        var options = new RequestOptions({headers: headers});
        return this.http.get(this.url + url, options).map((response:Response) => {
            if (response.json()[url + 'Result']) {
                return response.json()[url + 'Result'];
            }
            return response.json();
        }).toPromise().catch((error) => {
            if (error.status == 401) {
                this.logout();
                this.router.navigate(['/login']);
            }

            console.log("Error while Get ", error);


        });
    }

    isAdmin():boolean {
        return this.role == "Admin";
    }

    Register(model:any):any {
        let headers = new Headers({'Content-Type': 'application/json'});
        let options = new RequestOptions({headers: headers});
        //  string Register(string username, string password, string passwordAgain, string email);
        console.log(model);

        return this.http.post(this.url + "Register", JSON.stringify({
                username: model.username,
                password: model.password,
                passwordAgain: model.confirmPassword,
                email: model.email
            }), options)
            .map((response:Response) => {
                // login successful if there's a jwt token in the response
                let token = response.json() && response.json().RegisterResult;
                if (token) {
                    // set token property

                    console.log(token);
                    this.username = model.username;
                    this.token = token;
                    this.role = JSON.parse(atob(token.split('.')[1])).role;


                    // store username and jwt token in local storage to keep user logged in between page refreshes
                    localStorage.setItem('currentUser', JSON.stringify({
                        username: model.username,
                        token: token,
                        role: this.role
                    }));
                    console.log(localStorage.getItem("token"));
                    // return true to indicate successful login
                    return true;
                } else {
                    // return false to indicate failed login
                    return false;
                }
            });
    }


    login(username:string, password:string):Observable<boolean> {
        let headers = new Headers({'Content-Type': 'application/json'});
        let options = new RequestOptions({headers: headers});


        return this.http.post(this.url + "Login", JSON.stringify({username: username, password: password}), options)
            .map((response:Response) => {
                // login successful if there's a jwt token in the response
                let token = response.json() && response.json().LoginResult;
                if (token) {
                    // set token property

                    console.log(token);
                    this.username = username;
                    this.token = token;
                    this.role = JSON.parse(atob(token.split('.')[1])).role;

                    console.log();


                    // store username and jwt token in local storage to keep user logged in between page refreshes
                    localStorage.setItem('currentUser', JSON.stringify({
                        username: username,
                        token: token,
                        role: this.role
                    }));
                    console.log(localStorage.getItem("token"));
                    this.startRefreshingList();
                    // return true to indicate successful login
                    return true;
                } else {
                    // return false to indicate failed login
                    return false;
                }
            });
    }

    logout():void {
        // clear token remove user from local storage to log user out
        this.stopRefreshingList();
        this._devices.next([]);
        this.username = null;
        this.token = null;
        localStorage.removeItem('currentUser');
    }

    startUsage(deviceID:string) {
        //  void AddAsUsageClient(string deviceId, string clientId);
        this.authorizedPost("AddAsUsageClient", JSON.stringify({deviceId: deviceID, clientId: this.username}))
            .then();

        if (this.usageMap.has(deviceID)) {
            clearInterval(this.usageMap.get(deviceID));
        }


        //DeviceUsage GetLastUsageUpdate(string deviceId, string clientId);
        let timer:number = setInterval(() => {
            this.authorizedPost("GetLastUsageUpdate", JSON.stringify({deviceId: deviceID, clientId: this.username}))
                .then((response:any) => {
                    for (let device of (this._devices.getValue())) {

                        if (device.Id == deviceID) {
                            let res = response;
                            device.CpuUsage = res.CpuUsage;
                            device.RamUsage = res.RamUsage;
                            device.LastUsageTime = new Date()
                            return;
                        }
                        break;
                    }
                });

        }, 1000);
        this.usageMap.set(deviceID, timer);

    }

    stopUsage(deviceID:string) {
        // void RemoveFromUsageClients(string deviceId, string clientId);
        this.authorizedPost("RemoveFromUsageClients", JSON.stringify({deviceId: deviceID, clientId: this.username}))
            .then();


        if (this.usageMap.has(deviceID)) {
            clearInterval(this.usageMap.get(deviceID));
        }
    }


    startScreenWatching(deviceID:string) {
        //  void AddAsScreenClient(string deviceId, string clientId);
        this.authorizedPost("AddAsScreenClient", JSON.stringify({deviceId: deviceID, clientId: this.username}))
            .then();

        if (this.screenMap.has(deviceID)) {
            clearInterval(this.screenMap.get(deviceID));
        }


        // byte[] GetLastScreenUpdate(string deviceId, string clientId);
        let timer:number = setInterval(() => {
            this.authorizedPost("GetLastScreenUpdate", JSON.stringify({deviceId: deviceID, clientId: this.username}))
                .then((response:any) => {
                    for (let device of (this._devices.getValue())) {

                        if (device.Id == deviceID) {
                            console.log(response);
                            device.LastScreen = "data:image/jpeg;base64," + response;
                            return;
                        }
                        break;
                    }
                });

        }, 2000);
        this.screenMap.set(deviceID, timer);

    }

    stopScreenWatching(deviceID:string) {
        //   void RemoveFromScreenClients(string deviceId, string clientId);
        this.authorizedPost("RemoveFromScreenClients", JSON.stringify({deviceId: deviceID, clientId: this.username}))
            .then();


        if (this.screenMap.has(deviceID)) {
            clearInterval(this.screenMap.get(deviceID));
        }
    }


    startRefreshingList(){
        this.getOnlineDeviceList();
        this.deviceListTimer =  setInterval(() => {
            this.getOnlineDeviceList();
        }, 30 *1000);


    }

    stopRefreshingList(){
        if(this.deviceListTimer != 0){
            clearInterval(this.deviceListTimer);
            this.deviceListTimer = 0;
        }

    }



    getOnlineDeviceList() {
        this.authorizedGet("GetOnlineDeviceList")
            .then((response:any) => {
                if (!response) {
                    return;
                }

                var nieco:Device[] = (<Device[]>response);

                for (let dev of nieco) {
                    if (this._devicesMap.has(dev.Id)) {
                         var device:Device = this._devicesMap.get(dev.Id);

                        device.Name = dev.Name;
                        device.CpuInfo = dev.CpuInfo;
                        device.OsInfo = dev.OsInfo;
                        device.Online = dev.Online;
                        device.IsSelected = dev.IsSelected;
                    }else{
                        this._devicesMap.set(dev.Id,dev);
                    }
                }


                this._devices.next(Array.from(this._devicesMap.values()));
                return;


            });


    }

    doOperation(deviceID:string, operation:any):void {

        this.authorizedPost("DoOperation", JSON.stringify({
            deviceId: deviceID,
            operation: operation
        })).then();
    }

    explore(deviceId:String):any {
        return this.authorizedPost("RunExplorer", JSON.stringify({
            deviceId: deviceId,
            clientId: this.username
        }));

    }

    openFile(deviceId:String, index:number):any {
        return this.authorizedPost("OpenDirectory", JSON.stringify({
            deviceId: deviceId,
            clientId: this.username,
            index: index,
        }));
    }

    sendMessage(deviceId:String, tittle:string, text:string):void {
        this.authorizedPost("SendMessage", JSON.stringify({
            id: deviceId,
            tittle: tittle,
            message: text,
        })).then();
    }

    block(deviceId:String):void {
        this.authorizedPost("Block", JSON.stringify({
            deviceId: deviceId,
        })).then();
    }

    unBlock(deviceId:String):void {
        this.authorizedPost("UnBlock", JSON.stringify({
            deviceId: deviceId,
        })).then();
    }

    getProcesses(deviceId:String):any {
        return this.authorizedPost("GetRunningProcesses", JSON.stringify({
            deviceId: deviceId,
        }));
    }

    killProcces(deviceId:String, pid:any):any {
        return this.authorizedPost("KillProcess", JSON.stringify({
            deviceId: deviceId,
            pid: pid,
        }));
    }

    uploadFile(fileToUpload:any, devices:Device[], toOpenFolder:boolean) {


        return Observable.create(observer => {

            let formData:FormData = new FormData();
            let xhr:XMLHttpRequest = new XMLHttpRequest();


            formData.append("uploading", fileToUpload);

            xhr.onreadystatechange = () => {
                if (xhr.readyState === 4) {
                    if (xhr.status === 200) {
                        this.notifyDevices(fileToUpload.name, devices, toOpenFolder);
                        observer.complete();
                    } else {
                        console.log(xhr.response);
                        observer.error(xhr.response);
                    }
                }
            };

            xhr.upload.onprogress = (event) => {
                let value = Math.round(event.loaded / event.total * 100);
                observer.next(value);

            };

            xhr.open('POST', this.fileUploadUrl + 'Upload', true);
            xhr.setRequestHeader("file-name", fileToUpload.name);
            xhr.send(fileToUpload);
        });

    }

    private notifyDevices(fileName:string, devices:Device[], toOpenFolder:boolean) {

        // void PickFile(string deviceId,string fileName, string clientId, bool saveToOpenFolder);

        for (let device of devices) {
            this.authorizedPost("PickFile", JSON.stringify({
                deviceId: device.Id,
                fileName: fileName,
                clientId: this.username,
                saveToOpenFolder: toOpenFolder,
            })).then();
        }


    }


    downloadFile(device:Device, fileName:string) {
        // void PrepareFileOnServer(string deviceId, string clientId, string fileName);
        this.authorizedPost("PrepareFileOnServer", JSON.stringify({
            deviceId: device.Id,
            clientId: this.username,
            fileName: fileName,
        })).then((response) => {
            console.log(response);
            window.open(this.fileUploadUrl + "Download/" + fileName);
        }, errorr => console.log(errorr));


    }


}