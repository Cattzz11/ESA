import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TripDetails } from '../../Models/TripDetails';
import { AuthorizeService } from '../../../api-authorization/authorize.service';
import { UserInputModel } from '../../Models/UserInput';
import { PaymentModel } from '../../Models/PaymentModel';
import { SquareService } from '../../services/SquareService';

@Component({
  selector: 'app-trip-details',
  templateUrl: './trip-details.component.html',
  styleUrls: ['./trip-details.component.css'],
})
export class TripDetailsComponent implements OnInit {
  tripDetails: TripDetails | null = null;
  userInput: UserInputModel = new UserInputModel();
  payment: PaymentModel = {
      price: 0,
      currency: '',
      email: '',
      creditCard: '',
      firstName: '',
      lastName: '',
      shippingAddress: '',
      tripId: ''
  };

  constructor(
    //private tripService: TripDetailsService, // Inject your service
    private route: ActivatedRoute,
    private authorizeService: AuthorizeService,
    private squareService: SquareService
  ) { }

  ngOnInit(): void {
    // Step 1: Get the trip ID from the route parameters
    const tripId = this.route.snapshot.paramMap.get('id');
    

    if (tripId) {

      this.authorizeService.getTripDetails(tripId).subscribe({
        next: (response) => {
          
          this.tripDetails = response;
          console.log(this.tripDetails);
        },
        error: (error) => {
          console.error('Error fetching trip details: ', error);
        }
      });
    }
  }


  //buyTicket() {
  //  const tripId = this.route.snapshot.paramMap.get('id') || '';
  //  console.log(this.userInput);
  //  const priceOf = this.tripDetails?.price || 0; // Default to 0 if tripDetails or price is not available

  //  this.payment.price = priceOf;
  //  this.payment.currency = "EUR";
  //  this.payment.firstName = this.userInput.firstName
  //  this.payment.lastName = this.userInput.lastName
  //  this.payment.email = this.userInput.shippingAddress
  //  this.payment.creditCard = this.userInput.shippingAddress
  //  this.payment.shippingAddress = this.userInput.shippingAddress

  //  this.squareService.purchaseTicket(this.payment, tripId).subscribe(
  //    (response) => {
  //      // Handle successful response
  //      console.log('Ticket purchased successfully:', response);
  //    },
  //    (error) => {
  //      // Handle error
  //      console.error('Error purchasing ticket:', error);
  //    }
  //  );
  //}


}
