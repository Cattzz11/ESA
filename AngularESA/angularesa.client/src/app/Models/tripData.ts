export interface Itinerary {
  id: string;
  price: number;
  trip: Trip[];
  isSelfTransfer: boolean;
  isProtectedSelfTransfer: boolean;
  isChangeAllowed: boolean;
  isPartiallyChangeable: boolean;
  isCancellationAllowed: boolean;
  isPartiallyRefundable: boolean;
  score: number;
}

interface Trip {
  id: string;
  origin: Place;
  destination: Place;
  durationInMinutes: number;
  stopCount: number;
  departure: Date;
  arrival: Date;
  timeDeltaInDays: number;
  carriers: Carrier[];
  segments: Segment[];
}

interface Place {
  id: string;
  name: string;
  city?: string;
  country?: string;
}

interface Carrier {
  id: number;
  logoUrl?: string;
  name: string;
}

interface Segment {
  origin: Place;
  destination: Place;
  departure: Date;
  arrival: Date;
  durationInMinutes: number;
  flightNumber: string;
  carrier: Carrier;
}
