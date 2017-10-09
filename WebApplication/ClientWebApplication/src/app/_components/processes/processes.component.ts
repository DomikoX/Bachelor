/**
 * Created by DomikoX on 11.3.2017.
 */
/**
 * Created by DomikoX on 11.3.2017.
 */
import { Component, Input } from '@angular/core';
import {Device} from "../../_models/device";
import {WebApiService} from "../../_services/WebApi.service";


@Component({
    selector: 'processes',
    templateUrl: 'processes.component.html'
})

export class ProcessesComponent {
    @Input()
    device:Device;

    constructor(private webApiService:WebApiService) {

    }

    getProcesses() {
        this.webApiService.getProcesses(this.device.Id).subscribe(records => {
            console.log(records);
            this.device.Processes = records;
        })
    }

    kill(id) {
        this.webApiService.killProcces(this.device.Id, id).subscribe(records => {
           this.getProcesses();
        })
    }

}