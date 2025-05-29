import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import {PagedResponse, ReplyDTO, RequestResponse} from '../models/api.models';
import {AuthService} from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class UserReplyService {
  private readonly baseUrl = `http://localhost:5241/api/user/requests`;

  constructor(private http: HttpClient, private authService: AuthService) {}

  getReply(requestId: string, replyId: string) {
    const headers = this.authService.getAuthHeaders();
    return this.http.get<RequestResponse<ReplyDTO>>(`${this.baseUrl}/${requestId}/replies/${replyId}`, { headers });
  }

  getReplies(requestId: string, search: string = '', page: number, pageSize: number){
    const headers = this.authService.getAuthHeaders();
    let params = new HttpParams()
      .set('search', search)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<RequestResponse<PagedResponse<ReplyDTO>>>(`${this.baseUrl}/${requestId}/replies`, { params, headers });
  }

  acceptReply(requestId: string, replyId: string){
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    console.log(token)
    return this.http.patch(`${this.baseUrl}/${requestId}/replies/${replyId}/accept`, {}, {headers});
  }

  rejectReply(requestId: string, replyId: string){
    const headers = this.authService.getAuthHeaders();
    return this.http.patch(`${this.baseUrl}/${requestId}/replies/${replyId}/reject`, {}, {headers});
  }
}
