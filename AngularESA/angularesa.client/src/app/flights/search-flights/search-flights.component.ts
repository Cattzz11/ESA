import { Component, OnInit } from '@angular/core';
import { SkyscannerService } from '../../services/skyscannerService';
import { FlightData } from '../../Models/flight-data';
import { FormBuilder, FormGroup, NgForm } from '@angular/forms';

@Component({
  selector: 'app-search-flights',
  templateUrl: './search-flights.component.html',
  styleUrls: ['./search-flights.component.css']
})

export class SearchFlightsComponent implements OnInit {
  flightSearchForm!: FormGroup;
  flightResults: any = []; // Considere definir uma interface para os resultados

  constructor(private skyscannerService: SkyscannerService, private formBuilder: FormBuilder) { }

  ngOnInit(): void {
    this.flightSearchForm = this.formBuilder.group({
      fromEntityId: [''],
      toEntityId: [''],
      departDate: [''],
      returnDate: [''],
      market: [''],
      locale: [''],
      currency: [''],
      adults: [''],
      children: [''],
      infants: [''],
      cabinClass: [''],
    });
  }

  searchFlight() {
    console.log("AQUI!");

    if (this.flightSearchForm.valid) {
      const flightData: FlightData = this.flightSearchForm.value as FlightData;

      console.log("Valido!");
      console.log(flightData.fromEntityId);
      console.log(flightData.toEntityId);
      console.log(flightData.departDate);
      console.log(flightData.returnDate);


      this.skyscannerService.getFlights(flightData).subscribe({
        next: (response) => {
          // Assumindo que os dados de interesse estão em `response.data.itineraries`
          this.flightResults = response.data.itineraries;
        },
        error: (error) => {
          console.error('Error fetching flights:', error);
          this.flightResults = [];
        }
      });
    }
  }

  printData() {
    for (let entry of this.flightResults) {
      console.log(`ID: ${entry.id}`);
      console.log(`Price: ${entry.price.raw} (${entry.price.formatted})`);
      console.log(`Self Transfer: ${entry.isSelfTransfer}`);
      console.log(`Protected Self Transfer: ${entry.isProtectedSelfTransfer}`);
      console.log(`Legs:`);
      for (let leg of entry.legs) {
        console.log(`  Leg ID: ${leg.id}`);
        console.log(`  Departure: ${leg.departure}`);
        console.log(`  Arrival: ${leg.arrival}`);
        console.log(`  Duration in Minutes: ${leg.durationInMinutes}`);
        console.log(`  Stop Count: ${leg.stopCount}`);
        console.log(`  Origin: ${leg.origin.city} (${leg.origin.displayCode})`);
        console.log(`  Destination: ${leg.destination.city} (${leg.destination.displayCode})`);
        console.log(`  Carriers:`);
        for (let carrier of leg.carriers.marketing) {
          console.log(`    Carrier Name: ${carrier.name}`);
          console.log(`    Carrier Logo: ${carrier.logoUrl}`);
        }
        console.log(`  Segments:`);
        for (let segment of leg.segments) {
          console.log(`    Segment ID: ${segment.id}`);
          console.log(`    Flight Number: ${segment.flightNumber}`);
          console.log(`    Departure: ${segment.departure}`);
          console.log(`    Arrival: ${segment.arrival}`);
          console.log(`    Marketing Carrier: ${segment.marketingCarrier.name}`);
          console.log(`    Operating Carrier: ${segment.operatingCarrier.name}`);
          // Assumindo que `origin` e `destination` de cada segmento têm uma estrutura similar a de `leg`
          console.log(`    Origin: ${segment.origin.name} (${segment.origin.displayCode})`);
          console.log(`    Destination: ${segment.destination.name} (${segment.destination.displayCode})`);
        }
      }
      console.log('-----------------------------------');
    }
  }

}
