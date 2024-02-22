import { Component, OnInit } from '@angular/core';
import { Itinerary } from '../Models/tripData';
import { SkyscannerService } from '../services/skyscannerService';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  flightResults: Itinerary[] = [];

  constructor(private skyscannerService: SkyscannerService) { }

  ngOnInit(): void {
    this.skyscannerService.getSugestionsCompany().subscribe({
      next: (response) => {
        this.flightResults = response;
        for (var val of response) {
          console.log(val);
        }
      },
      error: (error) => {
        console.error('Error fetching flights:', error);
        this.flightResults = [];
      }
    });
  }

  public printAirports() {
    for (var val of this.flightResults) {
      for (var val2 of val.trip) {
        console.log(val2);
      }
    }
  }
}
