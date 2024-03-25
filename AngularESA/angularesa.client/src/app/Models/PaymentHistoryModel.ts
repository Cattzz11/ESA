import { Trip } from "./Trip";

export interface PaymentHistoryModel {
  price: number;
  currency: string;
  email: string;
  creditCard: string;
  firstName: string;
  lastName: string;
  shippingAddress: string;
  trip: Trip;
  status: string;
}
