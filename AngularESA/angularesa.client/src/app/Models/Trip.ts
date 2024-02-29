import { Flight } from "./Flight";
import { UserFlight } from "./UserFlight";

export interface Trip {
  id: string;
  price: number;
  isSelfTransfer: boolean;
  isProtectedSelfTransfer: boolean;
  isChangeAllowed: boolean;
  isPartiallyChangeable: boolean;
  isCancellationAllowed: boolean;
  isPartiallyRefundable: boolean;
  score: number;
  flights: Flight[];
}
