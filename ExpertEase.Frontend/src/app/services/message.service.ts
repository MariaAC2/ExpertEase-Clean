import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {AuthService} from './auth.service';
import {MessageAddDTO, RequestResponse} from '../models/api.models';
import {Observable} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private readonly baseUrl = 'http://localhost:5241/api/Message';
  constructor(private readonly http: HttpClient, private readonly authService: AuthService) { }

  sendMessage(conversationId: string, message: MessageAddDTO): Observable<RequestResponse<any>> {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.post(`${this.baseUrl}/Add/${conversationId}`, message, { headers });
  }
}
