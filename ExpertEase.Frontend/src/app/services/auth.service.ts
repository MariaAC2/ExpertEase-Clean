import {HttpClient, HttpHeaders} from '@angular/common/http';
import { Injectable } from '@angular/core';
import {tap, Observable} from 'rxjs';
import {LoginDTO, LoginResponseDTO, RequestResponse, SocialLoginDTO, UserRegisterDTO} from '../models/api.models';
import {jwtDecode} from 'jwt-decode';

export interface DecodedToken {
  // Custom claims from your backend
  nameid?: string;
  email?: string;
  role?: string;

  // Standard JWT claims
  exp?: number;        // Expiration time (Unix timestamp)
  iat?: number;        // Issued at time (Unix timestamp)
  nbf?: number;        // Not before time (Unix timestamp)
  sub?: string;        // Subject (usually user ID)
  iss?: string;        // Issuer
  aud?: string;        // Audience
  jti?: string;        // JWT ID (unique identifier for the token)

  // Allow any other claims
  [key: string]: any;
}

export interface OAuthCodeExchangeDTO {
  code: string;
  provider: string;
  redirectUri: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = 'http://localhost:5241/api';
  private readonly authUrl = `${this.baseUrl}/Auth`;

  constructor(private readonly http: HttpClient) {}

  registerUser(data: UserRegisterDTO): Observable<any> {
    return this.http.post(`${this.authUrl}/Register`, data);
  }

  loginUser(data: LoginDTO): Observable<RequestResponse<LoginResponseDTO>> {
    return this.http.post<RequestResponse<LoginResponseDTO>>(`${this.authUrl}/Login`, data).pipe(
      tap((result) => {
        const token = result.response?.token;

        console.log("Login Token:", token);
        if (token) {
          localStorage.setItem('access_token', token);
        }
      })
    );
  }

  socialLogin(data: SocialLoginDTO): Observable<RequestResponse<LoginResponseDTO>> {
    return this.http.post<RequestResponse<LoginResponseDTO>>(`${this.authUrl}/SocialLogin`, data).pipe(
      tap((result) => {
        const token = result.response?.token;

        console.log("Social Login Token:", token);
        if (token) {
          localStorage.setItem('access_token', token);
        }
      })
    );
  }

  // NEW: OAuth Code Exchange Method
  exchangeOAuthCode(data: OAuthCodeExchangeDTO): Observable<RequestResponse<LoginResponseDTO>> {
    console.log('Sending OAuth code exchange request:', data);

    return this.http.post<RequestResponse<LoginResponseDTO>>(`${this.authUrl}/ExchangeOAuthCode`, data).pipe(
      tap((result) => {
        const token = result.response?.token;

        console.log("OAuth Exchange Token:", token);
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

  // Check if user is authenticated
  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const decoded = this.decodeToken();
      if (!decoded) return false;

      // Check if token is expired
      const currentTime = Date.now() / 1000;
      if (decoded.exp && decoded.exp < currentTime) {
        this.logout();
        return false;
      }

      return true;
    } catch (error) {
      console.error('Error checking authentication:', error);
      this.logout();
      return false;
    }
  }

  // Clear stored token and any auth-related data
  logout(): void {
    localStorage.removeItem('access_token');

    // Clear any OAuth-related data
    sessionStorage.removeItem('google_oauth_state');
    sessionStorage.removeItem('google_oauth_redirect');
    sessionStorage.removeItem('google_oauth_type');
    localStorage.removeItem('google_auth_code');
    localStorage.removeItem('google_auth_state');
    localStorage.removeItem('google_auth_error');
    localStorage.removeItem('google_auth_code_redirect');
    localStorage.removeItem('google_auth_type');
  }

  // Get user info from token
  getCurrentUserInfo(): { id: string; email: string; role: string } | null {
    const decoded = this.decodeToken();
    if (!decoded || !decoded.nameid) return null;

    return {
      id: decoded.nameid,
      email: decoded.email || '',
      role: decoded.role || ''
    };
  }
}
