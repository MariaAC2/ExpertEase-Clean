import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {AuthService} from './auth.service';
import {MessageAddDTO} from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private readonly baseUrl = 'http://localhost:5241/api/Message';
  constructor(private readonly http: HttpClient, private readonly authService: AuthService) { }

  sendMessage(message: MessageAddDTO) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.post(`${this.baseUrl}/Add`, message, { headers });
  }
}
