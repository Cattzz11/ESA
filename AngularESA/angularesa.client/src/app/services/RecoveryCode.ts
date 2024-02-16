import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class RecoveryCode {
  constructor(private http: HttpClient) { }

  sendRecoveryCode(email: string) {
    return this.http.post('/api/passwordrecovery', email);
  }
}
