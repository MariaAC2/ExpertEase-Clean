import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import { Injectable } from '@angular/core';
import {
  AdminUserUpdateDTO,
  PagedResponse,
  RequestResponse,
  UserAddDTO,
  UserDTO,
  UserUpdateDTO
} from '../models/api.models';
import { jwtDecode } from 'jwt-decode';
import {AuthService, DecodedToken} from './auth.service';

@Injectable({ providedIn: 'root' })
export class AdminUsersService {
  private baseUrl = 'http://localhost:5241/api/admin/users';

  constructor(private http: HttpClient, private authService: AuthService) {}

  getUser(userId: string) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RequestResponse<UserDTO>>(`${this.baseUrl}/${userId}`, {headers});
  }

  getUsers(search: string | undefined, page: number, pageSize: number) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    const params = new HttpParams()
      .set('search', search || '')
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<RequestResponse<PagedResponse<UserDTO>>>(
      `${this.baseUrl}`,
      { headers, params }
    );
  }

  addUser(user: UserAddDTO) {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.post(`${this.baseUrl}`, user, { headers });
  }

  updateUser(userId: string, user: AdminUserUpdateDTO) {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.patch(`${this.baseUrl}/${userId}`, user, { headers });
  }

  deleteUser(userId: string) {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.delete(`${this.baseUrl}/${userId}`, {headers});
  }
}
