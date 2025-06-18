import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import { Injectable } from '@angular/core';
import {
  AdminUserUpdateDTO,
  PagedResponse,
  RequestResponse, SpecialistDTO,
  UserAddDTO, UserDetailsDTO,
  UserDTO, UserPaymentDetailsDTO,
  UserUpdateDTO, UserUpdateResponseDTO
} from '../models/api.models';
import { jwtDecode } from 'jwt-decode';
import {AuthService, DecodedToken} from './auth.service';
import {Observable} from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly baseUrl = 'http://localhost:5241/api/User';
  constructor(private readonly http: HttpClient, private readonly authService: AuthService) {}
  getUser(userId: string) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RequestResponse<UserDTO>>(`${this.baseUrl}/GetById/${userId}`, {headers});
  }

  getUserProfile() {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RequestResponse<UserDTO>>(`${this.baseUrl}/GetProfile`, {headers});
  }

  getUserDetails(userId: string) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RequestResponse<UserDetailsDTO>>(`${this.baseUrl}/GetDetails/${userId}`, {headers});
  }

  getUserPaymentDetails(userId: string) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RequestResponse<UserPaymentDetailsDTO>>(`${this.baseUrl}/GetPaymentDetails/${userId}`, {headers});
  }

  getUsers(search: string | undefined, page: number, pageSize: number) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    const params = new HttpParams()
      .set('search', search ?? '')
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<RequestResponse<PagedResponse<UserDTO>>>(
      `${this.baseUrl}/GetPage`,
      { headers, params }
    );
  }

  addUser(user: UserAddDTO) {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.post(`${this.baseUrl}/Add`, user, { headers });
  }

  updateUser(userId: string, user: AdminUserUpdateDTO) {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.patch(`${this.baseUrl}/Update/${userId}`, user, { headers });
  }

  updateUserProfile(user: UserUpdateDTO): Observable<RequestResponse<UserUpdateResponseDTO>> {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.patch(`${this.baseUrl}/Update`, user, {headers});
  }

  deleteUser(userId: string) {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.delete(`${this.baseUrl}/Delete/${userId}`, {headers});
  }
}
