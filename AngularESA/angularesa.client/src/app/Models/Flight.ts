import { City } from "./City";
import { Segment } from "./Segment";

export interface Flight {
  id: string;
  duration: string;
  departure: Date;
  arrival: Date;
  direction: string;
  originCity: City;
  destinationCity: City;
  segments: Segment[];
}
