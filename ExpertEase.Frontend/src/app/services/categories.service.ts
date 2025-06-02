import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import {AuthService} from './auth.service';
import {
  CategoryAddDTO,
  CategoryAdminDTO, CategoryUpdateDTO,
  PagedResponse,
  RequestResponse
} from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class CategoriesService {
  private baseUrl = 'http://localhost:5241/api/admin/categories';
  constructor(private http: HttpClient, private authService: AuthService) { }

  getCategory(id: string) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RequestResponse<CategoryAdminDTO>>(`${this.baseUrl}/${id}`, {headers});
  }

  getCategoriesAdmin(search: string | undefined, page: number, pageSize: number) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    const params = new HttpParams()
      .set('search', search || '')
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<RequestResponse<PagedResponse<CategoryAdminDTO>>>(
      `${this.baseUrl}`,
      { headers, params }
    );
  }

  addCategory(user: CategoryAddDTO) {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.post(`${this.baseUrl}`, user, { headers });
  }

  updateCategory(id: string, user: CategoryUpdateDTO) {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.patch(`${this.baseUrl}/${id}`, user, { headers });
  }

  deleteCategory(id: string) {
    const token = this.authService.getToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.delete(`${this.baseUrl}/${id}`, {headers});
  }
}
