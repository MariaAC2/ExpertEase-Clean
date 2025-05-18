import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders, HttpParams} from "@angular/common/http";
import {PagedResponse, RequestResponse, UserSpecialistDTO} from "../models/api.models";

@Injectable({
  providedIn: 'root'
})
export class HomeService {
  private baseUrl = 'http://localhost:5241/api/home';
  constructor(private http: HttpClient) { }

  getSpecialist(userId: string) {
    return this.http.get<RequestResponse<UserSpecialistDTO>>(`${this.baseUrl}/${userId}`);
  }

  getSpecialists(search: string | undefined, page: number, pageSize: number) {
    const params = new HttpParams()
        .set('search', search || '')
        .set('page', page)
        .set('pageSize', pageSize);

    return this.http.get<RequestResponse<PagedResponse<UserSpecialistDTO>>>(
        `${this.baseUrl}`,
        { params }
    );
  }
}
