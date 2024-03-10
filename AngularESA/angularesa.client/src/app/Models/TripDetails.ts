import { City } from "./City";
import { Segment } from "./Segment";

export interface TripDetails {
  id: string;
  departureDate: Date;
  departureTime: string;
  originCity: string;
  duration: string;
  arrivalDate: Date;
  arrivalTime: string;
  destinationCity: string;
  price: number;
}
