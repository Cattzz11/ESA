import { City } from "./City";
import { Segment } from "./Segment";

export interface Flight {
  id: string;
  duration: string;
  departure: Date;
  arrival: Date;
  originCityId?: string;
  originCity: City;
  destinationCityId?: string;
  destinationCity: City;
  segments: Segment[];
  stopCount?: number;
}
