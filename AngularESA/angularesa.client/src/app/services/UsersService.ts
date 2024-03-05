import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { User } from '../Models/users';

@Injectable({
  providedIn: 'root'
})
export class UsersService {
  private apiUrl = '/api/users'; // Update with your actual API endpoint

  constructor(private http: HttpClient) { }

  getUsers(): Observable<User[]> {
    return this.http.get<any>(this.apiUrl).pipe(
      map(response => Object.values(response))
    );
  }

  deleteUser(email: string): Observable<any> {
    const apiUrl = `/api/users/${email}`; // Replace with your actual API endpoint
    return this.http.delete(apiUrl);
  }

}
