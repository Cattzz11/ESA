import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { PaymentModel } from '../Models/PaymentModel';

@Injectable({
  providedIn: 'root',
})
export class SquareService {

  constructor(private http: HttpClient) { }

  purchaseTicket(payment: PaymentModel): Observable<any> {
    // Customize this method based on your API requirements
    console.log("Payload: ", payment);

    return this.http.post('api/payment/purchase-ticket', {
      price: payment.price,
      currency: payment.currency,
      email: payment.email,
      creditCard: payment.creditCard,
      firstName: payment.firstName,
      lastName: payment.lastName,
      shippingAddress: payment.shippingAddress
    }, {
      observe: 'response',
      responseType: 'text'
    })
      .pipe<boolean>(map((res: HttpResponse<string>) => {
        return res.ok;
      }));
  }
}
