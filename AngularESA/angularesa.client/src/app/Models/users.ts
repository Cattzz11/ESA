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
  profilePicture?: string;
  gender?: string;
  //profilePictureBinary?: Uint8Array;
  userFlights?: UserFlight[];
}
