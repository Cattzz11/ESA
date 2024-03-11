import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SquareService {
  //private apiUrl = `/api/trip-details/${tripId}`;

  constructor(private http: HttpClient) { }

  purchaseTicket(userInput: any, price: number, tripId: string): Observable<any> {
    // Customize this method based on your API requirements
    const payload = { userInput, price, tripId };
    console.log(payload);
    return this.http.post<any>(`/api/purchase-ticket/${tripId}`, payload);
  }
}
