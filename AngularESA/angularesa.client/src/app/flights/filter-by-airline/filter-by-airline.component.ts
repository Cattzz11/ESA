import { Component, OnInit } from '@angular/core';
import { SkyscannerService } from '../../services/skyscannerService';
import { ActivatedRoute } from '@angular/router';
import { Carrier } from '../../Models/Carrier';
import { Trip } from '../../Models/Trip';

@Component({
  selector: 'app-filter-by-airline',
  templateUrl: './filter-by-airline.component.html',
  styleUrl: './filter-by-airline.component.css'
})
export class FilterByAirlineComponent implements OnInit {
  flightResults: Trip[] = [];
  isLoading: boolean = true;
  carrier: Carrier | undefined;
  cheapestPrices: any[] = [];
  showPriceStatistics!: boolean | false;

  lowestPrice: number | undefined;
  highestPrice: number | undefined;
  averagePrice: number | undefined;

  constructor(private skyscannerService: SkyscannerService, private route: ActivatedRoute,) {}

  ngOnInit(): void {
    const routerState = history.state.data;
    
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

  onShowPricesClick() {
    // Toggle the showPriceStatistics variable
    this.showPriceStatistics = !this.showPriceStatistics;
  }
}
