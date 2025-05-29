import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import { Injectable } from '@angular/core';
import {
  PagedResponse,
  RequestResponse,
  UserAddDTO,
  UserDTO, SpecialistAddDTO,
  SpecialistDTO, SpecialistUpdateDTO,
  UserUpdateDTO
} from '../models/api.models';
import { jwtDecode } from 'jwt-decode';
import {AuthService, DecodedToken} from './auth.service';

@Injectable({ providedIn: 'root' })
export class AdminSpecialistsService {
  private baseUrl = 'http://localhost:5241/api/admin/specialists';

  constructor(private http: HttpClient, private authService: AuthService) {}

  getSpecialist(userId: string) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RequestResponse<SpecialistDTO>>(`${this.baseUrl}/${userId}`, {headers});
  }

  getSpecialists(search: string | undefined, page: number, pageSize: number) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    const params = new HttpParams()
      .set('search', search || '')
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<RequestResponse<PagedResponse<SpecialistDTO>>>(
      `${this.baseUrl}`,
      { headers, params }
    );
  }

  addSpecialist(user: SpecialistAddDTO) {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.post(`${this.baseUrl}`, user, { headers });
  }

  updateSpecialist(userId: string, user: SpecialistUpdateDTO) {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.patch(`${this.baseUrl}/${userId}`, user, { headers });
  }

  deleteSpecialist(userId: string) {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.delete(`${this.baseUrl}/${userId}`, {headers});
  }
}
