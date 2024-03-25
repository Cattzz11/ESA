import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { PaymentModel } from '../Models/PaymentModel';
import { Card } from '../Models/Card';
import { CardID } from '../Models/CardID';
import { User } from '../Models/users';
import { PaymentHistoryModel } from '../Models/PaymentHistoryModel';

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
    });
  }

  getCustomerCards(customerEmail: string): Observable<Card[]> {
    return this.http.get<Card[]>(`api/payment/get-cards/${customerEmail}`).pipe(
      map((response: Card[]) => {
        // Perform any data transformations here if needed
        return response;
      })
    );
  }

  payNow(card: CardID): Observable<any> {
    console.log("Card ID:", card);
    return this.http.post(`api/payment/pay-now`, { customerCardID: card.cardID }, { observe: 'response', responseType: 'text' })
      .pipe<boolean>(map((res: HttpResponse<string>) => {
        console.log(res);
        return res.ok;
      }));
  }

  premiumSubscription(card: CardID): Observable<any> {
    console.log("Card ID:", card);
    return this.http.post(`api/payment/premium-subscription`, { customerCardID: card.cardID }, { observe: 'response', responseType: 'text' })
      .pipe<boolean>(map((res: HttpResponse<string>) => {
        console.log(res);
        return res.ok;
      }));
  }

  cancelPremiumSubscription(card: CardID): Observable<any> {
    console.log("Card ID:", card);
    return this.http.post(`api/payment/cancel-subscription`, { customerCardID: card.cardID }, { observe: 'response', responseType: 'text' })
      .pipe<boolean>(map((res: HttpResponse<string>) => {
        console.log(res);
        return res.ok;
      }));
  }

  fetchPaymentHistory(user: User): Observable<any> {
    console.log("User email:", user.email);
    return this.http.get<PaymentHistoryModel[]>(`api/payment/get-user-payments/${user.email}`).pipe(
      map((response: PaymentHistoryModel[]) => {
        // Perform any data transformations here if needed
        return response;
      })
    );
  }

}
