import { Component } from '@angular/core';
import { AuthorizeService } from '../../../api-authorization/authorize.service';
import { User } from '../../Models/users';

@Component({
  selector: 'app-premium-profile-page',
  templateUrl: './premium-profile-page.component.html',
  styleUrl: './premium-profile-page.component.css'
})
export class PremiumProfilePageComponent {
  public user: User | null = null;

  constructor(private auth: AuthorizeService) { }

  ngOnInit() {
    this.auth.getUserInfo().subscribe({
      next: (userInfo: User) => { 
        this.user = userInfo;
      },
      error: (error) => {
        console.error('Error fetching user info', error);
      }
    });
  }

  edit() {
    console.log(this.user?.age);
  }
}
