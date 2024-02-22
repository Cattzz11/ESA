import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FlightData } from '../Models/flight-data';
import { Country } from '../Models/country';
import { map } from 'rxjs/operators';
import { Airport } from '../Models/Airport';
import { Itinerary } from '../Models/tripData';
import { AirLine } from '../Models/AirLine';

@Injectable({
  providedIn: 'root'
})
export class SkyscannerService {
  constructor(private http: HttpClient) { }

  public getRoundtripFlights(data: FlightData): Observable<Itinerary[]> {
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

    return this.http.get<Itinerary[]>('api/search-roundtrip', { params: params });
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

    return this.http.get('api/search-everywhere', { params: params });
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

    return this.http.get('api/search-one-way', { params: params });
  }

  public getCalendarFlights(data: FlightData): Observable<any> {
    let params = new HttpParams();

    // Campos obrigatórios
    params = params.set('fromEntityId', data.fromEntityId);
    if (data.departDate) params = params.set('departDate', data.departDate);

    // Campos opcionais
    if (data.toEntityId) params = params.set('toEntityId', data.toEntityId);
    if (data.market) params = params.set('market', data.market);
    if (data.locale) params = params.set('locale', data.locale);
    if (data.currency) params = params.set('currency', data.currency);

    return this.http.get('api/price-calendar', { params: params });
  }

  public getAirportList(data: Country): Observable<Airport[]> {
    let params = new HttpParams()
      .set('id', data.id)
      .set('name', data.name);

    return this.http.get<Airport[]>('api/airport-list', { params: params });
  }

  public getSugestionsCompany(carrierName: string): Observable<Itinerary[]> {
    let params = new HttpParams().set('carrierName', carrierName);
    return this.http.get<Itinerary[]>('api/sugestions-company', { params: params });
  }

  public getFavoriteAirline(): Observable<AirLine[]> {
    return this.http.get<AirLine[]>('api/favorite-airline');
  }
  
}
