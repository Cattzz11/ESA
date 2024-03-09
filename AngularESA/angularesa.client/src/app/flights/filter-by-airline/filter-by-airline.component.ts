import { Component, OnInit } from '@angular/core';
import { SkyscannerService } from '../../services/skyscannerService';
import { ActivatedRoute, Router } from '@angular/router';
import { Carrier } from '../../Models/Carrier';
import { Trip } from '../../Models/Trip';
import { HttpClient } from '@angular/common/http';
import { TripDetails } from '../../Models/TripDetails';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AuthorizeService } from '../../../api-authorization/authorize.service';

@Component({
  selector: 'app-filter-by-airline',
  templateUrl: './filter-by-airline.component.html',
  styleUrl: './filter-by-airline.component.css'
})
export class FilterByAirlineComponent implements OnInit {
  flightResults: Trip[] = [];
  isLoading: boolean = true;
  carrier: Carrier | undefined;
  tripForm: FormGroup | undefined;

  constructor(private skyscannerService: SkyscannerService, private route: ActivatedRoute, private formBuilder: FormBuilder, private http: HttpClient, private router : Router) { }

  ngOnInit(): void {
    const routerState = history.state.data;

    this.tripForm = this.formBuilder.group({
      tripID: ['']
    });

    if (routerState) {
      this.carrier = routerState;

      this.skyscannerService.getSugestionsCompany(routerState.id).subscribe({
        next: (response) => {
          this.flightResults = response;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error fetching flights:', error);
          this.flightResults = [];
          this.isLoading = false;
        }
      });
    }
  }
    onTripClick(trip: Trip) {
      this.tripForm?.patchValue({
        tripID: trip.flights[0].id
      });
      // Submit the form or perform other actions as needed
      this.submitForm();
    }

  submitForm() {
    // Handle form submission logic
    const tripID = this.tripForm?.value.tripID;

    // Redirect to the '/trip-details/id' page
    this.router.navigate(['/trip-details', tripID]);
  }
    

}
