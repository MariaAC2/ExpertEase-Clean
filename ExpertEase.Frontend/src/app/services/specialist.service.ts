import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import { Injectable } from '@angular/core';
import {
  PagedResponse,
  RequestResponse,
  UserAddDTO,
  UserDTO,
  SpecialistAddDTO,
  SpecialistDTO,
  SpecialistUpdateDTO,
  UserUpdateDTO,
  UserDetailsDTO,
  SpecialistPaginationQueryParams,
  CategoryDTO
} from '../models/api.models';
import { jwtDecode } from 'jwt-decode';
import {AuthService, DecodedToken} from './auth.service';
import {GeocodingService} from './geocoding.service';
import {GeolocationService} from './geolocation.service';

@Injectable({ providedIn: 'root' })
export class SpecialistService {
  private readonly baseUrl = 'http://localhost:5241/api/Specialist';

  constructor(
    private readonly http: HttpClient,
    private readonly authService: AuthService,
    private geocodingService: GeocodingService,
    private geolocationService: GeolocationService
  ) {}

  getSpecialists(params: SpecialistPaginationQueryParams) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    let httpParams = new HttpParams()
      .set('page', params.page.toString())
      .set('pageSize', params.pageSize.toString());

    // Add optional parameters only if they have values
    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }

    if (params.categoryId) {
      httpParams = httpParams.set('categoryId', params.categoryId);
    }

    if (params.categoryName) {
      httpParams = httpParams.set('categoryName', params.categoryName);
    }

    if (params.minRating !== undefined && params.minRating !== null) {
      httpParams = httpParams.set('minRating', params.minRating.toString());
    }

    if (params.maxRating !== undefined && params.maxRating !== null) {
      httpParams = httpParams.set('maxRating', params.maxRating.toString());
    }

    if (params.sortByRating) {
      httpParams = httpParams.set('sortByRating', params.sortByRating);
    }

    if (params.experienceRange) {
      httpParams = httpParams.set('experienceRange', params.experienceRange);
    }

    return this.http.get<RequestResponse<PagedResponse<SpecialistDTO>>>(
      `${this.baseUrl}/GetPage`,
      { headers, params: httpParams }
    );
  }

  // Legacy method for backward compatibility
  getSpecialistsLegacy(search: string | undefined, page: number, pageSize: number) {
    return this.getSpecialists({
      page,
      pageSize,
      search: search || undefined
    });
  }

  searchSpecialistsByCategory(categoryId: string, page: number = 1, pageSize: number = 10) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    const params = new HttpParams()
      .set('categoryId', categoryId)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<RequestResponse<PagedResponse<SpecialistDTO>>>(
      `${this.baseUrl}/SearchByCategory`,
      { headers, params }
    );
  }

  searchSpecialistsByRatingRange(minRating: number, maxRating: number, page: number = 1, pageSize: number = 10) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    const params = new HttpParams()
      .set('minRating', minRating.toString())
      .set('maxRating', maxRating.toString())
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<RequestResponse<PagedResponse<SpecialistDTO>>>(
      `${this.baseUrl}/SearchByRatingRange`,
      { headers, params }
    );
  }

  searchSpecialistsByExperienceRange(experienceRange: string, page: number = 1, pageSize: number = 10) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    const params = new HttpParams()
      .set('experienceRange', experienceRange)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<RequestResponse<PagedResponse<SpecialistDTO>>>(
      `${this.baseUrl}/SearchByExperienceRange`,
      { headers, params }
    );
  }

  getTopRatedSpecialists(page: number = 1, pageSize: number = 10) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<RequestResponse<PagedResponse<SpecialistDTO>>>(
      `${this.baseUrl}/GetTopRated`,
      { headers, params }
    );
  }

  getSpecialist(userId: string) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RequestResponse<SpecialistDTO>>(`${this.baseUrl}/GetById/${userId}`, {headers});
  }

  addSpecialist(user: SpecialistAddDTO) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.post(`${this.baseUrl}/Add`, user, { headers });
  }

  updateSpecialist(userId: string, user: SpecialistUpdateDTO) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.patch(`${this.baseUrl}/Update/${userId}`, user, { headers });
  }

  deleteSpecialist(userId: string) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.delete(`${this.baseUrl}/Delete/${userId}`, {headers});
  }
}
