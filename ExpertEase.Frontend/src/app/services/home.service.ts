import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders, HttpParams} from "@angular/common/http";
import {PagedResponse, RequestResponse, SpecialistDTO} from "../models/api.models";
import {AuthService} from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class HomeService {
  private baseUrl = 'http://localhost:5241/api/home';
  constructor(private http: HttpClient, private authService: AuthService) { }

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
        { params, headers }
    );
  }
}
