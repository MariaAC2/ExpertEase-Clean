import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import {AuthService} from './auth.service';
import {PagedResponse, RequestResponse, ReviewAddDTO, ReviewDTO} from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private readonly baseUrl = `http://localhost:5241/api/Review`;
  constructor(private readonly http: HttpClient, private readonly authService: AuthService) { }

  addReview(serviceTaskId: string, review: ReviewAddDTO) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.post(`${this.baseUrl}/Add/${serviceTaskId}`, review, { headers });
  }

  getReviews(page: number, pageSize: number) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    return this.http.get<RequestResponse<PagedResponse<ReviewDTO>>>(`${this.baseUrl}/GetPage`, { params, headers });
  }

  getReview(reviewId: string) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RequestResponse<ReviewDTO>>(`${this.baseUrl}/GetById/${reviewId}`, { headers });
  }
}
