import { Carrier } from "./Carrier";
import { City } from "./City";
import { Flight } from "./Flight";

export interface Segment {
  flightNumber: string;
  departure: Date;
  arrival: Date;
  duration: string;
  flightId: string;
  flight: Flight;
  carrierId: string;
  carrier: Carrier;
  originCityId: string;
  originCity: City;
  destinationCityId: string;
  destinationCity: City;
}
