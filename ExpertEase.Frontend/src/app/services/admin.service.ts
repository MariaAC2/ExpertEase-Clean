import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {Observable, tap} from 'rxjs';
import {IUserDTO} from './api.generated';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private baseUrl = 'http://localhost:5241/api';

  constructor(private http: HttpClient) {}

  getUsers(): Observable<IUserDTO[]> {
    return this.http.get<IUserDTO[]>(`${this.baseUrl}/admin/users`);
  }

  // loginUser(data: any) {
  //   console.log(data);
  //   return this.http.post(`${this.baseUrl}/auth/login`, data).pipe(
  //     tap((result: any) => {
  //       const token = result.response?.token;
  //
  //       if (token) {
  //         localStorage.setItem('access_token', token);
  //       }
  //     })
  //   );
  // }
}
