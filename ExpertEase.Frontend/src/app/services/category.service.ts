import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import {AuthService} from './auth.service';
import {
  CategoryAddDTO,
  CategoryAdminDTO, CategoryDTO, CategoryUpdateDTO,
  PagedResponse,
  RequestResponse
} from '../models/api.models';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private readonly baseUrl = 'http://localhost:5241/api/Category';
  constructor(private http: HttpClient, private readonly authService: AuthService) { }

  addCategory(dto: CategoryAddDTO): Observable<Object> {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.post(`${this.baseUrl}/Add`, dto, { headers });
  }

  getCategoryById(id: string): Observable<RequestResponse<CategoryAdminDTO>> {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.get<RequestResponse<CategoryAdminDTO>>(`${this.baseUrl}/GetById/${id}`, { headers });
  }

  getAllCategories(search?: string): Observable<RequestResponse<CategoryDTO[]>> {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<RequestResponse<CategoryDTO[]>>(`${this.baseUrl}/GetAll`, { headers, params });
  }

  getCategoriesForSpecialist(search?: string): Observable<RequestResponse<CategoryDTO[]>> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<RequestResponse<CategoryDTO[]>>(`${this.baseUrl}/GetAllForSpecialist`, { params });
  }

  getCategoriesAdminPage(search: string | undefined, page: number, pageSize: number): Observable<RequestResponse<PagedResponse<CategoryAdminDTO>>> {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    const params = new HttpParams()
      .set('search', search || '')
      .set('page', page)
      .set('pageSize', pageSize);
    return this.http.get<RequestResponse<PagedResponse<CategoryAdminDTO>>>(`${this.baseUrl}/GetPageForAdmin`, { headers, params });
  }

  updateCategory(dto: CategoryUpdateDTO): Observable<Object> {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.patch(`${this.baseUrl}/Update/${dto.id}`, dto, { headers });
  }

  deleteCategory(id: string): Observable<Object> {
      const token = this.authService.getToken();
      const headers = new HttpHeaders({
        Authorization: `Bearer ${token}`
      });
    return this.http.delete(`${this.baseUrl}/Delete/${id}`, { headers });
  }

  // getCategory(id: string) {
  //   const token = this.authService.getToken();
  //   const headers = new HttpHeaders({
  //     Authorization: `Bearer ${token}`
  //   });
  //
  //   return this.http.get<RequestResponse<CategoryAdminDTO>>(`${this.baseUrl}/${id}`, {headers});
  // }
  //
  // getCategoriesAdmin(search: string | undefined, page: number, pageSize: number) {
  //   const token = this.authService.getToken();
  //   const headers = new HttpHeaders({
  //     Authorization: `Bearer ${token}`
  //   });
  //
  //   const params = new HttpParams()
  //     .set('search', search || '')
  //     .set('page', page)
  //     .set('pageSize', pageSize);
  //
  //   return this.http.get<RequestResponse<PagedResponse<CategoryAdminDTO>>>(
  //     `${this.baseUrl}`,
  //     { headers, params }
  //   );
  // }
  //
  // addCategory(user: CategoryAddDTO) {
  //   const token = this.authService.getToken();
  //
  //   const headers = new HttpHeaders({
  //     Authorization: `Bearer ${token}`
  //   });
  //
  //   return this.http.post(`${this.baseUrl}`, user, { headers });
  // }
  //
  // updateCategory(id: string, user: CategoryUpdateDTO) {
  //   const token = this.authService.getToken();
  //
  //   const headers = new HttpHeaders({
  //     Authorization: `Bearer ${token}`
  //   });
  //
  //   return this.http.patch(`${this.baseUrl}/${id}`, user, { headers });
  // }
  //
  // deleteCategory(id: string) {
  //   const token = this.authService.getToken();
  //
  //   const headers = new HttpHeaders({
  //     Authorization: `Bearer ${token}`
  //   });
  //
  //   return this.http.delete(`${this.baseUrl}/${id}`, {headers});
  // }
}
