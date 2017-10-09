import { Component, OnInit } from '@angular/core';

import { User } from '../_models/index';
import {WebApiService} from "../_services/WebApi.service";
import {Device} from "../_models/device";

@Component({
    moduleId: module.id,
    templateUrl: 'home.component.html'
})

export class HomeComponent implements OnInit {
    devices:Device[] = [];
    selectedDevice:Device;
    multipleSelectedDevices:Device[] = [];

    constructor(public webApiService:WebApiService) {
    }

    ngOnInit() {
        this.webApiService.devices.subscribe((array) => {
            this.devices = array;
            if (this.devices == null || this.devices.length == 0) {
                this.selectedDevice = null;
            }
        });

    }

    selectDevice(device:Device) {
        this.selectedDevice = device;
    }

    refresh() {
        this.webApiService.getOnlineDeviceList();
    }

    addDevice(device:Device) {
        if (device.IsSelected) {
            this.multipleSelectedDevices.push(device);
        } else {
            var index = this.multipleSelectedDevices.indexOf(device);
            this.multipleSelectedDevices.splice(index, 1);
        }
    }
}