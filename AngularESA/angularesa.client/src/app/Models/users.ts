export interface User {
  userName: string;
  email: string;
  role: string;
  name: string;
  age?: number; 
  nationality?: string; 
  occupation?: string;
  profilePicture?: string;
  gender?: string;
  //profilePictureBinary?: Uint8Array;
}
