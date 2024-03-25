import { Component } from '@angular/core';
import { User } from '../../Models/users';
import { AccompanyingPassenger } from '../../Models/AccompanyingPassenger';
import { Trip } from '../../Models/Trip';
import { AuthorizeService } from '../../../api-authorization/authorize.service';
import { FormBuilder } from '@angular/forms';
import { UsersService } from '../../services/UsersService';
import { Router } from '@angular/router';
import { SquareService } from '../../services/SquareService';
import { PaymentModel } from '../../Models/PaymentModel';
import { PaymentHistoryModel } from '../../Models/PaymentHistoryModel';

@Component({
  selector: 'app-payment-component',
  templateUrl: './payment-component.component.html',
  styleUrl: './payment-component.component.css'
})
export class PaymentComponentComponent {
  public user: User | null = null;
  public history: Trip[] = [];
  public historyLoading: boolean = true;
  public usersPayments: PaymentHistoryModel[] = [];
  public processPayments: PaymentHistoryModel[] = [];
  public canceledPayments: PaymentHistoryModel[] = [];
  public completedPayments: PaymentHistoryModel[] = [];

  constructor(
    private auth: AuthorizeService,
    private userService: UsersService,
    private router: Router,
    public squareService: SquareService
  ) {

  }

  ngOnInit() {
    const storedUser = sessionStorage.getItem('user');
    if (storedUser) {
      this.user = JSON.parse(storedUser);
    }
    else {
      this.auth.getUserInfo().subscribe({
        next: (userInfo: User) => {
          this.user = userInfo;
          this.fetchPaymentHistory();

        },
        error: (error) => {
          console.error('Error fetching user info', error);
        }
      });
    }

  }


  fetchPaymentHistory()
  {
    if (this.user != null) {
      this.squareService.fetchPaymentHistory(this.user).subscribe(
        (paymentList: PaymentHistoryModel[]) => {
          console.log('Payment list:', paymentList);
          this.usersPayments = paymentList;
          this.orderPayments(this.usersPayments);
        },
        (error) => {
          // Handle error if any
          console.error('Error fetching payment history:', error);
        }
      );
    }
      
  }
    orderPayments(paymentsOrder: PaymentHistoryModel[]) {
      paymentsOrder.forEach(payment => {
        if (payment.status === 'CANCELED') {
          this.canceledPayments.push(payment);
        } else if (payment.status === 'APPROVED') {
          this.processPayments.push(payment);
        } else if (payment.status === 'COMPLETED') {
          this.completedPayments.push(payment);
        }
        // You can add more conditions for other statuses if needed
      });

      console.log("PROCESSED: ", this.processPayments);
    }


}
