import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FlightData } from '../Models/flight-data';

@Injectable({
  providedIn: 'root'
})
export class SkyscannerService {
  constructor(private http: HttpClient) { }

  getFlights(data: FlightData): Observable<any> {
    let params = new HttpParams();

    // Campos obrigatórios
    params = params.set('fromEntityId', data.fromEntityId);
    params = params.set('toEntityId', data.toEntityId);
    params = params.set('departDate', data.departDate);

    // Campos opcionais
    if (data.returnDate) params = params.set('returnDate', data.returnDate);
    if (data.market) params = params.set('market', data.market);
    if (data.locale) params = params.set('locale', data.locale);
    if (data.currency) params = params.set('currency', data.currency);
    if (data.adults) params = params.set('adults', data.adults);
    if (data.children) params = params.set('children', data.children);
    if (data.infants) params = params.set('infants', data.infants);
    if (data.cabinClass) params = params.set('cabinClass', data.cabinClass);

    return this.http.get('api/getFlights', { params: params });
  }
}
