import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import {
  ConversationDTO, FirestoreConversationItemDTO,
  PagedResponse,
  RequestResponse,
  SpecialistDTO,
  UserConversationDTO
} from '../models/api.models';
import {AuthService} from './auth.service';
import {Observable} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ExchangeService {
  private readonly baseUrl: string = 'http://localhost:5241/api/Conversation';

  constructor(private readonly authService: AuthService, private readonly http: HttpClient) { }

  /**
   * Get paginated list of user conversations
   * Calls: GET /api/Conversation/GetPage
   */
  getExchanges(pagination?: { page: number; pageSize: number }): Observable<RequestResponse<PagedResponse<UserConversationDTO>>> {
    let params = new HttpParams();

    if (pagination) {
      // Use lowercase to match your PaginationQueryParams
      params = params.set('page', pagination.page.toString());
      params = params.set('pageSize', pagination.pageSize.toString());
    }

    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    // Use the full URL with baseUrl
    return this.http.get<RequestResponse<PagedResponse<UserConversationDTO>>>(
      `${this.baseUrl}/GetPage`,
      { params, headers }
    );
  }

  /**
   * Get paginated conversation messages
   * Calls: GET /api/Conversation/GetById/{userId}
   */
  getExchange(
    userId: string,
    pagination?: { page: number; pageSize: number }
  ): Observable<RequestResponse<PagedResponse<FirestoreConversationItemDTO>>> {
    let params = new HttpParams();

    if (pagination) {
      // Use lowercase to match your PaginationQueryParams
      params = params.set('page', pagination.page.toString());
      params = params.set('pageSize', pagination.pageSize.toString());
    }

    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    // Use the full URL with baseUrl
    return this.http.get<RequestResponse<PagedResponse<FirestoreConversationItemDTO>>>(
      `${this.baseUrl}/GetById/${userId}`,
      { params, headers }
    );
  }
}
