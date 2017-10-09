import { Component } from '@angular/core';
import { Router } from '@angular/router';
import {WebApiService} from "../_services/WebApi.service";


@Component({
    templateUrl: 'admin.component.html'
})

export class AdminComponent {
    constructor( private webApiService: WebApiService) { }





}
