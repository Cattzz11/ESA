import { Country } from "./country";

export interface City {
  id: string;
  name: string;
  apiKey: string;
  country: Country;
  coordenates?: string;
}
