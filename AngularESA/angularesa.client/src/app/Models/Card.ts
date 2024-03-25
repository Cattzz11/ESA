export interface Card {
  id: string;
  cardBrand: string;
  last4: string;
  expMonth: number;
  expYear: number;
  cardholderName: string;
  billingAddress: {
    postalCode: string;
  };
  fingerprint: string;
}
