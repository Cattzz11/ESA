import { Carrier } from "./Carrier";
import { City } from "./City";

export interface FlightsItinerary {
  flightIATA: string;
  flightICAO: string;
  flightStatus: string;
  departureLocation: City;
  departureSchedule: Date;
  arrivalLocation: City;
  arrivalSchedule: Date;
  airline: Carrier;
}
