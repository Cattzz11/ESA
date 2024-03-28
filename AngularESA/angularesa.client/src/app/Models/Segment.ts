import { Carrier } from "./Carrier";
import { City } from "./City";
import { Flight } from "./Flight";

export interface Segment {
  flightNumber: string;
  departure: Date;
  arrival: Date;
  duration: string;
  originCity: City;
  destinationCity: City;
  flight: Flight;
  carrier: Carrier;
}
