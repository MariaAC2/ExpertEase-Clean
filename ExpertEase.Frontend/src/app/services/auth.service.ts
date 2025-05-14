import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {tap} from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = 'http://localhost:5241/api';

  constructor(private http: HttpClient) {}

  registerUser(data: any) {
    console.log(data);
    return this.http.post(`${this.baseUrl}/auth/register`, data);
  }

  loginUser(data: any) {
    console.log(data);
    return this.http.post(`${this.baseUrl}/auth/login`, data).pipe(
      tap((result: any) => {
        const token = result.response?.token;

        if (token) {
          localStorage.setItem('access_token', token);
        }
      })
    );
  }
}
