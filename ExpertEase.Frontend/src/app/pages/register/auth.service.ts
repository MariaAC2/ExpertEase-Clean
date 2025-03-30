import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = 'http://localhost:5241/api';

  constructor(private http: HttpClient) {}

  registerUser(data: any) {
    console.log(data);
    return this.http.post(`${this.baseUrl}/Users/register`, data);
  }
}
