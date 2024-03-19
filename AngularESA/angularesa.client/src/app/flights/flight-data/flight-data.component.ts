import { Component, OnInit, numberAttribute } from '@angular/core';
import { Trip } from '../../Models/Trip';
import { User } from '../../Models/users';
import { AuthorizeService } from '../../../api-authorization/authorize.service';
import { TripDetails } from '../../Models/TripDetails';
import { ActivatedRoute, Router } from '@angular/router';
import { SquareService } from '../../services/SquareService';
import { PaymentModel } from '../../Models/PaymentModel';
import { PriceOptions } from '../../Models/PriceOptions';

@Component({
  selector: 'app-flight-data',
  templateUrl: './flight-data.component.html',
  styleUrl: './flight-data.component.css'
})
export class FlightDataComponent implements OnInit {
  user: User | null = null;

  trip: Trip | undefined;
  tripPremium: TripDetails | undefined;

  payment: PaymentModel | undefined;

  constructor(
    private auth: AuthorizeService,
    private route: ActivatedRoute,
    private router: Router,
    private squareService: SquareService
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

  buyTicket(trip: Trip) {
    if (this.user && this.user.role !== 1) {
      let payment: PaymentModel = {
        price: trip.price,
        currency: "EUR",
        email: this.user.email,
        creditCard: this.user.email,
        firstName: this.user.name,
        lastName: this.user.name,
        shippingAddress: this.user.name,
        trip: trip
      }

      this.squareService.purchaseTicket(payment).subscribe(
        (response) => {
          // Handle successful response
          console.log('Ticket purchased successfully:', response);
        },
        (error) => {
          // Handle error
          console.error('Error purchasing ticket:', error);
        }
      );
    } else {
      
    }

    
  }

  buyPage(trip: Trip) {
    this.router.navigate(['/payment']); 
  }

}
