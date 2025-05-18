import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {tap} from 'rxjs';
import {LoginDTO, LoginResponseDTO, RequestResponse, UserRegisterDTO} from '../models/api.models';
import {jwtDecode} from 'jwt-decode';

export interface DecodedToken {
  userId?: string;
  email?: string;
  role?: string;
  [key: string]: any;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = 'http://localhost:5241/api/auth';

  constructor(private http: HttpClient) {}

  registerUser(data: UserRegisterDTO) {
    console.log(data);
    return this.http.post(`${this.baseUrl}/register`, data);
  }

  loginUser(data: LoginDTO) {
    console.log(data);
    return this.http.post<RequestResponse<LoginResponseDTO>>(`${this.baseUrl}/login`, data).pipe(
      tap((result) => {
        const token = result.response?.token;

        console.log("Token:", token);

        if (token) {
          localStorage.setItem('access_token', token);
        }
      })
    );
  }

  getToken(): string | null {
    return localStorage.getItem('access_token');
  }

  decodeToken(): DecodedToken | null {
    const token = this.getToken()
    if (!token) return null;

    try {
      return jwtDecode<DecodedToken>(token);
    } catch (e) {
      console.error('Invalid token', e);
      return null;
    }
  }

  getUserRole(): string | null {
    const decoded = this.decodeToken();
    return decoded?.role ?? null;
  }
}
