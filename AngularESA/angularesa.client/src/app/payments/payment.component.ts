import { Component, OnInit } from '@angular/core';

declare global {
  interface Window {
    Square: any; // Define the Square object as any type
  }
}
@Component({
  selector: 'app-payment',
  templateUrl: './payment.component.html', // Reference to your HTML template
  styleUrls: ['./payment.component.css']   // Optionally, reference to your CSS file
})
export class PaymentComponent implements OnInit {

  appId = 'sq0idp--WU1GTgsF2Ohg_9LVhZvww';
  locationId = 'L8ERGXW9MT8FA';

  async initializeCard(payments: any) {
    const card = await payments.card();
    await card.attach('#card-container');
    return card;
  }

  async createPayment(token: any, verificationToken: any) {
    const body = JSON.stringify({
      locationId: this.locationId,
      sourceId: token,
      verificationToken,
      idempotencyKey: window.crypto.randomUUID(),
    });

    const paymentResponse = await fetch('/payment', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body,
    });

    if (paymentResponse.ok) {
      return paymentResponse.json();
    }

    const errorBody = await paymentResponse.text();
    throw new Error(errorBody);
  }

  async tokenize(paymentMethod: any) {
    const tokenResult = await paymentMethod.tokenize();
    if (tokenResult.status === 'OK') {
      return tokenResult.token;
    } else {
      let errorMessage = `Tokenization failed with status: ${tokenResult.status}`;
      if (tokenResult.errors) {
        errorMessage += ` and errors: ${JSON.stringify(tokenResult.errors)}`;
      }

      throw new Error(errorMessage);
    }
  }

  async verifyBuyer(payments: any, token: any) {
    const verificationDetails = {
      amount: '1.00',
      billingContact: {
        givenName: 'John',
        familyName: 'Doe',
        email: 'john.doe@square.example',
        phone: '3214563987',
        addressLines: ['123 Main Street', 'Apartment 1'],
        city: 'London',
        state: 'LND',
        countryCode: 'GB',
      },
      currencyCode: 'GBP',
      intent: 'CHARGE',
    };

    const verificationResults = await payments.verifyBuyer(
      token,
      verificationDetails,
    );
    return verificationResults.token;
  }

  displayPaymentResults(status: string) {
    const statusContainer = document.getElementById(
      'payment-status-container',
    );
    if (status === 'SUCCESS') {
      statusContainer?.classList.remove('is-failure');
      statusContainer?.classList.add('is-success');
    } else {
      statusContainer?.classList.remove('is-success');
      statusContainer?.classList.add('is-failure');
    }

    //statusContainer.style.visibility = 'visible';
  }

  async ngOnInit() {
    if (!window.Square) {
      throw new Error('Square.js failed to load properly');
    }

    let payments:any;
    try {
      payments = window.Square.payments(this.appId, this.locationId);
    } catch {
      const statusContainer = document.getElementById(
        'payment-status-container',
      );
      //statusContainer.className = 'missing-credentials';
     // statusContainer.style.visibility = 'visible';
      return;
    }

    let card:any;
    try {
      card = await this.initializeCard(payments);
    } catch (e) {
      console.error('Initializing Card failed', e);
      return;
    }

    const cardButton = document.getElementById('card-button');
   /* cardButton.addEventListener('click', async (event) => {
      event.preventDefault();

      try {
        // disable the submit button as we await tokenization and make a payment request.
        //cardButton.disabled = true;
        const token = await this.tokenize(card);
        const verificationToken = await this.verifyBuyer(payments, token);
        const paymentResults = await this.createPayment(token, verificationToken);
        this.displayPaymentResults('SUCCESS');

        console.debug('Payment Success', paymentResults);
      } catch (e) {
       // cardButton.disabled = false;
        this.displayPaymentResults('FAILURE');
        console.error(e.message);
      }
    });*/
  }

}
