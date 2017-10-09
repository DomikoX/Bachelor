/**
 * Created by DomikoX on 11.3.2017.
 */
import { Component, Input } from '@angular/core';
import {Device} from "../../_models/device";
import {WebApiService} from "../../_services/WebApi.service";


@Component({
    selector: 'messages',
    templateUrl: 'messages.component.html'
})

export class MessagesComponent {
    model: any = {};
    @Input()
    devices:Device[];

    constructor(private webApiService:WebApiService) {

    }

    send() {
        for(let dev of this.devices){
            this.webApiService.sendMessage(dev.Id,this.model.tittle, this.model.text);
        }


    }

    block() {
        for(let dev of this.devices){
            this.webApiService.block(dev.Id);
        }

    }

    unbBlock() {
        for(let dev of this.devices){
            this.webApiService.unBlock(dev.Id);
        }

    }

}