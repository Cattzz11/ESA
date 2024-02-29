import { Observable } from "rxjs";
import { Trip } from "../Models/Trip";
import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { City } from "../Models/City";


@Injectable({
  providedIn: 'root'
})
export class DataService {
  constructor(private http: HttpClient) { }

  public getFlightsByUser(userId: string): Observable<Trip[]> {
    let params = new HttpParams().set('userId', userId);
    return this.http.get<Trip[]>('api/data/flight-by-user', { params: params });
  }

  public getAllCities(): Observable<City[]> {
    return this.http.get<City[]>('api/data/all-cities-and-countries');
  }
}
