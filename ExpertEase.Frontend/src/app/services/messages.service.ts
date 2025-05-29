import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import {PagedResponse, RequestResponse, SpecialistDTO, UserExchangeDTO} from '../models/api.models';
import {AuthService} from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class MessagesService {
  private baseUrl: string = 'http://localhost:5241/api/messages';

  constructor(private authService: AuthService, private http: HttpClient) { }

  getExchange(senderUserId: string) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RequestResponse<UserExchangeDTO>>(
      `${this.baseUrl}/${senderUserId}`,
      { headers }
    );
  }

  getExchanges(search: string | undefined, page: number, pageSize: number) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    const params = new HttpParams()
      .set('search', search || '')
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<RequestResponse<PagedResponse<UserExchangeDTO>>>(
      `${this.baseUrl}`,
      { headers, params }
    );
  }
}
