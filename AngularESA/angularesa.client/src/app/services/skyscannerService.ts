import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FlightData } from '../Models/flight-data';
import { map } from 'rxjs/operators';
import { City } from '../Models/City';
import { Flight } from '../Models/Flight';
import { Carrier } from '../Models/Carrier';
import { Trip } from '../Models/Trip';
import { Country } from '../Models/country';
import { Calendar } from '../Models/Calendar';

@Injectable({
  providedIn: 'root'
})
export class SkyscannerService {
  constructor(private http: HttpClient) { }

  public getRoundtripFlights(data: FlightData): Observable<Flight[]> {
    let params = new HttpParams();

    // Campos obrigatórios
    params = params.set('fromEntityId', data.fromEntityId);
    if (data.toEntityId) params = params.set('toEntityId', data.toEntityId);
    if (data.departDate) params = params.set('departDate', data.departDate);
    if (data.returnDate) params = params.set('returnDate', data.returnDate);

    // Campos opcionais
    if (data.market) params = params.set('market', data.market);
    if (data.locale) params = params.set('locale', data.locale);
    if (data.currency) params = params.set('currency', data.currency);
    if (data.adults) params = params.set('adults', data.adults);
    if (data.children) params = params.set('children', data.children);
    if (data.infants) params = params.set('infants', data.infants);
    if (data.cabinClass) params = params.set('cabinClass', data.cabinClass);

    return this.http.get<Flight[]>('api/search-roundtrip', { params: params });
  }

  public getEverywhereFlights(data: FlightData): Observable<any> {
    let params = new HttpParams();

    // Campos obrigatórios
    params = params.set('fromEntityId', data.fromEntityId);
    
    // Campos opcionais
    if (data.toEntityId) params = params.set('toEntityId', data.toEntityId);
    if (data.year) params = params.set('year', data.year);
    if (data.month) params = params.set('month', data.month);
    if (data.market) params = params.set('market', data.market);
    if (data.locale) params = params.set('locale', data.locale);
    if (data.currency) params = params.set('currency', data.currency);
    if (data.adults) params = params.set('adults', data.adults);
    if (data.children) params = params.set('children', data.children);
    if (data.infants) params = params.set('infants', data.infants);
    if (data.cabinClass) params = params.set('cabinClass', data.cabinClass);

    return this.http.get('api/flight/search-everywhere', { params: params });
  }

  public getOneWayFlights(data: FlightData): Observable<any> {
    let params = new HttpParams();

    // Campos obrigatórios
    params = params.set('fromEntityId', data.fromEntityId);
    if (data.toEntityId) params = params.set('toEntityId', data.toEntityId);
    if (data.departDate) params = params.set('departDate', data.departDate);

    // Campos opcionais
    if (data.market) params = params.set('market', data.market);
    if (data.locale) params = params.set('locale', data.locale);
    if (data.currency) params = params.set('currency', data.currency);
    if (data.adults) params = params.set('adults', data.adults);
    if (data.children) params = params.set('children', data.children);
    if (data.infants) params = params.set('infants', data.infants);
    if (data.cabinClass) params = params.set('cabinClass', data.cabinClass);

    return this.http.get('api/flight/search-one-way', { params: params });
  }

  public getCalendarFlights(data: FlightData): Observable<Calendar[]> {
    let params = new HttpParams();

    // Campos obrigatórios
    params = params.set('fromEntityId', data.fromEntityId);
    if (data.toEntityId) params = params.set('toEntityId', data.toEntityId);

    // Campos opcionais
    if (data.departDate) params = params.set('departDate', data.departDate);
    if (data.market) params = params.set('market', data.market);
    if (data.locale) params = params.set('locale', data.locale);
    if (data.currency) params = params.set('currency', data.currency);

    return this.http.get<Calendar[]>('api/flight/price-calendar', { params: params });
  }

  public getAirportList(data: Country): Observable<City[]> {
    let params = new HttpParams()
      .set('id', data.id)
      .set('name', data.name);

    return this.http.get<City[]>('api/flight/airport-list', { params: params });
  }

  public getSugestionsCompany(carrierId: string): Observable<Trip[]> {
    let params = new HttpParams().set('carrierId', carrierId);
    return this.http.get<Trip[]>('api/flight/sugestions-company', { params: params });
  }

  public getFavoriteAirline(): Observable<Carrier[]> {
    return this.http.get<Carrier[]>('api/flight/favorite-airline');
  }
}
