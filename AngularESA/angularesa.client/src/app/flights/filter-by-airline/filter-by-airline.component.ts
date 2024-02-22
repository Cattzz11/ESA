import { Component, OnInit } from '@angular/core';
import { SkyscannerService } from '../../services/skyscannerService';
import { Itinerary } from '../../Models/tripData';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-filter-by-airline',
  templateUrl: './filter-by-airline.component.html',
  styleUrl: './filter-by-airline.component.css'
})
export class FilterByAirlineComponent implements OnInit {
  flightResults: Itinerary[] = [];
  airline: string = "";
  isLoading: boolean = true;

  constructor(private skyscannerService: SkyscannerService, private route: ActivatedRoute,) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.airline = this.route.snapshot.params['airline'];
    });

    this.skyscannerService.getSugestionsCompany(this.airline).subscribe({
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
