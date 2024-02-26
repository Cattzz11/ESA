import { Flight } from "./Flight";

export interface City {
  id: string;
  name: string;
  apiKey: string;
  countryId: string;
  flights?: Flight[];
}
