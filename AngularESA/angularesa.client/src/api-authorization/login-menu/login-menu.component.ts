import { Component, OnInit } from '@angular/core';
import { AuthorizeService } from '../authorize.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login-menu',
  templateUrl: './login-menu.component.html',
  styleUrls: ['./login-menu.component.css']
})
export class LoginMenuComponent implements OnInit {
  public isSignedIn: boolean = false;

  constructor(private auth: AuthorizeService, private router: Router) { }

  ngOnInit() {
    this.auth.onStateChanged().forEach((state: any) => {
      this.auth.isSignedIn().forEach((signedIn: boolean) => this.isSignedIn = signedIn);
    });
  }

  signOut() {
    if (this.isSignedIn) {
      this.auth.signOutCustom().forEach(response => {
        if (response) {
          this.router.navigateByUrl('');
        }
      });
    }
  }


}
