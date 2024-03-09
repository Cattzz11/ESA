import { Component } from '@angular/core';
import { User } from '../../Models/users';
import { AccompanyingPassenger } from '../../Models/AccompanyingPassenger';

@Component({
  selector: 'app-payment-component',
  templateUrl: './payment-component.component.html',
  styleUrl: './payment-component.component.css'
})
export class PaymentComponentComponent {
  public user: User | null = null;
  public acomp: AccompanyingPassenger | null = null;
}
