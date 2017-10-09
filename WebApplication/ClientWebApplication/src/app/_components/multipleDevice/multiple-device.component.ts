import { Component, Input } from '@angular/core';
import {Device} from "../../_models/device";
import {WebApiService} from "../../_services/WebApi.service";


@Component({
    selector: 'multiple-device',
    templateUrl: 'multiple-device.component.html'
})

export class MultipleDeviceComponent {
    @Input()
    devices: Device[];

    constructor(private webApiService: WebApiService) { }








}