import { Carrier } from "./Carrier";
import { City } from "./City";
import { Flight } from "./Flight";

export interface Segment {
  flightNumber: string;
  departure: Date;
  arrival: Date;
  duration: string;
  originCityId?: string;
  originCity: City;
  destinationCityId?: string;
  destinationCity: City;
  flightId?: string;
  flight: Flight;
  carrierId?: string;
  carrier: Carrier;
}
