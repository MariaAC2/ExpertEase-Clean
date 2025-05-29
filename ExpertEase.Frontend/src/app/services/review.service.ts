import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import {AuthService} from './auth.service';
import {PagedResponse, ReplyDTO, RequestResponse, ReviewAddDTO, ReviewDTO} from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private baseUrl = `http://localhost:5241/api/`;
  constructor(private http: HttpClient, private authService: AuthService) { }

  addReview(serviceTaskId: string, review: ReviewAddDTO) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.post(`${this.baseUrl}task/${serviceTaskId}/confirm/reviews`, review, { headers });
  }

  getReviews(search: string, page: number, pageSize: number) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    const params = new HttpParams()
      .set('search', search)
      .set('page', page)
      .set('pageSize', pageSize);
    return this.http.get<RequestResponse<PagedResponse<ReviewDTO>>>(`${this.baseUrl}profile/reviews`, { params, headers });
  }

  getReview(reviewId: string) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RequestResponse<ReviewDTO>>(`${this.baseUrl}profile/reviews/${reviewId}`, { headers });
  }
}
