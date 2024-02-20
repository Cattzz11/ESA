export interface FlightData {
  fromEntityId: string;
  toEntityId?: string;
  departDate?: string;
  returnDate?: string;
  market?: string;
  locale?: string;
  currency?: string;
  adults?: number;
  children?: number;
  infants?: number;
  cabinClass?: string;
  year?: number;
  month?: number;
}
