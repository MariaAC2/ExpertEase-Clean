import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {tap} from 'rxjs';
import {LoginDTO, LoginResponseDTO, UserRegisterDTO} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = 'http://localhost:5241/api';

  constructor(private http: HttpClient) {}

  registerUser(data: UserRegisterDTO) {
    console.log(data);
    return this.http.post(`${this.baseUrl}/auth/register`, data);
  }

  loginUser(data: LoginDTO) {
    console.log(data);
    return this.http.post<LoginResponseDTO>(`${this.baseUrl}/auth/login`, data).pipe(
      tap((result: LoginResponseDTO) => {
        const token = result.token;

        if (token) {
          localStorage.setItem('access_token', token);
        }
      })
    );
  }
}
