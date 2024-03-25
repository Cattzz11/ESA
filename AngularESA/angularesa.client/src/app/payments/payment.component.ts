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

  async ngOnInit() {
    
    }

}
