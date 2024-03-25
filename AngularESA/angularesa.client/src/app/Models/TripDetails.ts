import { Flight } from "./Flight";
import { PriceOptions } from "./PriceOptions";

export interface TripDetails {
  id: string;
  destinationImage: string;
  flights: Flight[];
  priceOptions: PriceOptions[];
}
