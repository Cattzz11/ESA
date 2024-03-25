import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { AircraftData } from "../Models/AircraftData";
import { Injectable } from "@angular/core";

@Injectable({
  providedIn: 'root'
})

export class AeroDataBoxService {
  constructor(private http: HttpClient) { }

  public getAirplaneData(flightIATA: string): Observable<AircraftData> {
    let params = new HttpParams().set('flightIATA', flightIATA);

    return this.http.get<AircraftData>('api/aero-dataBox/flight-status', { params: params });
  }
}
