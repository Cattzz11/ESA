import { City } from "./City";

export interface Country {
  id: string;
  name: string;
  countries?: City[];
}
