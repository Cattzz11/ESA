import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthorizeService } from '../authorize.service';

@Component({
  selector: 'app-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.css']
})
export class LogoutComponent {

  constructor(private authService: AuthorizeService, private router: Router) { }

  confirmSignOut() {
    this.authService.signOutGoogle(); // Assuming AuthService has a signOut() method
    this.router.navigate(['/']); // Navigate to home or another page after signing out
  }

  cancelSignOut() {
    // Optionally, you can navigate to another page or take other actions on cancel
    this.router.navigate(['/']);
  }

}
