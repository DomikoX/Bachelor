/**
 * Created by DomikoX on 11.3.2017.
 */
import { Component, Input } from '@angular/core';
import {Device} from "../../_models/device";
import {WebApiService} from "../../_services/WebApi.service";
import {Record} from "../../_models/Record";


@Component({
    selector: 'explorer',
    templateUrl: 'explorer.component.html'
})

export class ExplorerComponent {
    @Input()
    device:Device;

    constructor(private webApiService:WebApiService) {

    }

    explore() {
        this.webApiService.explore(this.device.Id).then(records => {
            this.device.Records = (records as Record[]);
        })
    }

    open(i) {
        this.webApiService.openFile(this.device.Id, i).then(records => {
            if (records != null) {
                this.device.Records = (records as Record[]);
            }
        })
    }

    dwn(fileName:string) {


       this.webApiService.downloadFile(this.device,fileName);

    }

}