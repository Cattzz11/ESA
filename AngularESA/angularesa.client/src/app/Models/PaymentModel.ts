import { Trip } from "./Trip";

export interface PaymentModel {
  price: number;
  currency: string;
  email: string;
  creditCard: string;
  firstName: string;
  lastName: string;
  shippingAddress: string;
  trip: Trip;
}
