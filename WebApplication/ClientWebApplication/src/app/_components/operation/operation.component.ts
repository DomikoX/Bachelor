import { Component, Input } from '@angular/core';
import {Device} from "../../_models/device";
import {WebApiService} from "../../_services/WebApi.service";


@Component({
    selector: 'operation',
    templateUrl: 'operation.component.html'
})

export class OperationComponent {
    @Input()
    devices: Device[];

    public operation;

    constructor(private webApiService: WebApiService) { }
    doOperation(){

         var text = this.devices.length == 1 ? "Are you sure to do "+ this.operation+ " on " + this.devices[0].Name + " computer" :  "Are you sure you want do "+ this.operation+ " for all devices?"
;        if (confirm(text)) {

            for(let dev of this.devices){
                this.webApiService.doOperation(dev.Id,this.operation);
            }


        } else {
        }

    }


}