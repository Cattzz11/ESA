import { Component, OnInit } from '@angular/core';
import { SkyscannerService } from '../../services/skyscannerService';
import { ActivatedRoute } from '@angular/router';
import { Carrier } from '../../Models/Carrier';
import { Trip } from '../../Models/Trip';
import { City } from '../../Models/City';

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

  constructor(private skyscannerService: SkyscannerService, private route: ActivatedRoute,) { }

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

  onShowPricesClick(fromEntityId: any, toEntityId: any, departureDate: any, returnDate: any) {
    this.skyscannerService.getPriceOptions(fromEntityId, toEntityId, departureDate.toString("yyyy-MM-dd"), returnDate.toString("yyyy-MM-dd")).subscribe({
      next: (response) => {
        //this.lowestPrice = response.price.valueOf();
        //this.highestPrice = response.highestPrice;
        //this.averagePrice = response.averagePrice;
        console.log(response);
        this.showPriceStatistics = true;
      },
      error: (error) => {
        console.error('Error fetching prices: ', error);
        this.showPriceStatistics = false;
      }
    });

    
  }
}
