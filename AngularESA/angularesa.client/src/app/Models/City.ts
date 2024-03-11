import { Country } from "./country";

export interface City {
  id: string;
  name: string;
  apiKey?: string;
  countryId?: string;
  country: Country;
  displayName?: string;
}
