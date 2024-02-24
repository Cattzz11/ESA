import { Component, OnInit } from '@angular/core';
import { SkyscannerService } from '../../services/skyscannerService';
import { ActivatedRoute } from '@angular/router';
import { Flight } from '../../Models/Flight';
import { Carrier } from '../../Models/Carrier';

@Component({
  selector: 'app-filter-by-airline',
  templateUrl: './filter-by-airline.component.html',
  styleUrl: './filter-by-airline.component.css'
})
export class FilterByAirlineComponent implements OnInit {
  flightResults: Flight[] = [];
  airlineId: string = "";
  airlineLogo: string = "";
  isLoading: boolean = true;

  constructor(private skyscannerService: SkyscannerService, private route: ActivatedRoute,) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.airlineId = this.route.snapshot.params['id'];
      this.airlineLogo = this.route.snapshot.params['logoURL'];
    });
    console.log(this.airlineLogo);

    this.skyscannerService.getSugestionsCompany(this.airlineId).subscribe({
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
