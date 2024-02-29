import { Segment } from "./Segment";

export interface Carrier {
  id: string;
  name: string;
  logoURL?: string;
  searchTimes: number;
}
