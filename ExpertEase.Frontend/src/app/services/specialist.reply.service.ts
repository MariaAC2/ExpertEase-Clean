import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import {AuthService} from './auth.service';
import {
  AdminUserUpdateDTO,
  PagedResponse, ReplyAddDTO, ReplyDTO, ReplyUpdateDTO, RequestAddDTO,
  RequestDTO,
  RequestResponse,
  UserAddDTO,
  UserDTO
} from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class SpecialistReplyService {
  private baseUrl = 'http://localhost:5241/api/specialist/requests';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  }

  addReply(requestId: string, reply: ReplyAddDTO){
    const headers = this.getAuthHeaders();
    return this.http.post(
      `${this.baseUrl}/${requestId}/replies`,
      reply,
      { headers }
    );
  }

  getReplyById(requestId: string, replyId: string){
    const headers = this.getAuthHeaders();
    return this.http.get<RequestResponse<ReplyDTO>>(
      `${this.baseUrl}/${requestId}/replies/${replyId}`,
      { headers }
    );
  }

  getReplies(requestId: string, search: string, page: number, pageSize: number) {
    const headers = this.getAuthHeaders();
    const params = new HttpParams()
      .set('search', search)
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<RequestResponse<PagedResponse<ReplyDTO>>>(
      `${this.baseUrl}/${requestId}/replies`,
      { headers, params }
    );
  }

  updateReply(requestId: string, reply: ReplyUpdateDTO){
    const headers = this.getAuthHeaders();
    return this.http.patch(
      `${this.baseUrl}/${requestId}/replies/${reply.id}`,
      reply,
      { headers }
    );
  }

  cancelReply(requestId: string, replyId: string){
    const headers = this.getAuthHeaders();
    return this.http.patch(
      `${this.baseUrl}/${requestId}/replies/${replyId}/cancel`,
      { headers }
    );
  }
}
