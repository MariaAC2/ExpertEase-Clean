import {HttpClient, HttpHeaders} from '@angular/common/http';
import { Injectable } from '@angular/core';
import {
  BecomeSpecialistDTO, BecomeSpecialistResponseDTO,
  RequestResponse,
  SpecialistProfileDTO,
  SpecialistProfileUpdateDTO
} from '../models/api.models';
import {Observable, tap} from 'rxjs';
import {AuthService} from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class SpecialistProfileService {
  private readonly baseUrl = `http://localhost:5241/api/SpecialistProfile`;

  constructor(private readonly http: HttpClient, private readonly authService: AuthService) {}

  becomeSpecialist(specialistProfile: BecomeSpecialistDTO) {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.put<RequestResponse<BecomeSpecialistResponseDTO>>(
      `${this.baseUrl}/BecomeSpecialist`,
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

  getSpecialistProfile(){
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RequestResponse<SpecialistProfileDTO>>(`${this.baseUrl}/Get`, { headers });
  }

  updateSpecialistProfile(dto: SpecialistProfileUpdateDTO) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.patch(`${this.baseUrl}/Update`, dto, {headers});
  }
}
