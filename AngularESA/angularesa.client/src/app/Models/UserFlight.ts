import { AccompanyingPassenger } from "./AccompanyingPassenger";
import { Trip } from "./Trip";
import { User } from "./users";

export interface UserFlight {
  userId: string;
  tripId: string;
  user: User;
  trip: Trip;
  accompanyingPassengers?: AccompanyingPassenger[];
}
