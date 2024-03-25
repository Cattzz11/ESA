import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { SquareService } from '../../../services/SquareService';
import { Card } from '../../../Models/Card';
import { CardID } from '../../../Models/CardID';
import { User } from '../../../Models/users';
import { AuthorizeService } from '../../../../api-authorization/authorize.service';
import { FormArray } from '@angular/forms';
import { toArray } from 'rxjs';

@Component({
  selector: 'app-PopUpCancel',
  templateUrl: './PopUpCancel.component.html',
  styleUrls: ['./PopUpCancel.component.css']
})
export class PopUpCancelPremiumComponent implements OnInit {
  public user: User | null = null;
  public cards: Card[] = [];
  public isLoading: boolean = true;
  public selectedCard: string | undefined;
  transactionCompleted: boolean = false;
  public isPaymentProcessing: boolean = false;

  constructor(
    private squareService: SquareService,
    private auth: AuthorizeService,
    public dialogRef: MatDialogRef<PopUpCancelPremiumComponent>
  ) { }

  ngOnInit(): void {
    this.fetchUserData();
  }

  private fetchUserData(): void {
    const storedUser = sessionStorage.getItem('user');
    if (storedUser) {
      this.user = JSON.parse(storedUser);
      this.fetchCustomerCards();
    } else {
      this.auth.getUserInfo().subscribe({
        next: (userInfo: User) => {
          this.user = userInfo;
          this.fetchCustomerCards();
        },
        error: (error) => {
          console.error('Error fetching user info', error);
          this.isLoading = false;
        }
      });
    }
  }

  private fetchCustomerCards(): void {
    if (!this.user || !this.user.email) {
      console.error('User or user email is not available');
      this.isLoading = false;
      return;
    }

    this.squareService.getCustomerCards(this.user.email).subscribe({
      next: (cards: Card[]) => {
        if (cards) {
          var arrayCard = Object.values(cards);
          var createCard: Card = {
            id: '',
            cardBrand: '',
            last4: '',
            expMonth: 0,
            expYear: 0,
            cardholderName: '',
            billingAddress: {
              postalCode: ''
            },
            fingerprint: ''
          };

          createCard.id = arrayCard[0].toString();
          createCard.cardBrand = arrayCard[1].toString();
          createCard.last4 = arrayCard[2].toString();
          createCard.expMonth = Number(arrayCard[3]);
          createCard.expYear = Number(arrayCard[4]);
          createCard.cardholderName = arrayCard[5].toString();
          createCard.billingAddress.postalCode = Object.values(arrayCard[6])[0];
          createCard.fingerprint = arrayCard[7].toString();

          this.cards.push(createCard);

          console.log('Customer cards of response:', cards);
          console.log('Customer cards:', this.cards);
          this.isLoading = false;

        }
        
      },
      error: (error) => {
        console.error('Error fetching customer cards:', error);
        this.isLoading = false;
      }
    });

  }

  cancelPremiumSubscription(selectedCard?: string): void {
    if (selectedCard) {
      let newCard: CardID = { cardID: selectedCard.toString() }
      this.isPaymentProcessing = true;
      //selectedCard.toString()
      if (newCard) {
        this.squareService.cancelPremiumSubscription(newCard).subscribe(
          (response) => {
            console.log('Payment response:', response);
            if (response === true) {
              // Payment successful
              console.log('Cancelation Successful successful');
              this.transactionCompleted = true;
              setTimeout(() => {
                this.dialogRef.close();
              }, 2000);
              setTimeout(() => {
                this.isPaymentProcessing = false;
              }, 2000);
              
            } else {  
              // Payment failed
              console.error('Payment failed');
              this.isPaymentProcessing = false;
            }
          },
          (error) => {
            console.error('Payment failed:', error);
            this.isPaymentProcessing = false; 
          }
        );
      }
    }

    
  }



}
