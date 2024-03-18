import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { FlightsItinerary } from "../Models/FlightsItinerary";

@Injectable({
  providedIn: 'root'
})
export class FlightItineraryService {
  constructor(private http: HttpClient) { }

  public getMap(): Observable<any> {
    return this.http.get('api/flight-itinerary/generate-map');
  }

  public getFlights(): Observable<FlightsItinerary[]> {
    return this.http.get<FlightsItinerary[]>('api/flight-itinerary/live-flights');
  }
}
