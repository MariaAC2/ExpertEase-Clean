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
export class SpecialistRequestService {
  private baseUrl = 'http://localhost:5241/api/specialist/requests';
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
  acceptRequest(requestId: string) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.put(`/api/requests/${requestId}/accept`, null, { headers });
  }

  rejectRequest(requestId: string) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.put(`/api/requests/${requestId}/reject`, null, { headers });
  }

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
