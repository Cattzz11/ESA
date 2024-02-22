import { Component, OnInit } from '@angular/core';
import { Itinerary } from '../Models/tripData';
import { SkyscannerService } from '../services/skyscannerService';
import { AirLine } from '../Models/AirLine';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  airLineResults: AirLine[] = [];

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
  }

  searchFlights(data: AirLine) {
    this.router.navigate(['/filter-by-airline/', data.name]);
  }
}
