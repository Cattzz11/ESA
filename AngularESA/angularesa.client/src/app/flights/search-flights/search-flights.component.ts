import { Component } from '@angular/core';
import { SkyscannerService } from '../../services/skyscannerService';
import { FlightData } from '../../Models/flight-data';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-search-flights',
  templateUrl: './search-flights.component.html',
  styleUrl: './search-flights.component.css'
})
export class SearchFlightsComponent {
  flightResults: any = [];

  constructor(private skyscannerService: SkyscannerService) { }

  private createFlightDataFromForm(formValue: any): FlightData {
    let flightData: FlightData = {
      fromEntityId: formValue.fromEntityId,
      toEntityId: formValue.toEntityId,
      departDate: formValue.departDate,
      returnDate: formValue.returnDate || null,
      market: formValue.market || null,
      locale: formValue.locale || null,
      currency: formValue.currency || null,
      adults: formValue.adults || null,
      children: formValue.children || null,
      infants: formValue.infants || null,
      cabinClass: formValue.cabinClass || null,
    };
    return flightData;
  }

  onSubmit(form: NgForm) {
    if (form.valid) {
      const flightData: FlightData = this.createFlightDataFromForm(form.value);
      // Chama o serviço SkyscannerService com os dados do voo coletados do formulário
      this.skyscannerService.getFlights(flightData).subscribe({
        next: (data) => {
          this.flightResults = data;
        },
        error: (error) => {
          console.error('Error fetching flights:', error);
          this.flightResults = [];
        }
      });

    }
  }

}
