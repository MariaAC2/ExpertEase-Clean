import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import {PagedResponse, ReplyAddDTO, ReplyDTO, ReplyUpdateDTO, RequestResponse} from '../models/api.models';
import {AuthService} from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class ReplyService {
  private readonly baseUrl = `http://localhost:5241/api/Reply`;

  constructor(private readonly http: HttpClient, private readonly authService: AuthService) {}

  addReply(requestId: string, reply: ReplyAddDTO){
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.post(
      `${this.baseUrl}/Add/${requestId}`,
      reply,
      { headers }
    );
  }

  updateReply(reply: ReplyUpdateDTO){
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.patch(
      `${this.baseUrl}/Update/${reply.id}`,
      reply,
      { headers }
    );
  }

  acceptReply(replyId: string){
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    console.log(token)
    return this.http.patch(`${this.baseUrl}/Accept/${replyId}`, {}, {headers});
  }

  rejectReply(replyId: string){
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.patch(`${this.baseUrl}/Reject/${replyId}`, {}, {headers});
  }

  cancelReply(replyId: string){
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.patch(
      `${this.baseUrl}/Cancel/${replyId}`,
      { headers }
    );
  }
}
