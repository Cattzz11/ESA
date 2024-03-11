import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PaymentModel } from '../Models/PaymentModel';

@Injectable({
  providedIn: 'root',
})
export class SquareService {

  constructor(private http: HttpClient) { }

  purchaseTicket(payment: PaymentModel, tripId: string): Observable<any> {
    // Customize this method based on your API requirements
    payment.tripId = tripId;
    console.log("Payload: ",payment);
    return this.http.post<PaymentModel>(`/api/trip-details/purchase-ticket`, payment);
  }

}
