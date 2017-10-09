import { Component, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import {WebApiService} from "../_services/WebApi.service";
import {NgForm} from "@angular/forms";


@Component({
    moduleId: module.id,
    templateUrl: 'register.component.html'
})

export class RegisterComponent {
    @ViewChild('f') form: NgForm;
    model: any = {};
    loading = false;

    constructor(
        private router: Router,
            private webApiService: WebApiService) { }




    register() {
        this.loading = true;

    }

    save(model:any, isValid: boolean) {
        this.webApiService.Register(model)
            .subscribe(
                data => {
                    this.router.navigate(['/']);
                });
    }




}
