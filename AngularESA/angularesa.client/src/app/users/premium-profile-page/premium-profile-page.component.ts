import { Component } from '@angular/core';
import { AuthorizeService } from '../../../api-authorization/authorize.service';
import { User } from '../../Models/users';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-premium-profile-page',
  templateUrl: './premium-profile-page.component.html',
  styleUrl: './premium-profile-page.component.css'
})
export class PremiumProfilePageComponent {
  public user: User | null = null;

  constructor(private auth: AuthorizeService, private formBuilder: FormBuilder)
  {
    
  }

  ngOnInit() {
    this.auth.getUserInfo().subscribe({
      next: (userInfo: User) => { 
        this.user = userInfo;
        //this.editForm.patchValue({
        //  name: userInfo.name,
        //  birthDate: userInfo.age,
        //  nationality: userInfo.nationality,
        //  occupation: userInfo.occupation
        //});
      },
      error: (error) => {
        console.error('Error fetching user info', error);
      }
    });
  }
}
