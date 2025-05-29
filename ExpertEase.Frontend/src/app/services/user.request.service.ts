import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import {AuthService} from './auth.service';
import {
  AdminUserUpdateDTO,
  PagedResponse, RequestAddDTO,
  RequestDTO,
  RequestResponse,
  UserAddDTO,
  UserDTO
} from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class UserRequestService {
  private baseUrl = 'http://localhost:5241/api/user/requests';
  constructor(private http: HttpClient, private authService: AuthService) { }

  // getRequest(userId: string) {
  //   const token = this.authService.getToken();
  //   const headers = new HttpHeaders({
  //     Authorization: `Bearer ${token}`
  //   });
  //
  //   return this.http.get<RequestResponse<RequestDTO>>(`${this.baseUrl}/${userId}`, {headers});
  // }
  //
  // getRequests(search: string | undefined, page: number, pageSize: number) {
  //   const token = this.authService.getToken();
  //   const headers = new HttpHeaders({
  //     Authorization: `Bearer ${token}`
  //   });
  //
  //   const params = new HttpParams()
  //     .set('search', search || '')
  //     .set('page', page)
  //     .set('pageSize', pageSize);
  //
  //   return this.http.get<RequestResponse<PagedResponse<RequestDTO>>>(
  //     `${this.baseUrl}`,
  //     { headers, params }
  //   );
  // }

  addRequest(user: RequestAddDTO) {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.post(`${this.baseUrl}`, user, { headers });
  }
  //
  // updateRequest(userId: string, user: RequestUpdateDTO) {
  //   const token = this.authService.getToken();
  //
  //   const headers = new HttpHeaders({
  //     Authorization: `Bearer ${token}`
  //   });
  //
  //   return this.http.patch(`${this.baseUrl}${userId}`, user, { headers });
  // }
  //
  // deleteRequest(userId: string) {
  //   const token = this.authService.getToken();
  //
  //   const headers = new HttpHeaders({
  //     Authorization: `Bearer ${token}`
  //   });
  //
  //   return this.http.delete(`${this.baseUrl}${userId}`, {headers});
  // }
}
