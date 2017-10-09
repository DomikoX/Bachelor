///<reference path="../../_services/WebApi.service.ts"/>
/**
 * Created by DomikoX on 11.3.2017.
 */
import { Component, Input, NgZone } from '@angular/core';
import {Device} from "../../_models/device";
import {WebApiService} from "../../_services/WebApi.service";


@Component({
    selector: 'file-upload',
    templateUrl: 'file-upload.component.html'
})

export class FileUploadComponent {
    public progress: number = 0;
    @Input()
    devices:Device[];
    @Input()
    toOpenFolder:boolean;

    private fileToUpload;

    constructor(private webApiService:WebApiService, private zone:NgZone) {
    }

    upload(){
        console.log("UPLAOD");
        this.webApiService.uploadFile(this.fileToUpload,this.devices,this.toOpenFolder).subscribe(progress => {
            this.zone.run(() => this.progress = progress);
            console.log(progress);
        });
    }


    onChange($event){
        this.fileToUpload =  event.srcElement['files'][0];
        console.log(this.fileToUpload);
    }

}