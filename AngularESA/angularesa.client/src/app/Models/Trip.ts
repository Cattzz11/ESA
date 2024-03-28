import { Flight } from "./Flight";
import { PriceOptions } from "./PriceOptions";
import { UserFlight } from "./UserFlight";

export interface Trip {
  id: string;
  price?: number;
  flights: Flight[];
  priceOptions?: PriceOptions[];
}
