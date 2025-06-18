import {HttpClient, HttpHeaders} from '@angular/common/http';
import { Injectable } from '@angular/core';
import {tap} from 'rxjs';
import {LoginDTO, LoginResponseDTO, RequestResponse, SocialLoginDTO, UserRegisterDTO} from '../models/api.models';
import {jwtDecode} from 'jwt-decode';

export interface DecodedToken {
  nameid?: string;
  email?: string;
  role?: string;
  [key: string]: any;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = 'http://localhost:5241/api/Auth';

  constructor(private readonly http: HttpClient) {}

  registerUser(data: UserRegisterDTO) {
    return this.http.post(`${this.baseUrl}/Register`, data);
  }

  loginUser(data: LoginDTO) {
    return this.http.post<RequestResponse<LoginResponseDTO>>(`${this.baseUrl}/Login`, data).pipe(
      tap((result) => {
        const token = result.response?.token;

        console.log("Token:", token);
        if (token) {
          localStorage.setItem('access_token', token);
        }
      })
    );
  }

  socialLogin(data: SocialLoginDTO) {
    return this.http.post<RequestResponse<LoginResponseDTO>>(`${this.baseUrl}/SocialLogin`, data).pipe(
      tap((result) => {
        const token = result.response?.token;

        console.log("Social Login Token:", token);
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

  getUserId(): string | null {
    const decoded = this.decodeToken();
    return decoded?.nameid ?? null;
  }

  setToken(token: string): void {
    if (token) {
      localStorage.setItem('access_token', token);
    }
  }


  logout() {
    localStorage.removeItem('access_token');
  }
}
