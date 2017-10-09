import { Injectable } from '@angular/core';
import { Router, CanActivate,ActivatedRouteSnapshot,RouterStateSnapshot } from '@angular/router';

@Injectable()
export class AuthGuard implements CanActivate {

    constructor(private router: Router) { }



    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {


        var user = localStorage.getItem('currentUser');

        if (user) {
            if(state.url == '/admin'){
                if(JSON.parse(user).role == 'Admin'){
                    return true;
                }
                this.router.navigate(['/']);
                return false;
            }
            // logged in so return true
            return true;
        }

        // not logged in so redirect to login page
        this.router.navigate(['/login']);
        return false;
    }
}