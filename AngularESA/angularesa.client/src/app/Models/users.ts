import { AccompanyingPassenger } from "./AccompanyingPassenger";
import { UserFlight } from "./UserFlight";

export interface User {
  id: string;
  userName: string;
  email: string;
  role: number;
  name: string;
  age?: number; 
  nationality?: string; 
  occupation?: string;
  gender?: string
  userFlights?: UserFlight[];
}
