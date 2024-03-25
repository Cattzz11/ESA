import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { FlightsItinerary } from "../Models/FlightsItinerary";
import { AddressComponents } from "../flights/map/map.component";
import { Trip } from "../Models/Trip";
import { TripDetails } from "../Models/TripDetails";

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

  public getTrips(origin: AddressComponents, destination: AddressComponents): Observable<Trip[]> {
    let params = new HttpParams()
      .set('origin.city', origin.city)
      .set('origin.country', origin.country)
      .set('origin.latitude', origin.latitude)
      .set('origin.longitude', origin.longitude)
      .set('destination.city', destination.city)
      .set('destination.country', destination.country)
      .set('destination.latitude', destination.latitude)
      .set('destination.longitude', destination.longitude);

    return this.http.get<Trip[]>('api/flight-itinerary/search-flights', { params });
  }

  public getTripsPremium(origin: AddressComponents, destination: AddressComponents): Observable<TripDetails[]> {
    let params = new HttpParams()
      .set('origin.city', origin.city)
      .set('origin.country', origin.country)
      .set('origin.latitude', origin.latitude)
      .set('origin.longitude', origin.longitude)
      .set('destination.city', destination.city)
      .set('destination.country', destination.country)
      .set('destination.latitude', destination.latitude)
      .set('destination.longitude', destination.longitude);

    console.log("Angular Service");

    return this.http.get<TripDetails[]>('api/flight-itinerary/search-flights-premium', { params });
  }
}
