import { Component, OnInit } from '@angular/core';
import { Trip } from '../../Models/Trip';

@Component({
  selector: 'app-flight-data',
  templateUrl: './flight-data.component.html',
  styleUrl: './flight-data.component.css'
})
export class FlightDataComponent implements OnInit {
  trip: Trip | undefined;

  ngOnInit(): void {
    const routerState = history.state.data;

    if (routerState) {
      this.trip = routerState;
    }

    console.log(this.trip?.id);
  }
}

