import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { Observable, map } from "rxjs";
import { AuthorizeService } from "./authorize.service";

@Injectable({ providedIn: 'root' })
// protects routes from unauthenticated users
export class AuthGuard {
  constructor(private authService: AuthorizeService, private router: Router) { }

  canActivate() {
    return this.isSignedIn();
  }

  isSignedIn(): Observable<boolean> {
    return this.authService.isSignedIn().pipe(
      map((isSignedIn) => {
        if (!isSignedIn) {
          // redirect to signin page
          this.router.navigate(['signin']);
          return false;
        }
        return true;
      }));
  }
}
