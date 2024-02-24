import { AccompanyingPassenger } from "./AccompanyingPassenger";
import { Flight } from "./Flight";
import { User } from "./users";

export interface UserFlight {
  userId: string;
  flightId: string;
  user: User;
  flight: Flight;
  accompanyingPassengers?: AccompanyingPassenger;
}
