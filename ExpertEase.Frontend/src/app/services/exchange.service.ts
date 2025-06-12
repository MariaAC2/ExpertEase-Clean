import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import {
  ConversationDTO,
  PagedResponse,
  RequestResponse,
  SpecialistDTO,
  UserConversationDTO
} from '../models/api.models';
import {AuthService} from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class ExchangeService {
  private readonly baseUrl: string = 'http://localhost:5241/api/Conversation';

  constructor(private readonly authService: AuthService, private readonly http: HttpClient) { }

  getExchange(senderUserId: string | undefined) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RequestResponse<ConversationDTO>>(
      `${this.baseUrl}/GetById/${senderUserId}`,
      { headers }
    );
  }

  getExchanges() {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RequestResponse<UserConversationDTO[]>>(
      `${this.baseUrl}/GetPage`,
      { headers }
    );
  }
}
