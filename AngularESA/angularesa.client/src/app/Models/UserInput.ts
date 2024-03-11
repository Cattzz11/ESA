export class UserInputModel {
  email: string;
  creditCard: string;
  firstName: string;
  lastName: string;
  shippingAddress: string;

  constructor() {
    // You can initialize properties if needed
    this.email = '';
    this.creditCard = '';
    this.firstName = '';
    this.lastName = '';
    this.shippingAddress = '';
  }
}
