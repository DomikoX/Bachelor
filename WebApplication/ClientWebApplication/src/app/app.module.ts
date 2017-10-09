import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';

import { AppComponent } from './app.component';
import {RouterModule} from "@angular/router";
import {AuthGuard} from "./_guards/auth.guard";
import {HomeComponent} from "./home/home.component";
import {LoginComponent} from "./login/login.component";
import {WebApiService} from "./_services/WebApi.service";
import {DeviceDetailComponent} from "./_components/deviceDetail/device-detail.component";
import {OperationComponent} from "./_components/operation/operation.component";
import {ExplorerComponent} from "./_components/explorer/explorer.component";
import {MessagesComponent} from "./_components/message/messages.component";
import {ProcessesComponent} from "./_components/processes/processes.component";
import {MultipleDeviceComponent} from "./_components/multipleDevice/multiple-device.component";
import {FileUploadComponent} from "./_components/fileUpload/file-upload.component";
import {ScreenComponent} from "./_components/screen/screen.component";
import {RegisterComponent} from "./register/register.component";
import {EqualValidator} from "./_components/equal-validator.directive";
import {AdminComponent} from "./admin/admin.component";



@NgModule({
    declarations: [
        AppComponent,
        HomeComponent,
        LoginComponent,
        AdminComponent,
        RegisterComponent,
        DeviceDetailComponent,
        OperationComponent,
        ExplorerComponent,
        MessagesComponent,
        ProcessesComponent,
        MultipleDeviceComponent,
        FileUploadComponent,
        ScreenComponent,
        EqualValidator,
    ],
    imports: [
        BrowserModule,
        FormsModule,
        HttpModule,
        RouterModule.forRoot([
            {path: 'login', component: LoginComponent},
            {path: 'register', component: RegisterComponent},
            {path: 'admin', component: AdminComponent, canActivate: [AuthGuard]},
            {path: '', component: HomeComponent, canActivate: [AuthGuard]},
            {path: '**', redirectTo: ''}
        ])
    ],
    providers: [WebApiService, AuthGuard],
    bootstrap: [AppComponent]
})
export class AppModule {
}
