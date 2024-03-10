import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TripDetails } from '../../Models/TripDetails';
import { AuthorizeService } from '../../../api-authorization/authorize.service';

@Component({
  selector: 'app-trip-details',
  templateUrl: './trip-details.component.html',
  styleUrls: ['./trip-details.component.css'],
})
export class TripDetailsComponent implements OnInit {
  tripDetails: TripDetails | null = null;

  constructor(
    //private tripService: TripDetailsService, // Inject your service
    private route: ActivatedRoute,
    private authorizeService : AuthorizeService
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
}
