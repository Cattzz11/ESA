import { Flight } from "./Flight";

export interface Segment {
  flightNumber: string;
  departure: Date;
  arrival: Date;
  duration: string;
  flightId: string;
  flight: Flight;
  carrierId: string;
}
