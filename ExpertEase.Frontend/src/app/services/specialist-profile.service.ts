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

  becomeSpecialist(specialistData: any, portfolioFiles: File[]): Observable<RequestResponse<BecomeSpecialistResponseDTO>> {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    const formData = new FormData();

    // Add all the text/primitive data
    formData.append('UserId', specialistData.userId);
    formData.append('PhoneNumber', specialistData.phoneNumber);
    formData.append('Address', specialistData.address);
    formData.append('YearsExperience', specialistData.yearsExperience.toString());
    formData.append('Description', specialistData.description);

    // Add categories if any
    if (specialistData.categories && specialistData.categories.length > 0) {
      specialistData.categories.forEach((categoryId: string, index: number) => {
        formData.append(`Categories[${index}]`, categoryId);
      });
    }

    // Add portfolio photos
    if (portfolioFiles && portfolioFiles.length > 0) {
      portfolioFiles.forEach((file) => {
        formData.append('PortfolioPhotos', file, file.name);
      });
    }

    return this.http.put<RequestResponse<BecomeSpecialistResponseDTO>>(
      `${this.baseUrl}/BecomeSpecialist`,
      specialistData,
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

  updateSpecialistProfile(formData: FormData) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.patch(`${this.baseUrl}/Update`, formData, {headers});
  }
}
