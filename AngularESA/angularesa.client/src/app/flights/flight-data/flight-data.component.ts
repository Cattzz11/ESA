import { Component, OnInit, numberAttribute } from '@angular/core';
import { Trip } from '../../Models/Trip';
import { User } from '../../Models/users';
import { AuthorizeService } from '../../../api-authorization/authorize.service';
import { TripDetails } from '../../Models/TripDetails';

@Component({
  selector: 'app-flight-data',
  templateUrl: './flight-data.component.html',
  styleUrl: './flight-data.component.css'
})
export class FlightDataComponent implements OnInit {
  user: User | null = null;

  trip: Trip | undefined;
  tripPremium: TripDetails | undefined;

  constructor(
    private auth: AuthorizeService
  ) { }

  ngOnInit(): void {
    const storedUser = sessionStorage.getItem('user');
    if (storedUser) {
      this.user = JSON.parse(storedUser);
    }
    else
    {
      this.auth.getUserInfo().subscribe({
        next: (userInfo: User) => {
          this.user = userInfo;

          const routerState = history.state.data;

          if (this.user && this.user.role === 1) {
            if (routerState) {
              this.tripPremium = routerState;
            }
          } else {
            if (routerState) {
              this.trip = routerState;
            }
          }
        },
        error: (error) => {
          console.error('Error fetching user info', error);
        }
      });
    }
  }
}
