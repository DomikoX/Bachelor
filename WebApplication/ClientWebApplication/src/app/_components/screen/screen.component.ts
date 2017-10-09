/**
 * Created by DomikoX on 11.3.2017.
 */
import { Component, Input } from '@angular/core';
import {Device} from "../../_models/device";
import {WebApiService} from "../../_services/WebApi.service";


@Component({
    selector: 'screen',
    templateUrl: 'screen.component.html'
})

export class ScreenComponent {
    @Input()
    devices:Device[];

    constructor(private webApiService:WebApiService) {

    }

    startScreen() {
        for (let device of this.devices) {
            this.webApiService.startScreenWatching(device.Id);
        }


    }


    stopScreen() {
        for (let device of this.devices) {
            this.webApiService.stopScreenWatching(device.Id);
        }

    }


}