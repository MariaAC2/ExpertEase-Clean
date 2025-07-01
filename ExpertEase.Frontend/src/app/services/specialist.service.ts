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
  PaginationSearchQueryParams,
  SpecialistFilterParams,
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

  // Main method with separate search and filter parameters
  getSpecialists(searchParams: PaginationSearchQueryParams, filterParams?: SpecialistFilterParams) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    let httpParams = new HttpParams()
      .set('page', searchParams.page.toString())
      .set('pageSize', searchParams.pageSize.toString());

    // Add search parameter
    if (searchParams.search) {
      httpParams = httpParams.set('search', searchParams.search);
    }

    // Add filter parameters if provided - flatten them to match backend expectation
    if (filterParams) {
      if (filterParams.categoryIds && filterParams.categoryIds.length > 0) {
        // Send multiple category IDs as separate parameters
        filterParams.categoryIds.forEach((categoryId) => {
          httpParams = httpParams.append('filter.categoryIds', categoryId);
        });
      }

      if (filterParams.minRating !== undefined && filterParams.minRating !== null) {
        httpParams = httpParams.set('filter.minRating', filterParams.minRating.toString());
      }

      if (filterParams.experienceRange) {
        httpParams = httpParams.set('filter.experienceRange', filterParams.experienceRange);
      }

      if (filterParams.sortByRating) {
        httpParams = httpParams.set('filter.sortByRating', filterParams.sortByRating);
      }
    }

    return this.http.get<RequestResponse<PagedResponse<SpecialistDTO>>>(
      `${this.baseUrl}/GetPage`,
      { headers, params: httpParams }
    );
  }

  // Legacy method for backward compatibility - overloaded signatures
  getSpecialistsLegacy(search: string | undefined, page: number, pageSize: number) {
    return this.getSpecialists({
      page,
      pageSize,
      search: search || undefined
    });
  }

  // Method with combined parameters (for backward compatibility with existing code)
  getSpecialistsCombined(params: SpecialistPaginationQueryParams) {
    return this.getSpecialists({
      page: params.page,
      pageSize: params.pageSize,
      search: params.search
    }, params.filters);
  }

  searchSpecialistsByCategory(categoryIds: string[], page: number = 1, pageSize: number = 10) {
    return this.getSpecialists(
      { page, pageSize },
      { categoryIds }
    );
  }

  searchSpecialistsByRating(minRating: number, page: number = 1, pageSize: number = 10) {
    return this.getSpecialists(
      { page, pageSize },
      { minRating }
    );
  }

  searchSpecialistsByExperienceRange(experienceRange: string, page: number = 1, pageSize: number = 10) {
    return this.getSpecialists(
      { page, pageSize },
      { experienceRange }
    );
  }

  getTopRatedSpecialists(page: number = 1, pageSize: number = 10) {
    return this.getSpecialists(
      { page, pageSize },
      { sortByRating: 'desc', minRating: 4.5 }
    );
  }

  // Advanced filtering method
  searchSpecialistsAdvanced(
    searchParams: PaginationSearchQueryParams,
    categoryIds?: string[],
    minRating?: number,
    experienceRange?: string,
    sortByRating?: 'asc' | 'desc'
  ) {
    const filterParams: SpecialistFilterParams = {};

    if (categoryIds && categoryIds.length > 0) {
      filterParams.categoryIds = categoryIds;
    }

    if (minRating) {
      filterParams.minRating = minRating;
    }

    if (experienceRange) {
      filterParams.experienceRange = experienceRange;
    }

    if (sortByRating) {
      filterParams.sortByRating = sortByRating;
    }

    return this.getSpecialists(searchParams, filterParams);
  }

  // Quick filter methods
  getHighRatedSpecialists(page: number = 1, pageSize: number = 10) {
    return this.getSpecialists(
      { page, pageSize },
      { minRating: 4, sortByRating: 'desc' }
    );
  }

  getExperiencedSpecialists(page: number = 1, pageSize: number = 10) {
    return this.getSpecialists(
      { page, pageSize },
      { experienceRange: '7-10', sortByRating: 'desc' }
    );
  }

  getSpecialistsByCategories(categoryIds: string[], page: number = 1, pageSize: number = 10) {
    return this.getSpecialists(
      { page, pageSize },
      { categoryIds, sortByRating: 'desc' }
    );
  }

  // Search with text and filters combined
  searchAndFilter(
    searchText: string,
    filters: SpecialistFilterParams,
    page: number = 1,
    pageSize: number = 10
  ) {
    return this.getSpecialists(
      { page, pageSize, search: searchText },
      filters
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
