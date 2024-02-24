import { City } from "./City";
import { Segment } from "./Segment";
import { UserFlight } from "./UserFlight";

export interface Flight {
  id: string;
  price: number;
  duration: string;
  departure: Date;
  arrival: Date;
  isSelfTransfer: boolean;
  isProtectedSelfTransfer: boolean;
  isChangeAllowed: boolean;
  isPartiallyChangeable: boolean;
  isCancellationAllowed: boolean;
  isPartiallyRefundable: boolean;
  score: number;
  segments: Segment[];
  originCityId: string;
  originCity: City;
  destinationCityId: string;
  destinationCity: City;
  userFlights: UserFlight[];
}
