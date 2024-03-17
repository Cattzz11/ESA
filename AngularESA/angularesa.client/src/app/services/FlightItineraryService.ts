import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class FlightItineraryService {
  constructor(private http: HttpClient) { }

  public getMap(): Observable<any> {
    console.log("AQUI ANGULAR!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

    return this.http.get('api/flight-itinerary/generate-map');
  }
}
