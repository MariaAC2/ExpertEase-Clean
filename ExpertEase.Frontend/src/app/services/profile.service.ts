import { Injectable } from '@angular/core';
import {AuthService} from "./auth.service";
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {
  BecomeSpecialistDTO, BecomeSpecialistResponseDTO,
  PagedResponse,
  RequestResponse,
  UserDTO,
  UserUpdateDTO
} from "../models/api.models";
import {tap} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private baseUrl = 'http://localhost:5241/api/profile/user';
  constructor(private authService: AuthService, private http: HttpClient) { }
  getUserProfile() {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RequestResponse<UserDTO>>(`${this.baseUrl}`, {headers});
  }

  updateUserProfile(user: UserUpdateDTO) {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.put(`${this.baseUrl}`, user, {headers});
  }

  becomeSpecialist(specialistProfile: BecomeSpecialistDTO) {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.put<RequestResponse<BecomeSpecialistResponseDTO>>(
      `${this.baseUrl}become_specialist`,
      specialistProfile,
      { headers }
    ).pipe(
      tap((res) => {
        if (res.response?.token) {
          // 1. Save new token
          localStorage.setItem('access_token', res.response.token);
        }
      })
    );
  }

  logout() {
    localStorage.removeItem('access_token');
  }

  getCurrentUserId(): string {
    const userId = this.authService.decodeToken()?.nameid; // or decode JWT

    if (!userId) {
      throw new Error('User ID not found in token');
    }

    return userId;
  }
}
