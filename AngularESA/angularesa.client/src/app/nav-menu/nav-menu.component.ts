import { Component } from '@angular/core';
import { AuthorizeService } from '../../api-authorization/authorize.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;
  public isSignedIn: boolean = false;

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  constructor(private auth: AuthorizeService, private router: Router) { }

  get isUserSignedIn() {
    return !!sessionStorage.getItem('user');
  }

  ngOnInit() {
    this.auth.onStateChanged().subscribe((state: any) => {
      this.auth.isSignedIn().subscribe((signedIn: boolean) => {
        this.isSignedIn = signedIn;
      });
    });
  }

  //signOut() {
  //  if (this.isSignedIn) {
  //    this.auth.signOutCustom().subscribe(response => {
  //      if (response) {
  //        sessionStorage.removeItem('user');
  //        this.router.navigateByUrl('/');

  //        console.log("logout");
  //      }
  //    });

  //    console.log("user eliminado");
  //  }
  //}

  signOut() {
    this.auth.signOut();
  }

  //logout() {
  //  if (this.isSignedIn) {
  //    this.auth.signOutCustom().subscribe(response => {
  //      if (response) {
  //        sessionStorage.removeItem('user');
  //        this.router.navigateByUrl('/');
  //        this.auth.signOut();
  //        console.log("logout");
  //      }
  //    });

  //    console.log("user eliminado");
  //  }
  //}


}
