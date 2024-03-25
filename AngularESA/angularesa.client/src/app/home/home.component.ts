import { Component, OnInit } from '@angular/core';
import { SkyscannerService } from '../services/skyscannerService';
import { Router } from '@angular/router';
import { Carrier } from '../Models/Carrier';
import { Trip } from '../Models/Trip';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  airLineResults: Carrier[] = [];
  flightResults: Trip[] = [];
  isLoading: boolean = true;
  selectedOption: string = 'airlines';

  constructor(private skyscannerService: SkyscannerService, private router: Router) { }

  ngOnInit(): void {
    this.skyscannerService.getFavoriteAirline().subscribe({
      next: (response) => {
        this.airLineResults = response;
        for (var val of response) {
          console.log(val);
        }
      },
      error: (error) => {
        console.error('Error fetching flights:', error);
        this.airLineResults = [];
      }
    });

    this.skyscannerService.getSugestionsDestinations().subscribe({
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

  searchFlights(data: Carrier) {
    this.router.navigate(['/filter-by-airline'], { state: { data: data } });
  }

  selectTrip(trip: any) {
    this.router.navigate(['/flight-data'], { state: { data: trip } });
  }
  
}
