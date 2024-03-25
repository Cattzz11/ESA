import { Component } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { SquareService } from '../../services/SquareService';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthorizeService } from '../../../api-authorization/authorize.service';
import { User } from '../../Models/users';
import { PopUpPremiumComponent } from './PopUpPremium/PopUpPremium.component';
import { PopUpCancelPremiumComponent } from './PopUpCancelPremium/PopUpCancel.component';

@Component({
  selector: 'app-premium',
  templateUrl: './premium.component.html',
  styleUrl: './premium.component.css'
})
export class PremiumComponent {
  user: User | null = null;

  constructor(
    private auth: AuthorizeService,
    private route: ActivatedRoute,
    private router: Router,
    private squareService: SquareService,
    private dialogRef: MatDialog
  ) { }

  ngOnInit(): void {
    const storedUser = sessionStorage.getItem('user');
    if (storedUser) {
      this.user = JSON.parse(storedUser);
    }
    else {
      this.auth.getUserInfo().subscribe({
        next: (userInfo: User) => {
          this.user = userInfo;


        },
        error: (error) => {
          console.error('Error fetching user info', error);
        }
      });
    }
  }

  premiumSubscribe() {
    this.dialogRef.open(PopUpPremiumComponent);
  }
  cancelPremiumSubscription()
  {
    this.dialogRef.open(PopUpCancelPremiumComponent);
  }

}
